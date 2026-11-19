using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using InjectionMonitor.Data;

namespace InjectionMonitor.Views
{
    /// <summary>메인 화면: 사출기 타일(선택) → 선택 설비 현재값 → 금형 온도 추이 → 최근 알람 이력.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private readonly IMainViewSource _source;
        private IList<Machine> _machines;
        private Recipe _recipe;
        private int _selected = 2;   // 샘플 기본 선택: IM-03

        private readonly ApcStatusTile[] _tiles;
        private readonly Label _valuesTitle = SectionTitle("");
        private readonly Label _chartTitle = SectionTitle("");
        private readonly ApcValueCard _tempCard = new ApcValueCard { Unit = "℃", ShowLamp = true, Dock = DockStyle.Fill };
        private readonly ApcValueCard _pressureCard = new ApcValueCard { Unit = "bar", ShowLamp = true, Dock = DockStyle.Fill };
        private readonly ApcValueCard _cycleCard = new ApcValueCard { Unit = "s", ShowLamp = true, Dock = DockStyle.Fill };
        private readonly ApcTrendChart _chart = new ApcTrendChart { Dock = DockStyle.Fill };

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("사출 성형 라인 모니터링")
        {
            _source = source;
            _machines = source.GetMachines();
            _recipe = source.GetRecipe();
            _chart.XLabels = new[] { "-60분", "-30분", "현재" };

            _tiles = new ApcStatusTile[_machines.Count];
            var tileCells = new Control[_machines.Count];
            for (int i = 0; i < _tiles.Length; i++)
            {
                int index = i;
                _tiles[i] = new ApcStatusTile { Dock = DockStyle.Fill };
                _tiles[i].Click += (s, e) => { _selected = index; UpdateAll(); };
                tileCells[i] = _tiles[i];
            }

            var edit = Button("레시피 편집", ApcButtonVariant.Normal);
            edit.Click += (s, e) => EditRecipe();

            var grid = BuildAlarmGrid(source.GetAlarms());
            var chartBox = new Panel { Dock = DockStyle.Top, Height = S(220), Padding = new Padding(0, 0, 0, S(12)) };
            chartBox.Controls.Add(_chart);

            // Fill 먼저, Top 줄은 아래쪽부터
            Body.Controls.Add(grid);
            Body.Controls.Add(Row(SectionTitle("최근 알람 이력")));
            Body.Controls.Add(chartBox);
            Body.Controls.Add(Row(_chartTitle));
            Body.Controls.Add(Columns(S(120), new Control[] { _tempCard, _pressureCard, _cycleCard }));
            Body.Controls.Add(Row(_valuesTitle, edit));
            Body.Controls.Add(Columns(S(90), tileCells));

            UpdateAll();
        }

        private void UpdateAll()
        {
            if (_tiles == null) return;
            int alarm = 0, warn = 0, standby = 0, stop = 0;
            while (Alarm.Count > 0) Alarm.AcknowledgeCurrent();

            for (int i = 0; i < _machines.Count; i++)
            {
                var m = _machines[i];
                StatusKind kind; string text;
                Evaluate(m, out kind, out text);
                _tiles[i].Text = m.Id; _tiles[i].Status = kind; _tiles[i].StatusText = text; _tiles[i].Selected = i == _selected;
                if (kind == StatusKind.Alarm) { alarm++; Alarm.Push(m.Id + ": " + text); }
                else if (kind == StatusKind.Warn) warn++;
                else if (m.Process == "대기") standby++;
                else if (kind == StatusKind.Stop) stop++;
            }

            var parts = new List<string>();
            if (alarm > 0) parts.Add("알람 " + alarm);
            if (warn > 0) parts.Add("경고 " + warn);
            if (standby > 0) parts.Add("대기 " + standby);
            if (stop > 0) parts.Add("정지 " + stop);
            Header.StatusText = parts.Count == 0 ? "정상" : "전체 · " + string.Join(" · ", parts.ToArray());
            Header.Status = alarm > 0 ? StatusKind.Alarm : warn > 0 ? StatusKind.Warn : parts.Count > 0 ? StatusKind.Stop : StatusKind.Run;

            ShowSelected();
        }

        private void ShowSelected()
        {
            var m = _machines[_selected];
            _valuesTitle.Text = m.Id + " 현재값";
            _chartTitle.Text = m.Id + " 금형 온도 추이 (회색 점선 = 설정값, 빨간 점선 = 상한)";

            Fill(_tempCard, m.Id + " 금형 온도", m.Temp, _recipe.TempStatus, "설정 " + F1(_recipe.SetTemp) + " · 상한 " + F1(_recipe.TempUpper));
            Fill(_pressureCard, m.Id + " 사출 압력", m.Pressure, _recipe.PressureStatus, "상한 " + F1(_recipe.MaxPressure));
            Fill(_cycleCard, m.Id + " 사이클 타임", m.Cycle, _recipe.CycleStatus, "목표 " + _recipe.CycleTarget + " · 상한 " + F1(_recipe.CycleUpper));

            _chart.Reference = _recipe.SetTemp;
            _chart.UpperLimit = _recipe.TempUpper;
            _chart.SetValues(_source.GetTempTrend(m.Id));
        }

        private static void Fill(ApcValueCard card, string title, double? value, Func<double, StatusKind> judge, string footer)
        {
            card.Title = title;
            card.Footer = footer;
            card.Value = value.HasValue ? F1(value.Value) : "";
            card.Status = value.HasValue ? judge(value.Value) : StatusKind.Stop;   // 값 없으면 판정하지 않는다
        }

        /// <summary>설비 상태 = 공정 상태와 값 판정 중 나쁜 쪽. 같으면 온도 → 압력 → 사이클 순.</summary>
        private void Evaluate(Machine m, out StatusKind kind, out string text)
        {
            if (m.Process == "대기") { kind = StatusKind.Stop; text = "대기"; return; }
            if (m.Process == "정지") { kind = StatusKind.Stop; text = "정지 · " + m.Note; return; }

            kind = StatusKind.Run; text = "가동 · 정상";
            Worst(m.Temp, _recipe.TempStatus, "금형 온도", ref kind, ref text);
            Worst(m.Pressure, _recipe.PressureStatus, "사출 압력", ref kind, ref text);
            Worst(m.Cycle, _recipe.CycleStatus, "사이클 타임", ref kind, ref text);
        }

        private static void Worst(double? v, Func<double, StatusKind> judge, string name, ref StatusKind kind, ref string text)
        {
            if (!v.HasValue) return;
            var s = judge(v.Value);
            if (s == StatusKind.Alarm && kind != StatusKind.Alarm) { kind = s; text = "알람 · " + name + " 상한 초과"; }
            else if (s == StatusKind.Warn && kind == StatusKind.Run) { kind = s; text = "경고 · " + name + " 상한 근접"; }
        }

        private void EditRecipe()
        {
            using (var dlg = new RecipeDialog(_recipe))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                _source.SaveRecipe(dlg.Result);
                _recipe = dlg.Result;
                UpdateAll();
            }
        }

        private static ApcGrid BuildAlarmGrid(IList<AlarmRow> rows)
        {
            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn());
            grid.Columns.Add("time", "시각");
            grid.Columns.Add("unit", "설비");
            grid.Columns.Add("text", "내용");
            grid.Columns["text"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns["text"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            foreach (var r in rows) grid.Rows.Add(r.Status, r.Time.ToString("HH:mm:ss"), r.Unit, r.Text);
            return grid;
        }

        private static string F1(double v) { return v.ToString("0.0", Inv); }
    }
}
