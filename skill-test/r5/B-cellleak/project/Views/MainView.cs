using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CellLeakTester.Data;

namespace CellLeakTester.Views
{
    /// <summary>셀 누설 검사 메인 화면: 현재 셀·LOT → 판정판 + 누설량·기준값 → 오늘 집계 → 검사 이력.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private readonly IMainViewSource _source;
        private readonly ApcTextBox _barcode = new ApcTextBox { ReadOnly = true, Width = S(200) };
        private readonly ApcTextBox _lot = new ApcTextBox { ReadOnly = true, Width = S(200) };
        private readonly ApcTextBox _model = new ApcTextBox { ReadOnly = true, Width = S(160) };
        private readonly ApcTextBox _worker = new ApcTextBox { ReadOnly = true, Width = S(120) };
        private readonly ApcJudgePanel _judge = new ApcJudgePanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(12), 0) };
        private readonly ApcValueCard _leak = Card("누설량", "Pa/s");
        private readonly ApcValueCard _limit = Card("기준값(상한)", "Pa/s");
        private readonly ApcValueCard _total = Card("오늘 생산", "EA");
        private readonly ApcValueCard _ng = Card("NG", "EA");
        private readonly ApcValueCard _yield = Card("수율", "%");
        private readonly ApcGrid _grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("셀 누설 검사")
        {
            _source = source;

            var lotButton = Button("LOT 입력", ApcButtonVariant.Primary, 120);
            lotButton.Click += (s, e) => EditLot();
            var adminButton = Button("관리자", ApcButtonVariant.Normal, 120);
            adminButton.Click += (s, e) => AskAdmin();

            _grid.Columns.Add(new ApcLampColumn { HeaderText = "상태" });
            _grid.Columns.Add("time", "시각");
            _grid.Columns.Add("barcode", "바코드");
            _grid.Columns.Add("leak", "누설량(Pa/s)");
            _grid.Columns.Add("result", "판정");
            foreach (DataGridViewColumn c in _grid.Columns) if (!(c is ApcLampColumn)) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Top 도킹은 나중에 Add한 것이 위 → 이력(Fill) 먼저, 아래 줄부터
            Body.Controls.Add(_grid);
            Body.Controls.Add(Columns(S(110), new Control[] { _total, _ng, _yield }));
            Body.Controls.Add(Columns(S(200), new Control[] { _judge, _leak, _limit }, new[] { 50f, 25f, 25f }));
            Body.Controls.Add(Row(Caption("현재 셀"), _barcode, Caption("LOT"), _lot, Caption("모델"), _model, Caption("작업자"), _worker, lotButton, adminButton));
            Reload();
        }

        private void Reload()
        {
            var lot = _source.GetLot();
            _lot.Text = lot.LotNumber; _model.Text = lot.Model; _worker.Text = lot.Worker;

            var history = _source.GetHistory();
            _grid.Rows.Clear();
            foreach (var r in history)
            {
                bool ng = LeakJudge.IsNg(r.Leak);
                int row = _grid.Rows.Add(ng ? StatusKind.Alarm : StatusKind.Run, r.Time.ToString("HH:mm:ss"), r.Barcode, LeakJudge.Format(r.Leak), ng ? "NG" : "OK");
                if (!ng) continue;
                foreach (int col in new[] { 3, 4 })
                {
                    var style = _grid.Rows[row].Cells[col].Style;
                    style.ForeColor = ApcTheme.Status(StatusKind.Alarm);
                    style.Font = ApcTheme.GetFont(FontStyle.Bold);
                }
            }

            if (history.Count > 0)
            {
                var cur = history[0];
                bool ng = LeakJudge.IsNg(cur.Leak);
                _barcode.Text = cur.Barcode;
                _judge.Text = ng ? "NG" : "OK";
                _judge.Status = ng ? StatusKind.Alarm : StatusKind.Run;
                _judge.SubText = ng ? "누설량 " + LeakJudge.Format(cur.Leak) + " Pa/s > 상한 " + LeakJudge.LimitText : cur.Barcode;
                _leak.Value = LeakJudge.Format(cur.Leak);
                _leak.Status = ng ? StatusKind.Alarm : StatusKind.Run;
                _leak.Footer = "검사 " + cur.Time.ToString("HH:mm:ss");
                Header.Status = ng ? StatusKind.Alarm : StatusKind.Run;
                Header.StatusText = ng ? "NG 발생" : "가동중";
            }
            else
            {
                _judge.Text = "READY"; _judge.Status = StatusKind.Stop; _judge.SubText = "셀을 투입하세요";
                _leak.Value = ""; _leak.Status = StatusKind.Stop;
                Header.Status = StatusKind.Stop; Header.StatusText = "대기";
            }

            _limit.Value = LeakJudge.LimitText;
            _limit.Footer = LeakJudge.LimitText + " 이하 OK";
            _limit.Status = StatusKind.Run;

            int produced = _source.GetProducedCount(), ngCount = _source.GetNgCount();
            _total.Value = produced.ToString("#,0"); _total.Status = StatusKind.Run;
            _ng.Value = ngCount.ToString("#,0"); _ng.Status = ngCount > 0 ? StatusKind.Alarm : StatusKind.Run;
            _yield.Value = produced == 0 ? "" : (100.0 * (produced - ngCount) / produced).ToString("0.0", CultureInfo.InvariantCulture);
            _yield.Footer = "OK / 생산"; _yield.Status = produced == 0 ? StatusKind.Stop : StatusKind.Run;
        }

        private void EditLot()
        {
            var cur = _source.GetLot();
            using (var dlg = new LotEntryDialog(cur.LotNumber, cur.Model, cur.Worker))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                _source.SaveLot(new LotInfo(dlg.LotNumber, dlg.Model, dlg.Worker));
                Reload();
            }
        }

        private void AskAdmin()
        {
            using (var dlg = new AdminPasswordDialog())
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                if (!_source.VerifyAdminPassword(dlg.Password)) ApcDialog.Alert(FindForm(), "비밀번호가 맞지 않습니다.");
                // 확인되면 관리자 설정 화면으로 진입(이 껍데기에는 설정 화면 없음)
            }
        }

        private static ApcValueCard Card(string title, string unit)
        {
            return new ApcValueCard { Title = title, Unit = unit, Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(12), 0) };
        }
    }
}
