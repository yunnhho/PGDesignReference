using System.Collections.Generic;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using InjectionMonitor.Data;

namespace InjectionMonitor.Views
{
    /// <summary>
    /// 사출 성형 라인 모니터링: 설비 타일 6개(누르면 선택) → 선택 설비 현재값 카드 3개 → 금형 온도 추이 → 최근 알람 이력.
    /// 데이터는 IMainViewSource에서만 받는다(기본: 샘플 데이터).
    /// </summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const int DefaultSelected = 2;   // 샘플 화면은 IM-03(경고)을 선택해 보여 준다

        private readonly IMainViewSource _source;
        private readonly IList<Machine> _machines;
        private readonly ApcStatusTile[] _tiles;
        private readonly Label _selectedTitle = SectionTitle("");
        private readonly Label _chartTitle = SectionTitle("");
        private readonly ApcValueCard _tempCard = new ApcValueCard { Title = "금형 온도", Unit = "℃", ShowLamp = true, Dock = DockStyle.Fill };
        private readonly ApcValueCard _pressureCard = new ApcValueCard { Title = "사출 압력", Unit = "bar", ShowLamp = true, Dock = DockStyle.Fill };
        private readonly ApcValueCard _cycleCard = new ApcValueCard { Title = "사이클 타임", Unit = "s", ShowLamp = true, Dock = DockStyle.Fill };
        private readonly ApcTrendChart _chart = new ApcTrendChart { Dock = DockStyle.Fill, Reference = (double)Criteria.TempSet, UpperLimit = (double)Criteria.TempUpper, XLabels = new[] { "-60분", "-30분", "현재" } };
        private Machine _selected;

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
                var j = Criteria.JudgeMachine(m);
                var tile = new ApcStatusTile { Text = m.Name, Status = j.Status, StatusText = Criteria.TileText(m), Dock = DockStyle.Fill };
                var picked = m;
                tile.Click += (s, e) => Select(picked);
                _tiles[i] = tile;
                tileCells[i] = tile;
                if (j.Status == StatusKind.Alarm) Alarm.Push(m.Name + " " + j.Detail);
            }
            SetHeaderSummary();

            var history = BuildHistoryGrid(source.GetAlarmHistory());
            var edit = Button("레시피 편집", ApcButtonVariant.Normal);
            edit.Click += (s, e) => EditRecipe();

            // Top 도킹은 나중에 Add한 것이 위 → 이력(Fill) 먼저, 그다음 아래 줄부터
            Body.Controls.Add(history);
            Body.Controls.Add(Row(SectionTitle("최근 알람 이력")));
            Body.Controls.Add(Columns(S(200), new Control[] { _chart }));
            Body.Controls.Add(Row(_chartTitle));
            Body.Controls.Add(Columns(S(120), new Control[] { _tempCard, _pressureCard, _cycleCard }));
            Body.Controls.Add(Row(_selectedTitle, edit));
            Body.Controls.Add(Columns(S(90), tileCells));

            Select(_machines[DefaultSelected]);
        }

        private void SetHeaderSummary()
        {
            int alarm = 0, warn = 0, standby = 0, stopped = 0;
            foreach (var m in _machines)
            {
                var j = Criteria.JudgeMachine(m);
                if (m.Process == ProcessState.Standby) standby++;
                else if (m.Process == ProcessState.Stopped) stopped++;
                else if (j.Status == StatusKind.Alarm) alarm++;
                else if (j.Status == StatusKind.Warn) warn++;
            }
            var parts = new List<string> { "전체" };
            if (alarm > 0) parts.Add("알람 " + alarm);
            if (warn > 0) parts.Add("경고 " + warn);
            if (standby > 0) parts.Add("대기 " + standby);
            if (stopped > 0) parts.Add("정지 " + stopped);
            if (parts.Count == 1) parts.Add("정상");
            Header.StatusText = string.Join(" · ", parts.ToArray());
            Header.Status = alarm > 0 ? StatusKind.Alarm : warn > 0 ? StatusKind.Warn : standby + stopped > 0 ? StatusKind.Stop : StatusKind.Run;
        }

        private void Select(Machine m)
        {
            _selected = m;
            for (int i = 0; i < _tiles.Length; i++) _tiles[i].Selected = _machines[i] == m;

            _selectedTitle.Text = m.Name + " 현재값";
            _chartTitle.Text = m.Name + " 금형 온도 추이 (회색 점선 = 설정값, 빨간 점선 = 상한)";

            Fill(_tempCard, m.Temp, Criteria.JudgeTemp, "0.0", "설정 " + Criteria.TempSet.ToString("0.0") + " · 상한 " + Criteria.TempUpper.ToString("0.0"));
            Fill(_pressureCard, m.Pressure, Criteria.JudgePressure, "0.0", "상한 " + Criteria.PressureUpper.ToString("0.0"));
            Fill(_cycleCard, m.Cycle, Criteria.JudgeCycle, "0.0", "목표 " + Criteria.CycleTarget + " · 상한 " + Criteria.CycleUpper.ToString("0.0"));
            _chart.SetValues(m.TempTrend);
        }

        private static void Fill(ApcValueCard card, decimal? value, System.Func<decimal, Judgement> judge, string format, string footer)
        {
            card.Value = value.HasValue ? value.Value.ToString(format) : "";
            card.Status = Criteria.Card(value, judge).Status;
            card.Footer = footer;
        }

        private void EditRecipe()
        {
            using (var dlg = new RecipeDialog(_selected.Name, Criteria.TempSet, Criteria.PressureUpper, Criteria.CycleTarget))
            {
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    _source.SaveRecipe(_selected.Name, dlg.TempSet, dlg.PressureUpper, dlg.CycleTarget);
            }
        }

        private static ApcGrid BuildHistoryGrid(IList<AlarmRow> rows)
        {
            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn());
            grid.Columns.Add("time", "시각");
            grid.Columns.Add("unit", "설비");
            grid.Columns.Add("text", "내용");
            grid.Columns["time"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns["unit"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns["text"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns["text"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;   // 긴 글만 왼쪽
            foreach (var r in rows) grid.Rows.Add(r.Status, r.Time, r.Machine, r.Text);
            return grid;
        }
    }
}
