using System.Collections.Generic;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using FATester.Data;

namespace FATester.Views
{
    /// <summary>메인 화면: 시험 채널 4개 카드(상태·진행률·측정값) + 불량 분류 결과 표. 타일을 누르면 설비 상세.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        public const string ScreenTitle = "FA 시험 채널 모니터링";

        private readonly IList<ChannelInfo> _channels;

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base(ScreenTitle)
        {
            _channels = source.GetChannels();

            var groups = new Control[_channels.Count];
            for (int i = 0; i < _channels.Count; i++) groups[i] = BuildChannel(i, _channels[i]);

            // Top 도킹은 나중에 Add한 것이 위 → 표(Fill) 먼저
            Body.Controls.Add(BuildDefectGrid(source.GetDefects()));
            var title = SectionTitle("불량 분류 결과");
            title.Dock = DockStyle.Top;
            Body.Controls.Add(title);
            Body.Controls.Add(Columns(S(340), groups));

            ApplyHeaderSummary();
        }

        private Control BuildChannel(int index, ChannelInfo ch)
        {
            var group = new ApcGroup { Text = ch.Name, Dock = DockStyle.Fill };

            var tile = new ApcStatusTile { Text = ch.StateText, Status = ch.OverallStatus, StatusText = ch.Detail.Length == 0 ? "시료 없음" : ch.Detail, Dock = DockStyle.Fill };
            int captured = index;
            tile.Click += (s, e) => OpenDetail(captured);

            string progress = ch.ProgressPercent.HasValue ? ch.ProgressPercent.Value.ToString("0") : "";
            var progressCard = new ApcValueCard
            {
                Title = "진행률", Value = progress, Unit = "%", Status = ch.BaseStatus, ShowLamp = true,
                Footer = ch.SampleId.Length == 0 ? "" : "시료 " + ch.SampleId + " · " + ch.Item,
                Dock = DockStyle.Fill,
            };
            var resistance = MeasureCard("저항", ch.ResistanceMohm, "mΩ", ch.ResistanceStatus, Limits.ResistanceMohm);
            var leakage = MeasureCard("누설전류", ch.LeakageNa, "nA", ch.LeakageStatus, Limits.LeakageNa);

            // 아래 줄부터 Add (Top 도킹)
            group.Controls.Add(Columns(S(100), new Control[] { resistance, leakage }));
            group.Controls.Add(Columns(S(100), new Control[] { progressCard }));
            group.Controls.Add(Columns(S(64), new Control[] { tile }));
            return group;
        }

        private static ApcValueCard MeasureCard(string title, double? value, string unit, StatusKind status, double limit)
        {
            return new ApcValueCard
            {
                Title = title, Unit = unit, Status = status, ShowLamp = true,
                Value = value.HasValue ? value.Value.ToString("0.0") : "",
                Footer = "상한 " + limit.ToString("0.0"),
                Dock = DockStyle.Fill,
            };
        }

        private static ApcGrid BuildDefectGrid(IList<DefectRow> rows)
        {
            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn());
            grid.Columns.Add("id", "시료 ID");
            grid.Columns.Add("mode", "불량 모드");
            grid.Columns.Add("pos", "위치");
            grid.Columns.Add("time", "판정 시각");
            foreach (DataGridViewColumn c in grid.Columns)
                if (!(c is ApcLampColumn)) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (var r in rows) grid.Rows.Add(r.Status, r.SampleId, r.Mode, r.Position, r.JudgedAt);
            return grid;
        }

        private void ApplyHeaderSummary()
        {
            int alarm = 0, warn = 0, stop = 0;
            foreach (var ch in _channels)
            {
                switch (ch.OverallStatus)
                {
                    case StatusKind.Alarm: alarm++; break;
                    case StatusKind.Warn: warn++; break;
                    case StatusKind.Stop: stop++; break;
                }
                if (ch.OverallStatus == StatusKind.Alarm) Alarm.Push(ch.Name + ": " + ch.LastError + " " + ch.LastErrorText);
            }
            var parts = new List<string>();
            if (alarm > 0) parts.Add("알람 " + alarm);
            if (warn > 0) parts.Add("경고 " + warn);
            if (stop > 0) parts.Add("정지 " + stop);
            Header.Status = alarm > 0 ? StatusKind.Alarm : warn > 0 ? StatusKind.Warn : stop > 0 ? StatusKind.Stop : StatusKind.Run;
            Header.StatusText = parts.Count == 0 ? "정상" : "전체 · " + string.Join(" · ", parts.ToArray());
        }

        private void OpenDetail(int channelIndex)
        {
            using (var dlg = new ChannelDetailDialog(_channels, channelIndex)) dlg.ShowDialog(FindForm());
        }
    }
}
