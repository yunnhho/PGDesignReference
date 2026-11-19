using System;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using InjectionMonitor.Data;

namespace InjectionMonitor.Views
{
    /// <summary>사출 성형 라인 모니터링: 설비 타일 → 선택 설비 현재값 카드 → 금형 온도 추이 → 최근 알람 이력.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private readonly IMainViewSource _source;
        private readonly MachineInfo[] _machines;
        private readonly ApcStatusTile[] _tiles;
        private readonly ApcValueCard _moldCard = Card("금형 온도"), _pressureCard = Card("사출 압력"), _cycleCard = Card("사이클 타임");
        private readonly ApcTrendChart _chart = new ApcTrendChart { Dock = DockStyle.Fill };
        private readonly Label _selected = Caption("");
        private readonly Label _chartTitle = SectionTitle("");

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("사출 성형 라인 모니터링")
        {
            _source = source;
            _machines = source.GetMachines();
            _tiles = new ApcStatusTile[_machines.Length];
            for (int i = 0; i < _machines.Length; i++)
            {
                var m = _machines[i];
                int index = i;
                _tiles[i] = new ApcStatusTile { Text = m.Id, Status = m.Status, StatusText = m.StatusText, Dock = DockStyle.Fill };
                _tiles[i].Click += (s, e) => Select(index);
                if (m.Status == StatusKind.Alarm) Alarm.Push(m.Id + ": " + m.StatusText);
            }
            UpdateHeader();

            var edit = Button("레시피 편집", ApcButtonVariant.Normal, 140);
            edit.Click += (s, e) => EditRecipe();

            var chartPanel = new Panel { Dock = DockStyle.Top, Height = S(230) };
            _chart.XLabels = source.GetTrendLabels();
            _chart.Reference = Judging.MoldTempSet;
            _chart.UpperLimit = Judging.MoldTempAlarm;
            chartPanel.Controls.Add(_chart);
            _chartTitle.Dock = DockStyle.Top;
            chartPanel.Controls.Add(_chartTitle);

            Body.Controls.Add(BuildAlarmGrid());
            var alarmTitle = SectionTitle("최근 알람 이력");
            alarmTitle.Dock = DockStyle.Top;
            Body.Controls.Add(alarmTitle);
            Body.Controls.Add(chartPanel);
            Body.Controls.Add(Columns(S(120), new Control[] { _moldCard, _pressureCard, _cycleCard }));
            Body.Controls.Add(Row(_selected, edit));
            Body.Controls.Add(Columns(S(90), _tiles));

            Select(2 < _machines.Length ? 2 : 0);
        }

        private static ApcValueCard Card(string title)
        {
            return new ApcValueCard { Title = title, ShowLamp = true, Dock = DockStyle.Fill };
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
            foreach (var a in _source.GetRecentAlarms())
                grid.Rows.Add(a.Kind, a.Time.ToString("HH:mm:ss"), a.MachineId, a.Text);
            return grid;
        }

        private void UpdateHeader()
        {
            int alarm = 0, warn = 0, wait = 0, stop = 0;
            foreach (var m in _machines)
            {
                if (m.Status == StatusKind.Alarm) alarm++;
                else if (m.Status == StatusKind.Warn) warn++;
                else if (m.RunState == "대기") wait++;
                else if (m.RunState == "정지") stop++;
            }
            string text = (alarm > 0 ? " · 알람 " + alarm : "") + (warn > 0 ? " · 경고 " + warn : "")
                        + (wait > 0 ? " · 대기 " + wait : "") + (stop > 0 ? " · 정지 " + stop : "");
            Header.Status = alarm > 0 ? StatusKind.Alarm : warn > 0 ? StatusKind.Warn : (wait + stop) > 0 ? StatusKind.Stop : StatusKind.Run;
            Header.StatusText = text.Length == 0 ? "정상" : "전체" + text;
        }

        private void Select(int index)
        {
            var m = _machines[index];
            _selected.Text = "선택 설비  " + m.Id + " (" + m.StatusText + ")";
            _chartTitle.Text = m.Id + " 금형 온도 추이 (℃)";

            Fill(_moldCard, m.MoldTemp, "0.0", "℃", m.MoldTemp.HasValue ? Judging.MoldTemp(m.MoldTemp.Value) : StatusKind.Stop,
                 "설정 " + Judging.MoldTempSet.ToString("0.0") + " · 상한 " + Judging.MoldTempAlarm.ToString("0.0"));
            Fill(_pressureCard, m.Pressure, "0.0", "bar", m.Pressure.HasValue ? Judging.Pressure(m.Pressure.Value) : StatusKind.Stop,
                 "상한 " + Judging.PressureAlarm.ToString("0.0"));
            Fill(_cycleCard, m.CycleTime, "0.0", "s", m.CycleTime.HasValue ? Judging.Cycle(m.CycleTime.Value) : StatusKind.Stop,
                 "목표 " + Judging.CycleTarget.ToString("0"));
            // 정지·대기 설비는 카드 테두리도 설비 상태(Stop)
            if (m.RunState != "가동") { _moldCard.Status = _pressureCard.Status = _cycleCard.Status = StatusKind.Stop; }

            _chart.SetValues(_source.GetMoldTempTrend(m.Id));
        }

        private static void Fill(ApcValueCard card, double? value, string format, string unit, StatusKind status, string footer)
        {
            card.Value = value.HasValue ? value.Value.ToString(format) : "";
            card.Unit = value.HasValue ? unit : "";
            card.Status = status;
            card.Footer = footer;
        }

        private void EditRecipe()
        {
            var r = _source.GetRecipe();
            using (var dlg = new RecipeDialog(r.MoldTempSet, r.PressureLimit, r.CycleTarget))
            {
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    _source.SaveRecipe(new RecipeValues(dlg.MoldTempSet, dlg.PressureLimit, dlg.CycleTarget));
            }
        }
    }
}
