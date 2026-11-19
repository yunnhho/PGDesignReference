using System.Collections.Generic;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using InjectionMonitor.Data;

namespace InjectionMonitor.Views
{
    /// <summary>메인 화면. 데이터는 IMainViewSource에서만 받는다(기본: 샘플 데이터).</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const int InitialSelection = 2;   // IM-03

        private readonly IMainViewSource _source;
        private readonly IList<MachineInfo> _machines;
        private readonly ApcStatusTile[] _tiles;
        private readonly ApcValueCard _tempCard = new ApcValueCard { ShowLamp = true, Dock = DockStyle.Fill };
        private readonly ApcValueCard _pressureCard = new ApcValueCard { ShowLamp = true, Dock = DockStyle.Fill };
        private readonly ApcValueCard _cycleCard = new ApcValueCard { ShowLamp = true, Dock = DockStyle.Fill };
        private readonly ApcTrendChart _chart = new ApcTrendChart { Dock = DockStyle.Fill };
        private readonly Label _chartTitle = SectionTitle("");

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("사출 성형 라인 모니터링")
        {
            _source = source;
            _machines = source.GetMachines();

            _tiles = new ApcStatusTile[_machines.Count];
            var tileCells = new Control[_machines.Count];
            for (int i = 0; i < _machines.Count; i++)
            {
                var m = _machines[i];
                int index = i;
                var v = m.Overall();
                _tiles[i] = new ApcStatusTile { Text = m.Id, Status = v.Status, StatusText = m.StatusText(), Dock = DockStyle.Fill };
                _tiles[i].Click += (s, e) => Select(index);
                tileCells[i] = _tiles[i];
                if (v.Status == StatusKind.Alarm) Alarm.Push(m.Id + ": " + v.Detail);
            }

            ShowHeaderSummary();

            var grid = BuildAlarmGrid();
            var recipeButton = Button("레시피 편집", ApcButtonVariant.Normal, 130);
            recipeButton.Click += (s, e) => EditRecipe();

            // Top 도킹은 나중에 Add한 것이 위 → Fill 먼저, 아래 줄부터
            Body.Controls.Add(grid);
            Body.Controls.Add(Row(SectionTitle("최근 알람 이력")));
            Body.Controls.Add(Columns(S(150), new Control[] { _chart }));
            Body.Controls.Add(Row(_chartTitle));
            Body.Controls.Add(Columns(S(120), new Control[] { _tempCard, _pressureCard, _cycleCard }));
            Body.Controls.Add(Columns(S(90), tileCells));
            Body.Controls.Add(Row(SectionTitle("사출기 상태"), recipeButton));

            Select(InitialSelection);
        }

        private ApcGrid BuildAlarmGrid()
        {
            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn());
            grid.Columns.Add("time", "시각");
            grid.Columns.Add("unit", "설비");
            grid.Columns.Add("text", "내용");
            grid.Columns["text"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns["text"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            foreach (var a in _source.GetRecentAlarms()) grid.Rows.Add(a.Kind, a.Time, a.Machine, a.Text);
            return grid;
        }

        private void ShowHeaderSummary()
        {
            int alarm = 0, warn = 0, standby = 0, stopped = 0;
            foreach (var m in _machines)
            {
                var k = m.Overall().Status;
                if (k == StatusKind.Alarm) alarm++;
                else if (k == StatusKind.Warn) warn++;
                else if (k == StatusKind.Stop) { if (m.State == ProcessState.Standby) standby++; else stopped++; }
            }
            var parts = new List<string>();
            if (alarm > 0) parts.Add("알람 " + alarm);
            if (warn > 0) parts.Add("경고 " + warn);
            if (standby > 0) parts.Add("대기 " + standby);
            if (stopped > 0) parts.Add("정지 " + stopped);
            Header.StatusText = "전체 · " + (parts.Count == 0 ? "정상" : string.Join(" · ", parts));
            Header.Status = alarm > 0 ? StatusKind.Alarm : warn > 0 ? StatusKind.Warn : standby + stopped > 0 ? StatusKind.Stop : StatusKind.Run;
        }

        private void Select(int index)
        {
            for (int i = 0; i < _tiles.Length; i++) _tiles[i].Selected = i == index;
            var m = _machines[index];

            Fill(_tempCard, m.Id + " 금형 온도", m.Temp, "℃", m.Temp.HasValue ? Judgement.Temp(m.Temp.Value) : (Verdict?)null,
                "설정 " + Judgement.F(Judgement.TempSetpoint) + " · 하한 " + Judgement.F(Judgement.TempLower) + " · 상한 " + Judgement.F(Judgement.TempUpper));
            Fill(_pressureCard, m.Id + " 사출 압력", m.Pressure, "bar", m.Pressure.HasValue ? Judgement.Pressure(m.Pressure.Value) : (Verdict?)null,
                "기준 " + Judgement.F(Judgement.PressureUpper) + " 이하 · 경고 " + Judgement.F(Judgement.PressureWarnHigh) + " 이상");
            Fill(_cycleCard, m.Id + " 사이클 타임", m.Cycle, "s", m.Cycle.HasValue ? Judgement.Cycle(m.Cycle.Value) : (Verdict?)null,
                "목표 " + Judgement.CycleTarget + " · 기준 " + Judgement.F(Judgement.CycleUpper) + " 이하 · 경고 " + Judgement.F(Judgement.CycleWarnHigh) + " 이상");

            _chartTitle.Text = m.Id + " 금형 온도 추이 (℃)";
            _chart.Reference = Judgement.TempSetpoint;
            _chart.UpperLimit = Judgement.TempUpper;
            _chart.LowerLimit = Judgement.TempLower;
            _chart.XLabels = _source.GetTrendLabels();
            _chart.SetValues(_source.GetTempTrend(m.Id));
        }

        private static void Fill(ApcValueCard card, string title, double? value, string unit, Verdict? verdict, string footer)
        {
            card.Title = title;
            card.Value = value.HasValue ? Judgement.F(value.Value) : "";
            card.Unit = unit;
            card.Status = verdict.HasValue ? verdict.Value.Status : StatusKind.Stop;
            card.Footer = footer;
        }

        private void EditRecipe()
        {
            using (var dlg = new RecipeDialog(_source.GetRecipe()))
            {
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK) _source.SaveRecipe(dlg.Result);
            }
        }
    }
}
