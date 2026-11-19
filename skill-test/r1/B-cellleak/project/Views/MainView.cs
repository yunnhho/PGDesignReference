using System;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CellLeakTester.Data;

namespace CellLeakTester.Views
{
    /// <summary>
    /// 셀 누설 검사 메인 화면: 도구 줄(스캔·LOT 입력·관리자) → 현재 셀·LOT → 판정판 + 누설량 + 기준값 → 금일 집계 → 최근 검사 이력.
    /// 데이터는 IMainViewSource에서만 받는다(기본: 샘플 데이터).
    /// </summary>
    internal sealed class MainView : ApcTemplateView
    {
        private readonly IMainViewSource _source;
        private readonly ApcScanBox _scan = new ApcScanBox();
        private readonly ApcValueCard _barcode = Card("현재 셀 바코드");
        private readonly ApcValueCard _lot = Card("현재 LOT");
        private readonly ApcJudgePanel _judge = new ApcJudgePanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(12), 0) };
        private readonly ApcValueCard _leak = Card("누설량");
        private readonly ApcValueCard _limit = Card("기준값 (상한)");
        private readonly ApcValueCard _total = Card("금일 생산");
        private readonly ApcValueCard _ok = Card("OK");
        private readonly ApcValueCard _ng = Card("NG");
        private readonly ApcValueCard _yield = Card("수율");
        private readonly ApcGrid _grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("셀 누설 검사기")
        {
            _source = source;
            _scan.Width = S(300);
            _scan.Scanned += (s, e) => { Apply(_source.Inspect(e.Code)); e.Accepted = true; };

            var lotButton = Button("LOT 입력", ApcButtonVariant.Normal, 130);
            lotButton.Click += (s, e) => EditLot();
            var adminButton = Button("관리자 설정", ApcButtonVariant.Normal, 130);
            adminButton.Click += (s, e) => AdminCheck();

            _grid.Columns.Add(new ApcLampColumn());
            _grid.Columns.Add("time", "시각");
            _grid.Columns.Add("barcode", "바코드");
            _grid.Columns.Add("leak", "누설량(Pa/s)");
            _grid.Columns.Add("result", "판정");
            foreach (DataGridViewColumn c in _grid.Columns) if (!(c is ApcLampColumn)) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Top 도킹은 나중에 Add한 것이 위 → 이력(Fill) 먼저, 아래 줄부터
            var title = SectionTitle("최근 검사 이력");
            title.Dock = DockStyle.Top;
            Body.Controls.Add(_grid);
            Body.Controls.Add(title);
            Body.Controls.Add(Columns(S(130), new Control[] { _total, _ok, _ng, _yield }));
            Body.Controls.Add(Columns(S(220), new Control[] { _judge, _leak, _limit }, new[] { 50f, 25f, 25f }));
            Body.Controls.Add(Columns(S(110), new Control[] { _barcode, _lot }));
            Body.Controls.Add(Row(Caption("바코드"), _scan, lotButton, adminButton));
            Apply(_source.GetHistory().Count > 0 ? _source.GetHistory()[0] : null);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var form = FindForm();
            if (form != null) _scan.Attach(form);   // 생성자 시점엔 폼이 없다
        }


        private void Apply(InspectionRecord current)
        {
            double limit = _source.LeakLimit;
            string limitText = Num(limit);
            var lot = _source.GetLot();
            _lot.Value = lot.Lot;
            _lot.Footer = "모델 " + lot.Model + " · 작업자 " + lot.Worker;

            _limit.Value = limitText; _limit.Unit = "Pa/s"; _limit.Footer = limitText + " 이하 OK";
            _leak.Unit = "Pa/s"; _leak.Footer = "기준 상한 " + limitText + " Pa/s";

            if (current == null)
            {
                _judge.Text = "READY"; _judge.Status = StatusKind.Stop; _judge.SubText = "셀 바코드를 스캔하세요";
                Header.Status = StatusKind.Stop; Header.StatusText = "대기";
            }
            else
            {
                var kind = current.Ok ? StatusKind.Run : StatusKind.Alarm;
                _barcode.Value = current.Barcode;
                _barcode.Footer = "검사 " + current.Time.ToString("HH:mm:ss");
                _leak.Value = Num(current.Leak); _leak.Status = kind;
                _judge.Text = current.Ok ? "OK" : "NG"; _judge.Status = kind;
                _judge.SubText = current.Barcode;
                Header.Status = kind;
                Header.StatusText = current.Ok ? "정상" : "NG 발생";
                if (!current.Ok) Alarm.Push(current.Barcode + ": 누설량 " + Num(current.Leak) + " Pa/s > 상한 " + limitText);
            }

            var today = _source.GetToday();
            _total.Value = today.Total.ToString("#,0", CultureInfo.InvariantCulture); _total.Unit = "EA";
            _ok.Value = (today.Total - today.Ng).ToString("#,0", CultureInfo.InvariantCulture); _ok.Unit = "EA";
            _ng.Value = today.Ng.ToString("#,0", CultureInfo.InvariantCulture); _ng.Unit = "EA";
            _ng.Status = today.Ng > 0 ? StatusKind.Alarm : StatusKind.Run;
            _yield.Value = today.Total == 0 ? "" : today.YieldPercent.ToString("0.0", CultureInfo.InvariantCulture); _yield.Unit = "%";
            _yield.Footer = "OK / 생산";

            _grid.Rows.Clear();
            foreach (var r in _source.GetHistory())
                _grid.Rows.Add(r.Ok ? StatusKind.Run : StatusKind.Alarm, r.Time.ToString("HH:mm:ss"), r.Barcode, Num(r.Leak), r.Ok ? "OK" : "NG");
        }

        private void EditLot()
        {
            using (var dlg = new LotInputDialog(_source.GetLot()))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                _source.SetLot(dlg.Result);
                Apply(_source.GetHistory().Count > 0 ? _source.GetHistory()[0] : null);
            }
        }

        private void AdminCheck()
        {
            using (var dlg = new AdminPasswordDialog())
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                // 실제 비밀번호 확인·설정 화면 진입은 데이터 자리(인증 서비스)에서. 여기서는 입력 여부만 확인한다.
                ApcDialog.Alert(FindForm(), dlg.Password.Length == 0 ? "비밀번호를 입력하세요." : "관리자 확인을 요청했습니다.");
            }
        }

        private static string Num(double v) { return v.ToString("0.0", CultureInfo.InvariantCulture); }

        private static ApcValueCard Card(string title)
        {
            return new ApcValueCard { Title = title, Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(12), 0) };
        }
    }
}
