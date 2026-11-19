using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using InjectionMonitor.Data;

namespace InjectionMonitor.Views
{
    /// <summary>메인 화면. 사출기 타일 → 선택 설비 현재값 카드 → 금형 온도 추이 → 최근 알람 이력.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const int InitialSelected = 2;   // IM-03

        private readonly IMainViewSource _source;
        private readonly MachineInfo[] _machines;
        private readonly ApcStatusTile[] _tiles;
        private readonly ApcValueCard _tempCard = Card("금형 온도");
        private readonly ApcValueCard _pressureCard = Card("사출 압력");
        private readonly ApcValueCard _cycleCard = Card("사이클 타임");
        private readonly ApcTrendChart _chart = new ApcTrendChart { Dock = DockStyle.Fill };
        private readonly Label _trendTitle = SectionTitle("");

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("사출 성형 라인 모니터링")
        {
            _source = source;
            _machines = source.GetMachines();
            _tiles = new ApcStatusTile[_machines.Length];

            int alarms = 0, warns = 0, stops = 0, waits = 0;
            var tileCells = new Control[_machines.Length];
            for (int i = 0; i < _machines.Length; i++)
            {
                var m = _machines[i];
                string text;
                StatusKind kind = Judge.Machine(m, out text);
                int index = i;
                var tile = new ApcStatusTile { Text = m.Id, Status = kind, StatusText = text, Dock = DockStyle.Fill };
                tile.Click += (s, e) => Select(index);
                _tiles[i] = tile;
                tileCells[i] = tile;
                if (kind == StatusKind.Alarm) { alarms++; Alarm.Push(m.Id + ": " + text); }
                else if (kind == StatusKind.Warn) warns++;
                else if (m.Process == "정지") stops++;
                else if (kind == StatusKind.Stop) waits++;
            }
            SetHeaderSummary(alarms, warns, stops, waits);

            var recipe = Button("레시피 편집", ApcButtonVariant.Normal, 130);
            recipe.Click += (s, e) => EditRecipe();

            var grid = BuildHistory(source.GetAlarmHistory());
            _chart.Reference = Judge.TempSetpoint;
            _chart.UpperLimit = Judge.TempUpper;
            _chart.LowerLimit = Judge.TempLower;
            _chart.XLabels = new[] { "13:55", "14:20", "14:50" };

            // Top 도킹은 나중에 Add한 것이 위
            Body.Controls.Add(grid);
            Body.Controls.Add(Row(SectionTitle("최근 알람 이력")));
            Body.Controls.Add(Columns(S(190), new Control[] { _chart }));
            Body.Controls.Add(Row(_trendTitle, recipe));
            Body.Controls.Add(Columns(S(120), new Control[] { _tempCard, _pressureCard, _cycleCard }));
            Body.Controls.Add(Columns(S(90), tileCells));

            Select(InitialSelected);
        }

        private static ApcValueCard Card(string title)
        {
            return new ApcValueCard { Title = title, ShowLamp = true, Dock = DockStyle.Fill };
        }

        private void SetHeaderSummary(int alarms, int warns, int stops, int waits)
        {
            string text = "전체";
            if (alarms > 0) text += " · 알람 " + alarms;
            if (warns > 0) text += " · 경고 " + warns;
            if (stops > 0) text += " · 정지 " + stops;
            if (waits > 0) text += " · 대기 " + waits;
            if (text == "전체") text = "전체 · 정상";
            Header.StatusText = text;
            Header.Status = alarms > 0 ? StatusKind.Alarm : warns > 0 ? StatusKind.Warn : (stops + waits) > 0 ? StatusKind.Stop : StatusKind.Run;
        }

        private void Select(int index)
        {
            for (int i = 0; i < _tiles.Length; i++) _tiles[i].Selected = i == index;
            var m = _machines[index];
            _trendTitle.Text = "금형 온도 추이 · " + m.Id;

            string r;
            _tempCard.Value = m.Temp.HasValue ? Judge.F1(m.Temp.Value) : "";
            _tempCard.Unit = "℃";
            _tempCard.Status = m.Temp.HasValue ? Judge.Temp(m.Temp.Value, out r) : StatusKind.Stop;
            _tempCard.Footer = "설정 " + Judge.F1(Judge.TempSetpoint) + " · 상한 " + Judge.F1(Judge.TempUpper);

            _pressureCard.Value = m.Pressure.HasValue ? Judge.F1(m.Pressure.Value) : "";
            _pressureCard.Unit = "bar";
            _pressureCard.Status = m.Pressure.HasValue ? Judge.Pressure(m.Pressure.Value, out r) : StatusKind.Stop;
            _pressureCard.Footer = "상한 " + Judge.F1(Judge.PressureUpper) + " · 경고 " + Judge.F1(Judge.PressureUpperWarn) + " 이상";

            _cycleCard.Value = m.Cycle.HasValue ? Judge.F1(m.Cycle.Value) : "";
            _cycleCard.Unit = "s";
            _cycleCard.Status = m.Cycle.HasValue ? Judge.Cycle(m.Cycle.Value, out r) : StatusKind.Stop;
            _cycleCard.Footer = "목표 " + Judge.F0(Judge.CycleTarget) + " · 상한 " + Judge.F0(Judge.CycleUpper);

            _chart.SetValues(m.Trend);
        }

        private static ApcGrid BuildHistory(AlarmRecord[] records)
        {
            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn());
            grid.Columns.Add("time", "시각");
            grid.Columns.Add("machine", "설비");
            grid.Columns.Add("text", "내용");
            grid.Columns["time"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns["machine"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns["text"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns["text"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            foreach (var r in records) grid.Rows.Add(r.Status, r.Time, r.Machine, r.Text);
            return grid;
        }

        private void EditRecipe()
        {
            using (var dlg = new RecipeDialog())
            {
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    _source.SaveRecipe(dlg.TempSetpoint, dlg.PressureUpper, dlg.CycleTarget);
            }
        }
    }
}
