using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CleanroomMonitor.Data;

namespace CleanroomMonitor.Views
{
    /// <summary>
    /// 클린룸 환경 모니터링: 구역 센서 목록 표(기준 이탈 값 강조) + 선택 구역 온습도 추이 + 헤더 요약.
    /// 데이터는 IMainViewSource에서만 받는다(기본: 샘플 데이터).
    /// </summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const string ScreenTitle = "클린룸 환경 모니터링";
        private const int ColZone = 1, ColTemp = 2, ColHumi = 3, ColParticle = 4, ColDiff = 5;
        private const int InitialZoneIndex = 5;   // 샘플 시작 선택 = 알람 구역(B2)

        private readonly IMainViewSource _source;
        private readonly ApcGrid _grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true, ClearInitialSelection = false, MultiSelect = false };
        private readonly ApcGroup _tempGroup = new ApcGroup { Dock = DockStyle.Fill };
        private readonly ApcGroup _humiGroup = new ApcGroup { Dock = DockStyle.Fill };
        private readonly ApcTrendChart _tempChart = new ApcTrendChart { Dock = DockStyle.Fill };
        private readonly ApcTrendChart _humiChart = new ApcTrendChart { Dock = DockStyle.Fill };
        private readonly HashSet<string> _alarmed = new HashSet<string>();
        private IList<ZoneReading> _zones;

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base(ScreenTitle)
        {
            _source = source;
            BuildGrid();

            var charts = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            charts.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            _tempGroup.Controls.Add(_tempChart);
            _humiGroup.Controls.Add(_humiChart);
            _tempGroup.Margin = new Padding(0, 0, S(6), 0);
            _humiGroup.Margin = new Padding(S(6), 0, 0, 0);
            charts.Controls.Add(_tempGroup, 0, 0);
            charts.Controls.Add(_humiGroup, 1, 0);

            var settings = Button("설정", ApcButtonVariant.Normal, 110);
            settings.Dock = DockStyle.Right;
            settings.Click += (s, e) => OpenSettings();
            var title = SectionTitle("구역 센서 목록");
            title.Dock = DockStyle.Left;
            title.TextAlign = ContentAlignment.MiddleLeft;
            var titleRow = new Panel { Dock = DockStyle.Top, Height = S(48), BackColor = Color.Transparent };
            titleRow.Controls.Add(title);
            titleRow.Controls.Add(settings);

            // Fill 먼저, Top은 아래쪽 줄부터
            Body.Controls.Add(charts);
            Body.Controls.Add(Columns(ApcGrid.HeightForRows(8), new Control[] { _grid }));
            Body.Controls.Add(titleRow);

            Reload();
            _grid.Rows[InitialZoneIndex].Selected = true;
            _grid.CurrentCell = _grid.Rows[InitialZoneIndex].Cells[ColZone];
            ShowTrend();
            _grid.SelectionChanged += (s, e) => ShowTrend();
        }

        private void BuildGrid()
        {
            _grid.Columns.Add(new ApcLampColumn { HeaderText = "상태" });
            _grid.Columns.Add("zone", "구역");
            _grid.Columns.Add("temp", "온도(℃)");
            _grid.Columns.Add("humi", "습도(%RH)");
            _grid.Columns.Add("particle", "파티클 0.5µm(개/ft³)");
            _grid.Columns.Add("diff", "차압(Pa)");
            _grid.Columns.Add("judge", "판정");
            _grid.Columns["judge"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _grid.Columns["judge"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            _grid.Columns["particle"].Width = S(210);
            foreach (DataGridViewColumn c in _grid.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        /// <summary>표·헤더 요약·알람을 데이터 인터페이스 값으로 다시 채운다(설정 변경 후에도).</summary>
        private void Reload()
        {
            _zones = _source.GetZones();
            string selected = SelectedZone();
            _grid.Rows.Clear();
            int run = 0, warn = 0, alarm = 0, stop = 0;
            foreach (var z in _zones)
            {
                var j = ZoneJudge.Judge(z);
                int i = _grid.Rows.Add(j.Status, z.Zone,
                    z.Temp.HasValue ? z.Temp.Value.ToString("0.0") : "",
                    z.Humi.HasValue ? z.Humi.Value.ToString("0.0") : "",
                    z.Particle.HasValue ? z.Particle.Value.ToString("#,0") : "",
                    z.DiffPressure.HasValue ? z.DiffPressure.Value.ToString("0.0") : "",
                    j.Text);
                var cells = _grid.Rows[i].Cells;
                Emphasize(cells[ColTemp], j.Temp);
                Emphasize(cells[ColHumi], j.Humi);
                Emphasize(cells[ColParticle], j.Particle);
                Emphasize(cells[ColDiff], j.Diff);
                switch (j.Status)
                {
                    case StatusKind.Alarm: alarm++; PushAlarm(z.Zone, j.Text); break;
                    case StatusKind.Warn: warn++; break;
                    case StatusKind.Stop: stop++; break;
                    default: run++; break;
                }
            }
            UpdateHeader(run, alarm, warn, stop);
            if (selected != null)
                for (int i = 0; i < _grid.Rows.Count; i++)
                    if ((string)_grid.Rows[i].Cells[ColZone].Value == selected) { _grid.Rows[i].Selected = true; _grid.CurrentCell = _grid.Rows[i].Cells[ColZone]; }
        }

        private void PushAlarm(string zone, string text)
        {
            if (_alarmed.Add(zone)) Alarm.Push(zone + ": " + text.Replace("알람 · ", ""));
        }

        /// <summary>순서: 정상 → 알람 → 경고 → 정지. 0인 것은 생략. 램프 = 가장 나쁜 상태.</summary>
        private void UpdateHeader(int run, int alarm, int warn, int stop)
        {
            var parts = new List<string>();
            if (run > 0) parts.Add("정상 " + run);
            if (alarm > 0) parts.Add("알람 " + alarm);
            if (warn > 0) parts.Add("경고 " + warn);
            if (stop > 0) parts.Add("정지 " + stop);
            Header.StatusText = "전체 · " + string.Join(" · ", parts);
            Header.Status = alarm > 0 ? StatusKind.Alarm : warn > 0 ? StatusKind.Warn : stop > 0 ? StatusKind.Stop : StatusKind.Run;
        }

        private static void Emphasize(DataGridViewCell cell, ItemJudge item)
        {
            if (item.Status == StatusKind.Run || item.Status == StatusKind.Stop) return;
            cell.Style.ForeColor = ApcTheme.Status(item.Status);
            cell.Style.Font = ApcTheme.GetFont(FontStyle.Bold);
        }

        private string SelectedZone()
        {
            var row = _grid.CurrentRow;
            return row == null ? null : (string)row.Cells[ColZone].Value;
        }

        private void ShowTrend()
        {
            string zone = SelectedZone();
            if (zone == null) return;
            ZoneLimits l = null;
            foreach (var z in _zones) if (z.Zone == zone) l = z.Limits;
            var t = _source.GetTrend(zone);
            _tempGroup.Text = zone + " 온도 추이 (℃) · 빨간 점선 = 상·하한";
            _humiGroup.Text = zone + " 습도 추이 (%RH) · 빨간 점선 = 상·하한";
            _tempChart.UpperLimit = l.TempHigh; _tempChart.LowerLimit = l.TempLow;
            _humiChart.UpperLimit = l.HumiHigh; _humiChart.LowerLimit = l.HumiLow;
            _tempChart.XLabels = t.XLabels; _humiChart.XLabels = t.XLabels;
            _tempChart.SetValues(t.Temp);
            _humiChart.SetValues(t.Humi);
        }

        private void OpenSettings()
        {
            using (var dlg = new SettingsDialog(_source.GetLimits(), _source.GetSavePeriodSec()))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                _source.SaveSettings(dlg.Limits, dlg.SavePeriodSec);
                Reload();
                ShowTrend();
            }
        }
    }
}
