using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CleanroomMonitor.Data;

namespace CleanroomMonitor.Views
{
    /// <summary>
    /// 클린룸 환경 모니터링 메인 화면: 헤더 요약 + 구역 센서 표 + 선택 구역 온습도 추이.
    /// 데이터는 IMainViewSource에서만 받는다(기본: 샘플 데이터).
    /// </summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const int ColLamp = 0, ColName = 1, ColReason = 6;
        private readonly IMainViewSource _source;
        private readonly ApcGrid _grid = new ApcGrid { ReadOnly = true, ClearInitialSelection = false };
        private readonly ApcTrendChart _tempChart = new ApcTrendChart();
        private readonly ApcTrendChart _humiChart = new ApcTrendChart();
        private readonly Label _tempTitle = SectionTitle("");
        private readonly Label _humiTitle = SectionTitle("");

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("클린룸 환경 모니터링")
        {
            _source = source;

            // 아래쪽부터 Add: 추이(Fill) → 추이 제목 → 표 → 표 제목 → 도구 줄
            var charts = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            charts.Controls.Add(ChartCell(_tempChart, _tempTitle, true), 0, 0);
            charts.Controls.Add(ChartCell(_humiChart, _humiTitle, false), 1, 0);

            _grid.Height = (int)Math.Round(ApcTheme.MinHeight * 1.15) + ApcTheme.MinHeight * 8 + 2;
            _grid.Dock = DockStyle.Top;
            BuildColumns();
            _grid.SelectionChanged += (s, e) => ShowTrend();

            var settings = Button("설정", ApcButtonVariant.Normal, 100);
            settings.Click += (s, e) => OpenSettings();
            var tools = new Panel { Dock = DockStyle.Top, Height = S(46), BackColor = Color.Transparent };
            var toolsRow = Row(settings);
            toolsRow.Dock = DockStyle.Right;
            toolsRow.AutoSize = true;
            tools.Controls.Add(toolsRow);
            var gridTitle = SectionTitle("구역별 센서 현재값");
            gridTitle.Dock = DockStyle.Left;
            gridTitle.AutoSize = true;
            gridTitle.Padding = new Padding(0, S(12), 0, 0);
            tools.Controls.Add(gridTitle);

            Body.Controls.Add(charts);
            Body.Controls.Add(Spacer());
            Body.Controls.Add(_grid);
            Body.Controls.Add(tools);

            Refresh_();
        }

        private static Panel Spacer() { return new Panel { Dock = DockStyle.Top, Height = S(12), BackColor = Color.Transparent }; }

        private static Panel ChartCell(ApcTrendChart chart, Label title, bool leftCell)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Margin = new Padding(0, 0, leftCell ? S(12) : 0, 0) };
            chart.Dock = DockStyle.Fill;
            title.Dock = DockStyle.Top;
            title.Height = S(34);
            p.Controls.Add(chart);
            p.Controls.Add(title);
            return p;
        }

        private void BuildColumns()
        {
            _grid.Columns.Add(new ApcLampColumn { HeaderText = "", Width = ApcTheme.Px(70) });
            AddCol("zone", "구역", 100);
            AddCol("temp", "온도(℃)", 150);
            AddCol("humi", "습도(%RH)", 150);
            AddCol("particle", "파티클 0.5µm(개/ft³)", 230);
            AddCol("press", "차압(Pa)", 150);
            var reason = AddCol("reason", "상태 · 기준 이탈 내용", 100);
            reason.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            reason.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            foreach (DataGridViewColumn c in _grid.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private DataGridViewColumn AddCol(string name, string header, int width)
        {
            int i = _grid.Columns.Add(name, header);
            _grid.Columns[i].Width = ApcTheme.Px(width);
            return _grid.Columns[i];
        }

        private static string Fmt(double? v, string f) { return v.HasValue ? v.Value.ToString(f) : ""; }

        /// <summary>표·헤더 요약·알람바를 현재 데이터와 기준으로 다시 채운다.</summary>
        private void Refresh_()
        {
            int keep = _grid.CurrentRow != null ? _grid.CurrentRow.Index : 0;
            var zones = _source.GetZones();
            var limits = _source.GetLimits();
            _grid.Rows.Clear();
            int normal = 0, warn = 0, alarm = 0, stop = 0;
            for (int i = 0; i < zones.Count; i++)
            {
                var z = zones[i];
                string reason;
                var st = ZoneJudge.Judge(z, limits[i], out reason);
                _grid.Rows.Add(st, z.Name, Fmt(z.Temp, "0.0"), Fmt(z.Humi, "0.0"), Fmt(z.Particle, "#,0"), Fmt(z.Pressure, "0.0"), st == StatusKind.Run ? "정상" : StatusWord(st) + " · " + reason);
                var row = _grid.Rows[i];
                if (st == StatusKind.Warn || st == StatusKind.Alarm)
                    row.DefaultCellStyle.Font = ApcTheme.GetFont(FontStyle.Bold);
                if (st == StatusKind.Alarm)
                    row.DefaultCellStyle.ForeColor = ApcTheme.Status(StatusKind.Alarm);
                switch (st)
                {
                    case StatusKind.Run: normal++; break;
                    case StatusKind.Warn: warn++; break;
                    case StatusKind.Alarm: alarm++; Alarm.Push(z.Name + ": " + reason); break;
                    default: stop++; break;
                }
            }
            Header.Status = alarm > 0 ? StatusKind.Alarm : warn > 0 ? StatusKind.Warn : stop > 0 ? StatusKind.Stop : StatusKind.Run;
            Header.StatusText = "전체 · 정상 " + normal + " · 경고 " + warn + " · 알람 " + alarm + (stop > 0 ? " · 정지 " + stop : "");
            if (_grid.Rows.Count > 0)
            {
                _grid.CurrentCell = _grid.Rows[Math.Min(keep, _grid.Rows.Count - 1)].Cells[ColName];
            }
            ShowTrend();
        }

        private static string StatusWord(StatusKind k)
        {
            switch (k)
            {
                case StatusKind.Run: return "정상";
                case StatusKind.Warn: return "경고";
                case StatusKind.Alarm: return "알람";
                default: return "정지";
            }
        }

        private void ShowTrend()
        {
            if (_grid.CurrentRow == null) return;
            int idx = _grid.CurrentRow.Index;
            var zones = _source.GetZones();
            var lim = _source.GetLimits()[idx];
            string name = zones[idx].Name;
            var trend = _source.GetTrend(name);
            _tempTitle.Text = name + " 온도 추이(℃) · 상·하한 " + lim.TempLow.ToString("0.0") + " ~ " + lim.TempHigh.ToString("0.0");
            _humiTitle.Text = name + " 습도 추이(%RH) · 상·하한 " + lim.HumiLow.ToString("0.0") + " ~ " + lim.HumiHigh.ToString("0.0");
            _tempChart.UpperLimit = (double)lim.TempHigh; _tempChart.LowerLimit = (double)lim.TempLow;
            _humiChart.UpperLimit = (double)lim.HumiHigh; _humiChart.LowerLimit = (double)lim.HumiLow;
            _tempChart.XLabels = _humiChart.XLabels = new[] { "-60분", "-30분", "현재" };
            _tempChart.SetValues(trend.Temps);
            _humiChart.SetValues(trend.Humis);
        }

        private void OpenSettings()
        {
            using (var dlg = new SettingsDialog(_source.GetLimits(), _source.GetSaveIntervalSec()))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                _source.SaveSettings(dlg.Limits, dlg.SaveIntervalSec);
                while (Alarm.Count > 0) Alarm.AcknowledgeCurrent();   // 새 기준으로 다시 판정해 알람을 다시 올린다
                Refresh_();
            }
        }
    }
}
