using System;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CellLeakTester.Data;

namespace CellLeakTester.Views
{
    /// <summary>메인 화면. 데이터는 IMainViewSource에서만 받는다(기본: 샘플 데이터).</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const int HistoryRows = 17;

        private readonly IMainViewSource _source;
        private readonly ApcTextBox _barcode = new ApcTextBox { ReadOnly = true };
        private readonly ApcTextBox _lot = new ApcTextBox { ReadOnly = true };
        private readonly ApcJudgePanel _judge = new ApcJudgePanel { Dock = DockStyle.Fill };
        private readonly ApcValueCard _leak = Card("누설량", LeakJudgement.Unit, "");
        private readonly ApcValueCard _limit = Card("기준값 (상한)", LeakJudgement.Unit, LeakJudgement.LimitText);
        private readonly ApcValueCard _total = Card("오늘 생산", "EA", "");
        private readonly ApcValueCard _ng = Card("오늘 NG", "EA", "");
        private readonly ApcValueCard _yield = Card("수율", "%", "OK / 생산");
        private readonly ApcGrid _grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("셀 누설 검사")
        {
            _source = source;
            _barcode.Width = S(240);
            _lot.Width = S(240);
            _judge.Margin = new Padding(0, 0, S(12), 0);
            _limit.Value = LeakJudgement.Format(LeakJudgement.UpperLimit);
            _limit.Status = StatusKind.Run;

            BuildGrid();

            var lotButton = Button("LOT 입력", ApcButtonVariant.Normal, 130);
            lotButton.Click += (s, e) => EditLot();
            var adminButton = Button("관리자", ApcButtonVariant.Normal, 130);
            adminButton.Click += (s, e) => EnterAdmin();

            // Top 도킹은 나중에 Add한 것이 위 → 이력(Fill) 먼저, 그다음 아래 줄부터
            Body.Controls.Add(_grid);
            Body.Controls.Add(SectionTitleTop("최근 검사 이력"));
            Body.Controls.Add(Columns(S(130), new Control[] { _total, _ng, _yield }));
            Body.Controls.Add(Columns(S(220), new Control[] { _judge, _leak, _limit }, new[] { 50f, 25f, 25f }));
            Body.Controls.Add(Row(Caption("바코드"), _barcode, Caption("LOT"), _lot, lotButton, adminButton));
            ShowCurrent();
        }

        private Label SectionTitleTop(string text)
        {
            var title = SectionTitle(text);
            title.Dock = DockStyle.Top;
            return title;
        }

        private void BuildGrid()
        {
            _grid.Columns.Add(new ApcLampColumn());
            _grid.Columns.Add("time", "시각");
            _grid.Columns.Add("barcode", "바코드");
            _grid.Columns.Add("leak", "누설량(" + LeakJudgement.Unit + ")");
            _grid.Columns.Add("result", "판정");
            foreach (DataGridViewColumn c in _grid.Columns)
                if (!(c is ApcLampColumn)) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            int n = 0;
            foreach (var r in _source.RecentHistory)
            {
                if (n++ >= HistoryRows) break;
                var kind = LeakJudgement.IsOk(r.Leak) ? StatusKind.Run : StatusKind.Alarm;
                int row = _grid.Rows.Add(kind, r.Time.ToString("HH:mm:ss"), r.Barcode,
                    LeakJudgement.Format(r.Leak), LeakJudgement.ResultText(r.Leak));
                if (kind == StatusKind.Alarm)
                {
                    var style = _grid.Rows[row].Cells["leak"].Style;
                    style.ForeColor = ApcTheme.Status(kind);
                    style.Font = ApcTheme.GetFont(System.Drawing.FontStyle.Bold);
                }
            }
        }

        private void ShowCurrent()
        {
            var cur = _source.CurrentCell;
            bool ok = LeakJudgement.IsOk(cur.Leak);
            _barcode.Text = cur.Barcode;
            _lot.Text = _source.CurrentLot;

            _judge.Text = ok ? "OK" : "NG";
            _judge.SubText = cur.Barcode + " · " + _source.CurrentModel;
            _judge.Status = ok ? StatusKind.Run : StatusKind.Alarm;
            _leak.Value = LeakJudgement.Format(cur.Leak);
            _leak.Status = ok ? StatusKind.Run : StatusKind.Alarm;
            _leak.Footer = LeakJudgement.ResultText(cur.Leak);
            Header.Status = ok ? StatusKind.Run : StatusKind.Alarm;
            Header.StatusText = ok ? "가동중" : "NG 발생";

            int total = _source.TodayOk + _source.TodayNg;
            _total.Value = total.ToString("#,0", CultureInfo.InvariantCulture);
            _total.Status = StatusKind.Run;
            _ng.Value = _source.TodayNg.ToString("#,0", CultureInfo.InvariantCulture);
            _ng.Status = _source.TodayNg > 0 ? StatusKind.Alarm : StatusKind.Run;
            _yield.Value = total == 0 ? "" : (100.0 * _source.TodayOk / total).ToString("0.0", CultureInfo.InvariantCulture);
            _yield.Status = total == 0 ? StatusKind.Stop : StatusKind.Run;
        }

        private void EditLot()
        {
            using (var dlg = new LotInputDialog(_source.CurrentLot, _source.CurrentModel, _source.CurrentWorker))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                _source.SaveLot(dlg.Lot, dlg.Model, dlg.Worker);
                ShowCurrent();
            }
        }

        private void EnterAdmin()
        {
            using (var dlg = new AdminPasswordDialog())
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                if (!_source.VerifyAdminPassword(dlg.Password))
                    ApcDialog.Alert(FindForm(), "비밀번호가 맞지 않습니다.");
                // 맞으면 설정 화면 진입(이 프로젝트는 설정 화면 없음)
            }
        }

        private static ApcValueCard Card(string title, string unit, string footer)
        {
            return new ApcValueCard { Title = title, Unit = unit, Footer = footer, Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(12), 0) };
        }
    }
}
