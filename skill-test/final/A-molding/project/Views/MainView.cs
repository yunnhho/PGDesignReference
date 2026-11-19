using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using MoldingLineMonitor.Data;

namespace MoldingLineMonitor.Views
{
    /// <summary>메인 화면: 사출기 타일(선택) → 선택 설비 현재값·금일 집계 카드 → 사출 압력 추이 → 최근 알람 이력.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const string DefaultSelected = "IM-04";
        private const int TileHeight = 76;

        private readonly IMainViewSource _source;
        private readonly IList<MachineRow> _machines;
        private readonly ApcStatusTile[] _tiles;
        private readonly ApcValueCard _mold = Card(), _pressure = Card(), _clamp = Card(), _cycle = Card();
        private readonly ApcTrendChart _chart = new ApcTrendChart { Dock = DockStyle.Fill };
        private readonly Label _chartTitle = SectionTitle("");
        private readonly ApcGrid _grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
        private readonly IList<AlarmRow> _alarms;

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("사출 성형 라인 모니터링")
        {
            _source = source;
            _machines = source.GetMachines();
            _alarms = source.GetAlarmHistory();

            Header.Status = StatusKind.Run;
            Header.StatusText = "정상";
            _tiles = BuildTiles();
            BuildHistoryGrid();
            UpdateHeader();

            _chart.Reference = Criterion.PressureSet;
            _chart.UpperLimit = 150.0;
            _chart.XLabels = new[] { "-20분", "-10분", "현재" };

            int produced = source.GetProducedToday(), defects = source.GetDefectsToday();
            var producedCard = new ApcValueCard
            {
                Title = "금일 생산 수량", Value = produced.ToString("#,0", CultureInfo.InvariantCulture), Unit = "개",
                Status = StatusKind.Run, ShowLamp = true, Dock = DockStyle.Fill,
                Footer = "양품 " + (produced - defects).ToString("#,0", CultureInfo.InvariantCulture) + " · 불량 " + defects.ToString("#,0", CultureInfo.InvariantCulture)
            };
            double rate = produced == 0 ? 0 : 100.0 * defects / produced;
            var rateCard = new ApcValueCard
            {
                Title = "금일 불량률", Value = rate.ToString("0.0", CultureInfo.InvariantCulture), Unit = "%",
                Status = StatusKind.Run, ShowLamp = true, Dock = DockStyle.Fill,
                Footer = "불량 " + defects.ToString("#,0", CultureInfo.InvariantCulture) + " / 생산 " + produced.ToString("#,0", CultureInfo.InvariantCulture)
            };

            var recipe = Button("레시피 편집", ApcButtonVariant.Normal);
            recipe.Click += (s, e) => OpenRecipe();
            var confirm = Button("알람 확인", ApcButtonVariant.Primary);
            confirm.Click += (s, e) => OpenAlarmConfirm();

            // Top 도킹은 나중에 Add한 것이 위 → 이력(Fill) 먼저, 그다음 아래 줄부터
            Body.Controls.Add(_grid);
            Body.Controls.Add(Row(SectionTitle("최근 알람 이력"), confirm));
            Body.Controls.Add(Columns(S(190), new Control[] { _chart }));
            Body.Controls.Add(Row(_chartTitle, recipe));
            Body.Controls.Add(Columns(S(120), new Control[] { _mold, _pressure, _clamp, _cycle, producedCard, rateCard }));
            Body.Controls.Add(Columns(S(TileHeight), new Control[] { _tiles[4], _tiles[5], _tiles[6], _tiles[7] }));
            Body.Controls.Add(Columns(S(TileHeight), new Control[] { _tiles[0], _tiles[1], _tiles[2], _tiles[3] }));

            Select(DefaultSelected);
        }

        private static ApcValueCard Card()
        {
            return new ApcValueCard { ShowLamp = true, Dock = DockStyle.Fill };
        }

        private ApcStatusTile[] BuildTiles()
        {
            var tiles = new ApcStatusTile[_machines.Count];
            for (int i = 0; i < tiles.Length; i++)
            {
                var m = _machines[i];
                var state = MachineState.Evaluate(m);
                var tile = new ApcStatusTile { Text = m.Name, Status = state.Status, StatusText = state.Text, Dock = DockStyle.Fill };
                string name = m.Name;
                tile.Click += (s, e) => Select(name);
                tiles[i] = tile;
                if (state.Status == StatusKind.Alarm) Alarm.Push(m.Name + ": " + state.Text);
            }
            return tiles;
        }

        // 헤더 = 전체 범위의 정상 외 상태 수. 순서: 알람 → 경고 → 정지 → 통신 끊김
        private void UpdateHeader()
        {
            int alarm = 0, warn = 0, stop = 0, offline = 0;
            foreach (var m in _machines)
            {
                var st = MachineState.Evaluate(m);
                if (m.Process == MachineRow.Offline) offline++;
                else if (m.Process == MachineRow.Stopped) stop++;
                else if (st.Status == StatusKind.Alarm) alarm++;
                else if (st.Status == StatusKind.Warn) warn++;
            }
            var parts = new List<string> { "전체" };
            if (alarm > 0) parts.Add("알람 " + alarm);
            if (warn > 0) parts.Add("경고 " + warn);
            if (stop > 0) parts.Add("정지 " + stop);
            if (offline > 0) parts.Add("통신 끊김 " + offline);
            Header.StatusText = parts.Count == 1 ? "전체 · 정상" : string.Join(" · ", parts.ToArray());
            Header.Status = alarm > 0 ? StatusKind.Alarm : warn > 0 ? StatusKind.Warn : stop + offline > 0 ? StatusKind.Stop : StatusKind.Run;
        }

        private void Select(string name)
        {
            MachineRow sel = null;
            for (int i = 0; i < _tiles.Length; i++)
            {
                bool on = _machines[i].Name == name;
                _tiles[i].Selected = on;
                if (on) sel = _machines[i];
            }
            if (sel == null) return;

            bool running = sel.Process == MachineRow.Running;
            SetCard(_mold, sel, Criterion.Mold, sel.Mold, running);
            SetCard(_pressure, sel, Criterion.Pressure, sel.Pressure, running);
            SetCard(_clamp, sel, Criterion.Clamp, sel.Clamp, running);
            SetCard(_cycle, sel, Criterion.Cycle, sel.Cycle, running);
            _chartTitle.Text = sel.Name + " 사출 압력 추이 (bar, 회색 점선 = 설정값, 빨간 점선 = 상한)";
            _chart.SetValues(_source.GetPressureTrend(sel.Name));
        }

        // 값이 없으면 빈칸 + Stop(판정 안 함), 정지 설비의 읽은 값도 판정 안 함, 가동 중이면 판정 함수 하나.
        private static void SetCard(ApcValueCard card, MachineRow m, Criterion c, double? v, bool running)
        {
            card.Title = m.Name + " " + c.Name;
            card.Unit = c.Unit;
            card.Footer = c.Footer;
            card.Value = v == null ? "" : c.Text(v.Value);
            card.Status = v == null || !running ? StatusKind.Stop : c.Judge(v.Value).Status;
        }

        private void BuildHistoryGrid()
        {
            _grid.Columns.Add(new ApcLampColumn());
            _grid.Columns.Add("time", "시각");
            _grid.Columns.Add("unit", "설비");
            _grid.Columns.Add("text", "내용");
            _grid.Columns["text"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _grid.Columns["text"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            foreach (var a in _alarms) _grid.Rows.Add(a.Status, a.Time, a.Machine, a.Text);
        }

        private void OpenRecipe()
        {
            using (var dlg = new RecipeDialog())
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                // TODO: dlg.MoldSet / PressureUpper / ClampUpper / CycleTarget 를 데이터 인터페이스에 넘겨 저장한다.
            }
        }

        private void OpenAlarmConfirm()
        {
            var row = _grid.CurrentRow;
            if (row == null) return;
            var a = _alarms[row.Index];
            using (var dlg = new AlarmConfirmDialog(a))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                // TODO: dlg.Memo 를 데이터 인터페이스에 넘겨 조치 이력으로 저장한다.
            }
        }
    }
}
