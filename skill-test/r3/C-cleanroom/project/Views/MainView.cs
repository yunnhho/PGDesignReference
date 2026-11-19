using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CleanroomMonitor.Data;

namespace CleanroomMonitor.Views
{
    /// <summary>
    /// 메인 화면: 구역 센서 목록 표(기준 이탈 값 강조) + 선택 구역 온습도 추이. 헤더에 구역 상태 요약.
    /// 데이터는 IMainViewSource에서만 받는다(기본: 샘플 데이터).
    /// </summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const int ColLamp = 0, ColZone = 1, ColTemp = 2, ColHumi = 3, ColParticle = 4, ColDiff = 5, ColText = 6;
        private const string DefaultSelectedZone = "B1";

        private readonly IMainViewSource _source;
        private readonly ApcGrid _grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true, ClearInitialSelection = false };
        private readonly ApcTrendChart _tempChart = new ApcTrendChart { Dock = DockStyle.Fill };
        private readonly ApcTrendChart _humiChart = new ApcTrendChart { Dock = DockStyle.Fill };
        private readonly Label _tempTitle = SectionTitle("");
        private readonly Label _humiTitle = SectionTitle("");
        private bool _filling;

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("클린룸 환경 모니터링")
        {
            _source = source;
            BuildGrid();

            // Top 도킹은 나중에 Add한 것이 위 → 차트(Fill) 먼저, 그다음 아래 줄부터
            Body.Controls.Add(BuildCharts());
            Body.Controls.Add(Columns(ApcGrid.HeightForRows(8), new Control[] { _grid }));
            Body.Controls.Add(BuildTitleBar());

            _grid.SelectionChanged += (s, e) => { if (!_filling) ShowTrend(); };
            Reload(DefaultSelectedZone);
            foreach (var z in Judged())
                if (z.Verdict.Status == StatusKind.Alarm) Alarm.Push(z.Reading.Zone + ": " + z.Verdict.Text.Substring("알람 · ".Length));
        }

        private sealed class Judgement
        {
            public ZoneReading Reading;
            public ZoneVerdict Verdict;
        }

        private Judgement[] Judged()
        {
            var readings = _source.GetReadings();
            var limits = _source.GetLimits();
            var list = new Judgement[readings.Length];
            for (int i = 0; i < list.Length; i++)
                list[i] = new Judgement { Reading = readings[i], Verdict = ZoneJudge.Judge(readings[i], limits[i]) };
            return list;
        }

        private void BuildGrid()
        {
            _grid.Columns.Add(new ApcLampColumn { Width = S(60) });
            AddColumn("zone", "구역", 110);
            AddColumn("temp", "온도(℃)", 150);
            AddColumn("humi", "습도(%RH)", 150);
            AddColumn("particle", "파티클 0.5µm(개/ft³)", 230);
            AddColumn("diff", "차압(Pa)", 150);
            var text = _grid.Columns[_grid.Columns.Add("text", "판정")];
            text.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            text.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            text.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        }

        private void AddColumn(string name, string header, int width)
        {
            _grid.Columns[_grid.Columns.Add(name, header)].Width = S(width);
        }

        private Control BuildTitleBar()
        {
            var settings = Button("설정", ApcButtonVariant.Normal, 100);
            settings.Dock = DockStyle.Right;
            settings.Click += (s, e) => OpenSettings();
            var title = SectionTitle("구역별 센서 현황");
            title.Dock = DockStyle.Fill;
            title.AutoSize = false;
            title.TextAlign = ContentAlignment.MiddleLeft;
            var bar = new Panel { Dock = DockStyle.Top, Height = settings.Height + S(8), BackColor = Color.Transparent };
            settings.Height = bar.Height - S(8);
            bar.Controls.Add(title);
            bar.Controls.Add(settings);
            return bar;
        }

        private Control BuildCharts()
        {
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent, Margin = Padding.Empty };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            table.Controls.Add(ChartCell(_tempChart, _tempTitle, new Padding(0, 0, S(6), 0)), 0, 0);
            table.Controls.Add(ChartCell(_humiChart, _humiTitle, new Padding(S(6), 0, 0, 0)), 1, 0);
            return table;
        }

        private static Control ChartCell(ApcTrendChart chart, Label title, Padding padding)
        {
            title.Dock = DockStyle.Top;
            var cell = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = padding, Margin = Padding.Empty };
            cell.Controls.Add(chart);
            cell.Controls.Add(title);
            return cell;
        }

        /// <summary>표·헤더 요약을 다시 채우고 zone 행을 선택한다(없으면 첫 행).</summary>
        private void Reload(string selectZone)
        {
            _filling = true;
            _grid.Rows.Clear();
            var all = Judged();
            int[] counts = new int[4];   // 정상, 알람, 경고, 정지
            StatusKind worst = StatusKind.Run;
            int selectIndex = 0;
            for (int i = 0; i < all.Length; i++)
            {
                var r = all[i].Reading;
                var v = all[i].Verdict;
                int row = _grid.Rows.Add(v.Status, r.Zone,
                    r.Connected ? ZoneJudge.Fmt1(r.Temp.Value) : "", r.Connected ? ZoneJudge.Fmt1(r.Humi.Value) : "",
                    r.Connected ? ZoneJudge.FmtCount(r.Particle.Value) : "", r.Connected ? ZoneJudge.Fmt1(r.DiffPressure.Value) : "", v.Text);
                Emphasize(row, ColTemp, v.Temp);
                Emphasize(row, ColHumi, v.Humi);
                Emphasize(row, ColParticle, v.Particle);
                Emphasize(row, ColDiff, v.Diff);
                counts[v.Status == StatusKind.Run ? 0 : v.Status == StatusKind.Alarm ? 1 : v.Status == StatusKind.Warn ? 2 : 3]++;
                if (ZoneJudge.Severity(v.Status) > ZoneJudge.Severity(worst)) worst = v.Status;
                if (r.Zone == selectZone) selectIndex = i;
            }
            _grid.CurrentCell = _grid.Rows[selectIndex].Cells[ColZone];
            _filling = false;

            Header.Status = worst;
            Header.StatusText = SummaryText(counts);
            ShowTrend();
        }

        private static string SummaryText(int[] counts)
        {
            string[] names = { "정상", "알람", "경고", "정지" };
            var text = "전체";
            for (int i = 0; i < counts.Length; i++)
                if (counts[i] > 0) text += " · " + names[i] + " " + counts[i];
            return text;
        }

        // 기준 이탈 값(경고·알람)은 값 글자색 + 굵게
        private void Emphasize(int row, int col, StatusKind kind)
        {
            if (kind != StatusKind.Warn && kind != StatusKind.Alarm) return;
            var style = _grid.Rows[row].Cells[col].Style;
            style.ForeColor = ApcTheme.Status(kind);
            style.SelectionForeColor = ApcTheme.Status(kind);
            style.Font = ApcTheme.GetFont(FontStyle.Bold);
        }

        private void ShowTrend()
        {
            var row = _grid.CurrentRow;
            if (row == null) return;
            string zone = (string)row.Cells[ColZone].Value;
            var limits = _source.GetLimits();
            ZoneLimits l = null;
            foreach (var x in limits) if (x.Zone == zone) l = x;
            var trend = _source.GetTrend(zone);

            _tempTitle.Text = "선택 구역 " + zone + " · 온도 추이 (℃)";
            _humiTitle.Text = "선택 구역 " + zone + " · 습도 추이 (%RH)";
            Apply(_tempChart, trend.Temps, trend.XLabels, l.TempLow, l.TempHigh);
            Apply(_humiChart, trend.Humis, trend.XLabels, l.HumiLow, l.HumiHigh);
        }

        private static void Apply(ApcTrendChart chart, double[] values, string[] labels, double low, double high)
        {
            chart.LowerLimit = low;
            chart.UpperLimit = high;
            chart.XLabels = labels;
            chart.SetValues(values);
        }

        private void OpenSettings()
        {
            using (var dlg = new SettingsDialog(_source.GetLimits(), _source.GetStorageIntervalSec()))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                _source.SaveSettings(dlg.Limits, dlg.StorageIntervalSec);
            }
            var row = _grid.CurrentRow;
            Reload(row == null ? DefaultSelectedZone : (string)row.Cells[ColZone].Value);
        }
    }
}
