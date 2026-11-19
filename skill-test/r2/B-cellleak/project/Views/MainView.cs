using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CellLeakTester.Data;

namespace CellLeakTester.Views
{
    /// <summary>누설 검사 메인 화면. 데이터는 IMainViewSource에서만 받는다.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private readonly IMainViewSource _source;
        private readonly ApcValueCard _lot = TextCard("LOT");
        private string _model, _worker;

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("2차전지 셀 누설 검사")
        {
            _source = source;
            _model = source.CurrentModel;
            _worker = source.CurrentOperator;
            var cur = source.History[0];
            var current = cur.IsOk ? StatusKind.Run : StatusKind.Alarm;
            string verdict = cur.IsOk ? "OK" : "NG";

            Header.Status = current;
            Header.StatusText = "검사 " + verdict;

            var barcode = TextCard("현재 셀 바코드");
            barcode.Value = cur.Barcode;
            barcode.Footer = "검사 " + cur.Time;
            _lot.Value = source.CurrentLot;
            ShowLotFooter();

            var judge = new ApcJudgePanel { Dock = DockStyle.Fill, Text = verdict, Status = current, SubText = cur.Barcode, Margin = new Padding(0, 0, S(12), 0) };
            var leak = new ApcValueCard
            {
                Title = "누설량", Unit = "Pa/s", Value = cur.Leak.ToString("0.0", CultureInfo.InvariantCulture),
                Status = current, Footer = "기준 " + LeakJudge.Limit.ToString("0.0", CultureInfo.InvariantCulture) + " 이하 OK",
                Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(12), 0)
            };
            var limit = new ApcValueCard
            {
                Title = "기준값 (상한)", Unit = "Pa/s", Value = LeakJudge.Limit.ToString("0.0", CultureInfo.InvariantCulture),
                Footer = "상한과 같으면 OK, 초과 NG", Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(12), 0)
            };

            int total = source.TodayTotal, ng = source.TodayNg;
            var tTotal = Count("오늘 생산", "EA", total.ToString("#,0"), StatusKind.Run);
            var tNg = Count("오늘 NG", "EA", ng.ToString("#,0"), ng > 0 ? StatusKind.Alarm : StatusKind.Run);
            var tYield = Count("수율", "%", (100.0 * (total - ng) / total).ToString("0.0", CultureInfo.InvariantCulture), StatusKind.Run);
            tYield.Footer = "OK / 생산";

            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn());
            grid.Columns.Add("time", "시각");
            grid.Columns.Add("barcode", "바코드");
            grid.Columns.Add("leak", "누설량(Pa/s)");
            grid.Columns.Add("result", "판정");
            foreach (DataGridViewColumn c in grid.Columns) if (!(c is ApcLampColumn)) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (var r in source.History)
                grid.Rows.Add(r.IsOk ? StatusKind.Run : StatusKind.Alarm, r.Time, r.Barcode, r.Leak.ToString("0.0", CultureInfo.InvariantCulture), r.IsOk ? "OK" : "NG");

            var lotBtn = Button("LOT 입력", ApcButtonVariant.Normal, 130);
            lotBtn.Click += (s, e) => EditLot();
            var adminBtn = Button("관리자", ApcButtonVariant.Normal, 130);
            adminBtn.Click += (s, e) => AskAdmin();

            Body.Controls.Add(grid);
            Body.Controls.Add(Columns(S(130), new Control[] { tTotal, tNg, tYield }));
            Body.Controls.Add(Columns(S(220), new Control[] { judge, leak, limit }, new[] { 40f, 30f, 30f }));
            Body.Controls.Add(Columns(S(110), new Control[] { barcode, _lot }));
            Body.Controls.Add(Row(lotBtn, adminBtn));
        }

        private void ShowLotFooter() { _lot.Footer = _model + " · 작업자 " + _worker; }

        private void EditLot()
        {
            using (var dlg = new LotDialog(_lot.Value, _model, _worker))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                _lot.Value = dlg.Lot; _model = dlg.Model; _worker = dlg.Operator;
                ShowLotFooter();
            }
        }

        private void AskAdmin()
        {
            using (var dlg = new AdminPasswordDialog())
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                if (!_source.CheckAdminPassword(dlg.Password)) ApcDialog.Alert(FindForm(), "비밀번호가 맞지 않습니다.");
            }
        }

        private static ApcValueCard TextCard(string title)
        {
            return new ApcValueCard { Title = title, Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(12), 0) };
        }

        private static ApcValueCard Count(string title, string unit, string value, StatusKind status)
        {
            return new ApcValueCard { Title = title, Unit = unit, Value = value, Status = status, Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(12), 0) };
        }
    }
}
