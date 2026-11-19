using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CleanroomMonitor.Data;

namespace CleanroomMonitor.Views
{
    /// <summary>메인 화면: 구역 센서 표(기준 이탈 강조) + 선택 구역 온습도 추이. 데이터는 IMainViewSource에서만 받는다.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const int ColLamp = 0, ColZone = 1, ColTemp = 2, ColHumi = 3, ColParticle = 4, ColPressure = 5, ColVerdict = 6;
        private const string InitialZone = "A3";

        private readonly IMainViewSource _source;
        private readonly ApcGrid _grid = new ApcGrid { ReadOnly = true, ClearInitialSelection = false };
        private readonly ApcTrendChart _tempChart = new ApcTrendChart { Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(6), 0) };
        private readonly ApcTrendChart _humiChart = new ApcTrendChart { Dock = DockStyle.Fill, Margin = new Padding(S(6), 0, 0, 0) };
        private readonly Label _tempTitle = Caption("");
        private readonly Label _humiTitle = Caption("");
        private readonly Label _trendTitle = SectionTitle("");
        private ZoneSettings _settings;

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("클린룸 환경 모니터링")
        {
            _source = source;
            _settings = source.GetSettings();
            BuildGrid();

            var settingsButton = Button("설정", ApcButtonVariant.Normal, 90);
            settingsButton.Click += (s, e) => OpenSettings();

            var chartArea = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, BackColor = Color.Transparent };
            chartArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            chartArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            chartArea.RowStyles.Add(new RowStyle(SizeType.Absolute, S(30)));
            chartArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            foreach (var l in new[] { _tempTitle, _humiTitle }) { l.Dock = DockStyle.Fill; l.Margin = new Padding(0); }
            _humiTitle.Margin = new Padding(S(6), 0, 0, 0);
            chartArea.Controls.Add(_tempTitle, 0, 0);
            chartArea.Controls.Add(_humiTitle, 1, 0);
            chartArea.Controls.Add(_tempChart, 0, 1);
            chartArea.Controls.Add(_humiChart, 1, 1);

            // Fill 먼저, Top 줄은 아래쪽 것부터
            Body.Controls.Add(chartArea);
            Body.Controls.Add(Row(_trendTitle));
            Body.Controls.Add(Columns(ApcGrid.HeightForRows(_source.GetZones().Length), new Control[] { _grid }));
            Body.Controls.Add(Row(SectionTitle("구역 센서 현황"), settingsButton));

            LoadRows(InitialZone);
            _grid.SelectionChanged += (s, e) => ShowTrend();
            ShowTrend();
        }

        private void BuildGrid()
        {
            _grid.Columns.Add(new ApcLampColumn { HeaderText = "상태" });
            _grid.Columns.Add("zone", "구역");
            _grid.Columns.Add("temp", "온도(℃)");
            _grid.Columns.Add("humi", "습도(%RH)");
            _grid.Columns.Add("particle", "파티클 0.5µm(개/ft³)");
            _grid.Columns.Add("pressure", "차압(Pa)");
            _grid.Columns.Add("verdict", "판정");
            int[] widths = { 60, 110, 150, 160, 240, 150 };   // 짧은 값 열은 고정 폭, 긴 글(판정) 열만 Fill
            for (int i = ColLamp; i <= ColPressure; i++)
            {
                _grid.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                _grid.Columns[i].Width = S(widths[i]);
            }
            _grid.Columns[ColVerdict].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _grid.Columns[ColVerdict].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        }

        private ZoneLimits LimitsOf(string id)
        {
            return Array.Find(_settings.Limits, l => l.Id == id);
        }

        private void LoadRows(string selectZone)
        {
            _grid.Rows.Clear();
            var zones = _source.GetZones();
            int alarm = 0, warn = 0, stop = 0, down = 0, ok = 0, selectRow = 0;
            for (int i = 0; i < zones.Length; i++)
            {
                var z = zones[i];
                var lim = LimitsOf(z.Id);
                var zv = ZoneJudge.Zone(z, lim);
                _grid.Rows.Add(zv.Status, z.Id,
                    z.Temperature.HasValue ? z.Temperature.Value.ToString("0.0", CultureInfo.InvariantCulture) : "",
                    z.Humidity.HasValue ? z.Humidity.Value.ToString("0.0", CultureInfo.InvariantCulture) : "",
                    z.Particles.HasValue ? z.Particles.Value.ToString("#,0", CultureInfo.InvariantCulture) : "",
                    z.Pressure.HasValue ? z.Pressure.Value.ToString("0.0", CultureInfo.InvariantCulture) : "",
                    zv.Reason);
                var row = _grid.Rows[i];
                Mark(row.Cells[ColTemp], ZoneJudge.Temperature(z.Temperature, lim));
                Mark(row.Cells[ColHumi], ZoneJudge.Humidity(z.Humidity, lim));
                Mark(row.Cells[ColParticle], ZoneJudge.Particles(z.Particles, lim));
                Mark(row.Cells[ColPressure], ZoneJudge.Pressure(z.Pressure));
                if (z.Id == selectZone) selectRow = i;

                if (zv.Status == StatusKind.Alarm) { alarm++; Alarm.Push(z.Id + ": " + zv.Reason.Replace("가동 · ", "")); }
                else if (zv.Status == StatusKind.Warn) warn++;
                else if (z.Process == ZoneProcess.Disconnected) down++;
                else if (zv.Status == StatusKind.Stop) stop++;
                else ok++;
            }
            ShowSummary(ok, alarm, warn, stop, down);
            _grid.CurrentCell = _grid.Rows[selectRow].Cells[ColZone];
        }

        // 기준 이탈 값 셀: 상태색 글자 + 굵게(선택 행에서도 유지)
        private static void Mark(DataGridViewCell cell, Verdict v)
        {
            if (v.Status != StatusKind.Warn && v.Status != StatusKind.Alarm) return;
            cell.Style.ForeColor = ApcTheme.Status(v.Status);
            cell.Style.SelectionForeColor = ApcTheme.Status(v.Status);
            cell.Style.Font = ApcTheme.GetFont(FontStyle.Bold);
        }

        private void ShowSummary(int ok, int alarm, int warn, int stop, int down)
        {
            string text = "전체 · 정상 " + ok;
            if (alarm > 0) text += " · 알람 " + alarm;
            if (warn > 0) text += " · 경고 " + warn;
            if (stop > 0) text += " · 정지 " + stop;
            if (down > 0) text += " · 통신 끊김 " + down;
            Header.StatusText = alarm + warn + stop + down == 0 ? "전체 · 정상" : text;
            Header.Status = alarm > 0 ? StatusKind.Alarm : warn > 0 ? StatusKind.Warn : stop + down > 0 ? StatusKind.Stop : StatusKind.Run;
        }

        private void ShowTrend()
        {
            var row = _grid.CurrentRow;
            if (row == null) return;
            string id = (string)row.Cells[ColZone].Value;
            var lim = LimitsOf(id);
            var trend = _source.GetTrend(id);
            _trendTitle.Text = "선택 구역 " + id + " 온습도 추이 (최근 24시간, 1시간 간격)";
            _tempTitle.Text = "온도 (℃) · 기준 " + lim.TempLow.ToString("0.0") + " ~ " + lim.TempHigh.ToString("0.0");
            _humiTitle.Text = "습도 (%RH) · 기준 " + lim.HumiLow.ToString("0.0") + " ~ " + lim.HumiHigh.ToString("0.0");
            Configure(_tempChart, trend.Temperatures, lim.TempLow, lim.TempHigh);
            Configure(_humiChart, trend.Humidities, lim.HumiLow, lim.HumiHigh);
        }

        // ponytail: 설정값 선은 기준 범위의 가운데로 둔다(별도 목표값 설정 없음)
        private static void Configure(ApcTrendChart chart, double[] values, double low, double high)
        {
            chart.SetValues(values);
            chart.LowerLimit = low;
            chart.UpperLimit = high;
            chart.Reference = (low + high) / 2;
            chart.XLabels = new[] { "-23h", "-12h", "현재" };
        }

        private void OpenSettings()
        {
            using (var dlg = new SettingsDialog(_settings))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                _source.SaveSettings(dlg.Result);
                _settings = dlg.Result;
                var current = _grid.CurrentRow;
                LoadRows(current == null ? InitialZone : (string)current.Cells[ColZone].Value);
            }
        }
    }
}
