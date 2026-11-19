using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CleanroomMonitor.Data;

namespace CleanroomMonitor.Views
{
    /// <summary>
    /// 메인 화면: 구역 센서 표(기준 이탈 강조) → 선택 구역 온도·습도 추이. 헤더에 구역 상태 요약.
    /// 데이터는 IMainViewSource에서만 받는다(기본: 샘플 데이터).
    /// </summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const int ColLamp = 0, ColTemp = 2, ColHumi = 3, ColParticle = 4;
        private const string DefaultZone = "A4";
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private readonly IMainViewSource _source;
        private readonly ApcGrid _grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true, ClearInitialSelection = false };
        private readonly ApcGroup _tempGroup = new ApcGroup { Dock = DockStyle.Fill };
        private readonly ApcGroup _humiGroup = new ApcGroup { Dock = DockStyle.Fill };
        private readonly ApcTrendChart _tempChart = new ApcTrendChart { Dock = DockStyle.Fill };
        private readonly ApcTrendChart _humiChart = new ApcTrendChart { Dock = DockStyle.Fill };
        private readonly Label _info = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent, Font = ApcTheme.GetFont(), ForeColor = ApcTheme.C(ApcTheme.Current.Colors.TextMuted) };
        private bool _loading;

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("클린룸 환경 모니터링")
        {
            _source = source;

            _grid.Columns.Add(new ApcLampColumn { Width = S(60), HeaderText = "상태" });
            AddColumn("zone", "구역", 120);
            AddColumn("temp", "온도(℃)", 170);
            AddColumn("humi", "습도(%RH)", 170);
            AddColumn("particle", "파티클 0.5µm(개/ft³)", 220);
            AddColumn("pressure", "차압(Pa)", 150);
            var state = _grid.Columns[_grid.Columns.Add("state", "내용")];
            state.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            state.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;   // 긴 글만 왼쪽
            foreach (DataGridViewColumn c in _grid.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
            _grid.SelectionChanged += (s, e) => { if (!_loading) ShowTrend(); };

            _tempGroup.Controls.Add(_tempChart);
            _humiGroup.Controls.Add(_humiChart);
            var charts = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            charts.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _tempGroup.Margin = new Padding(0, 0, S(6), 0);
            _humiGroup.Margin = new Padding(S(6), 0, 0, 0);
            charts.Controls.Add(_tempGroup, 0, 0);
            charts.Controls.Add(_humiGroup, 1, 0);

            var gridGroup = new ApcGroup { Text = "구역별 센서 현황", Dock = DockStyle.Top };
            gridGroup.Height = gridGroup.HeightFor(ApcGrid.HeightForRows(8));
            gridGroup.Controls.Add(_grid);

            var settings = Button("설정", ApcButtonVariant.Normal, 110);
            settings.Dock = DockStyle.Right;
            settings.Click += (s, e) => OpenSettings();
            var topBar = new Panel { Dock = DockStyle.Top, Height = S(44), BackColor = Color.Transparent };
            topBar.Controls.Add(_info);
            topBar.Controls.Add(settings);

            // Top 도킹은 나중에 Add한 것이 위 → Fill 먼저, 아래 줄부터
            Body.Controls.Add(charts);
            Body.Controls.Add(new Panel { Dock = DockStyle.Top, Height = S(12), BackColor = Color.Transparent });
            Body.Controls.Add(gridGroup);
            Body.Controls.Add(topBar);

            LoadGrid(DefaultZone);
            foreach (var r in _source.GetReadings())
            {
                var j = ZoneJudge.Evaluate(r, LimitOf(r.Zone));
                if (j.Status == StatusKind.Alarm) Alarm.Push(r.Zone + ": " + j.StateText.Substring("알람 · ".Length));
            }
        }

        private void AddColumn(string name, string header, int width)
        {
            var c = _grid.Columns[_grid.Columns.Add(name, header)];
            c.Width = S(width);
        }

        private ZoneLimit LimitOf(string zone) { return _source.GetLimits().First(l => l.Zone == zone); }

        private static string Fmt(double? v, string fmt) { return v.HasValue ? v.Value.ToString(fmt, Inv) : ""; }

        private void LoadGrid(string selectZone)
        {
            _loading = true;
            _grid.Rows.Clear();
            int run = 0, warn = 0, alarm = 0, stop = 0, select = 0;
            foreach (var r in _source.GetReadings())
            {
                var j = ZoneJudge.Evaluate(r, LimitOf(r.Zone));
                int i = _grid.Rows.Add(j.Status, r.Zone, Fmt(r.Temp, "0.0"), Fmt(r.Humi, "0.0"), Fmt(r.Particle, "#,0"), Fmt(r.Pressure, "0.0"), j.StateText);
                Tint(_grid.Rows[i].Cells[ColTemp], j.Temp);
                Tint(_grid.Rows[i].Cells[ColHumi], j.Humi);
                Tint(_grid.Rows[i].Cells[ColParticle], j.Particle);
                if (r.Zone == selectZone) select = i;
                switch (j.Status)
                {
                    case StatusKind.Run: run++; break;
                    case StatusKind.Warn: warn++; break;
                    case StatusKind.Alarm: alarm++; break;
                    default: stop++; break;
                }
            }
            _grid.CurrentCell = _grid.Rows[select].Cells[1];
            _grid.Rows[select].Selected = true;
            _loading = false;

            var parts = new List<string> { "전체", "정상 " + run };
            if (warn > 0) parts.Add("경고 " + warn);
            if (alarm > 0) parts.Add("알람 " + alarm);
            if (stop > 0) parts.Add("정지 " + stop);
            Header.StatusText = string.Join(" · ", parts);
            Header.Status = alarm > 0 ? StatusKind.Alarm : (warn > 0 ? StatusKind.Warn : (stop > 0 ? StatusKind.Stop : StatusKind.Run));

            _info.Text = "데이터 저장 주기 " + _source.GetSaveIntervalSec() + " s    ·    경고 기준: 상·하한 근접 (온도 "
                + ZoneJudge.TempWarnMargin.ToString("0.0", Inv) + " ℃, 습도 " + ZoneJudge.HumiWarnMargin.ToString("0.0", Inv)
                + " %RH, 파티클 상한의 " + (ZoneJudge.ParticleWarnRatio * 100).ToString("0", Inv) + " %)    ·    차압은 판정하지 않음";
            ShowTrend();
        }

        private static void Tint(DataGridViewCell cell, StatusKind kind)
        {
            if (kind != StatusKind.Warn && kind != StatusKind.Alarm) return;
            cell.Style.ForeColor = ApcTheme.Status(kind);
            cell.Style.Font = ApcTheme.GetFont(FontStyle.Bold);
        }

        private void ShowTrend()
        {
            // ApcGrid는 사용자가 누르기 전엔 CurrentRow가 없을 수 있다 → 없으면 그대로 둔다
            var row = _grid.CurrentRow;
            if (row == null) return;
            string zone = (string)row.Cells[1].Value;
            var l = LimitOf(zone);

            _tempGroup.Text = zone + " 온도 추이 (℃)";
            _humiGroup.Text = zone + " 습도 추이 (%RH)";
            _tempChart.UpperLimit = l.TempHigh;
            _tempChart.LowerLimit = l.TempLow;
            _humiChart.UpperLimit = l.HumiHigh;
            _humiChart.LowerLimit = l.HumiLow;
            _tempChart.XLabels = _humiChart.XLabels = _source.GetTrendTimeLabels();
            _tempChart.SetValues(_source.GetTemperatureTrend(zone));
            _humiChart.SetValues(_source.GetHumidityTrend(zone));
        }

        private void OpenSettings()
        {
            using (var dlg = new ZoneSettingsDialog(_source.GetLimits(), _source.GetSaveIntervalSec()))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                _source.SaveSettings(dlg.Limits, dlg.SaveIntervalSec);
                var row = _grid.CurrentRow;
                LoadGrid(row == null ? DefaultZone : (string)row.Cells[1].Value);
            }
        }
    }
}
