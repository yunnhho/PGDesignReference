using System;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CellLeakTester.Data;

namespace CellLeakTester.Views
{
    /// <summary>메인 화면(JudgeView 골격): 현재 셀·LOT 줄 → 판정판 + 누설량 + 기준값 → 금일 집계 → 최근 검사 이력.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private readonly IMainViewSource _source;
        private readonly ApcScanBox _scan = new ApcScanBox();
        private readonly ApcTextBox _lot = new ApcTextBox { ReadOnly = true };
        private readonly ApcTextBox _model = new ApcTextBox { ReadOnly = true };
        private readonly ApcTextBox _worker = new ApcTextBox { ReadOnly = true };
        private readonly ApcJudgePanel _judge = new ApcJudgePanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(12), 0) };
        private readonly ApcValueCard _leak = Card("누설량", "Pa/s");
        private readonly ApcValueCard _limit = Card("기준값 (상한)", "Pa/s");
        private readonly ApcValueCard _total = Card("오늘 생산", "EA");
        private readonly ApcValueCard _ng = Card("오늘 NG", "EA");
        private readonly ApcValueCard _yield = Card("수율", "%");
        private readonly ApcGrid _grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("셀 누설 검사기")
        {
            _source = source;
            _scan.Width = S(260);
            _lot.Width = S(200);
            _model.Width = S(160);
            _worker.Width = S(120);
            _scan.Scanned += (s, e) => { Show(_source.Inspect(e.Code)); e.Accepted = true; };

            _limit.Value = LeakRules.Format(LeakRules.UpperLimit);
            _limit.Footer = LeakRules.LimitText;

            _grid.Columns.Add(new ApcLampColumn());
            _grid.Columns.Add("time", "시각");
            _grid.Columns.Add("barcode", "바코드");
            _grid.Columns.Add("leak", "누설량(Pa/s)");
            _grid.Columns.Add("result", "판정");
            foreach (DataGridViewColumn c in _grid.Columns) if (!(c is ApcLampColumn)) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            var history = _source.GetHistory();
            for (int i = 0; i < history.Count; i++) _grid.Rows.Add(RowValues(history[i]));

            var lotBtn = Button("LOT 입력", ApcButtonVariant.Normal, 120);
            lotBtn.Click += (s, e) => EditLot();
            var adminBtn = Button("관리자", ApcButtonVariant.Normal, 110);
            adminBtn.Click += (s, e) => Admin();

            Body.Controls.Add(_grid);
            Body.Controls.Add(Columns(S(130), new Control[] { _total, _ng, _yield }));
            Body.Controls.Add(Columns(S(220), new Control[] { _judge, _leak, _limit }, new[] { 50f, 25f, 25f }));
            Body.Controls.Add(Row(Caption("셀 바코드"), _scan, Caption("LOT"), _lot, Caption("모델"), _model, Caption("작업자"), _worker, lotBtn, adminBtn));

            ShowLot();
            ShowCounts();
            if (history.Count > 0) Show(history[0], false);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var form = FindForm();
            if (form != null) _scan.Attach(form);   // 생성자 시점엔 폼이 없다
        }

        private static object[] RowValues(InspectionRecord r)
        {
            return new object[] { r.Ok ? StatusKind.Run : StatusKind.Alarm, r.Time.ToString("HH:mm:ss"), r.Barcode,
                LeakRules.Format(r.Leak), r.Ok ? "OK" : "NG · 상한 초과" };
        }

        private void Show(InspectionRecord r) { Show(r, true); }

        private void Show(InspectionRecord r, bool isNew)
        {
            var kind = r.Ok ? StatusKind.Run : StatusKind.Alarm;
            _scan.Text = r.Barcode;
            _judge.Text = r.Ok ? "OK" : "NG";
            _judge.Status = kind;
            _judge.SubText = r.Ok ? r.Barcode : r.Barcode + " · " + LeakRules.NgReason(r.Leak);
            _leak.Value = LeakRules.Format(r.Leak);
            _leak.Status = kind;
            _leak.Footer = "측정 " + r.Time.ToString("HH:mm:ss");
            Header.Status = kind;
            Header.StatusText = r.Ok ? "가동중" : "NG 발생";
            if (!isNew) return;
            _grid.Rows.Insert(0, RowValues(r));
            if (!r.Ok) Alarm.Push(r.Barcode + ": " + LeakRules.NgReason(r.Leak));
            ShowCounts();
        }

        private void ShowLot()
        {
            var lot = _source.GetLot();
            _lot.Text = lot.Lot;
            _model.Text = lot.Model;
            _worker.Text = lot.Worker;
        }

        private void ShowCounts()
        {
            var d = _source.GetDaily();
            _total.Value = d.Total.ToString("#,0", CultureInfo.InvariantCulture);
            _ng.Value = d.Ng.ToString("#,0", CultureInfo.InvariantCulture);
            _ng.Status = d.Ng > 0 ? StatusKind.Alarm : StatusKind.Run;
            _yield.Value = d.Total == 0 ? "" : (100.0 * (d.Total - d.Ng) / d.Total).ToString("0.0", CultureInfo.InvariantCulture);
            _yield.Footer = "OK / 생산";
        }

        private void EditLot()
        {
            using (var dlg = new LotDialog(_source.GetLot()))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                _source.SaveLot(dlg.Result);
                ShowLot();
            }
        }

        private void Admin()
        {
            using (var dlg = new AdminPasswordDialog())
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                if (!_source.CheckAdminPassword(dlg.Password)) { ApcDialog.Alert(FindForm(), "비밀번호가 올바르지 않습니다."); return; }
                ApcDialog.Alert(FindForm(), "관리자 설정 화면은 아직 연결되지 않았습니다.");   // TODO: 설정 화면 연결
            }
        }

        private static ApcValueCard Card(string title, string unit)
        {
            return new ApcValueCard { Title = title, Unit = unit, Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(12), 0) };
        }
    }
}
