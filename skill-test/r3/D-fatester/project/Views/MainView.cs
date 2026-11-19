using System.Collections.Generic;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using FATester.Data;

namespace FATester.Views
{
    /// <summary>메인 화면: 채널 타일 → 진행률 → 저항 → 누설전류 카드 줄 → 불량 분류 결과 표.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("FA 시험기")
        {
            IList<ChannelInfo> channels = source.GetChannels();

            var tiles = new List<Control>();
            var progress = new List<Control>();
            var resistance = new List<Control>();
            var leakage = new List<Control>();
            int alarms = 0, warns = 0, waits = 0;
            foreach (ChannelInfo c in channels)
            {
                StatusKind kind = ChannelJudge.Kind(c);
                if (kind == StatusKind.Alarm) alarms++;
                else if (kind == StatusKind.Warn) warns++;
                else if (c.Process == ChannelProcess.Waiting) waits++;
                if (c.Process == ChannelProcess.Error) Alarm.Push(c.Name + " 오류 " + c.ErrorCode + " " + c.ErrorText);

                ChannelInfo channel = c;
                var tile = new ApcStatusTile { Text = c.Name, Status = kind, StatusText = ChannelJudge.Text(c), Dock = DockStyle.Fill };
                tile.Click += (s, e) => OpenDetail(channel);
                tiles.Add(tile);

                progress.Add(new ApcValueCard
                {
                    Title = c.Name + " 진행률",
                    Value = c.ProgressPercent.HasValue ? c.ProgressPercent.Value.ToString("#,0") : "",
                    Unit = "%",
                    Status = c.ProgressPercent.HasValue ? StatusKind.Run : StatusKind.Stop,
                    Footer = c.SampleId.Length > 0 ? c.SampleId + " · " + c.TestItem : "시료 없음",
                    ShowLamp = true,
                    Dock = DockStyle.Fill
                });
                resistance.Add(MeasureCard(c.Name + " 저항", c.ResistanceMOhm, "mΩ", ChannelJudge.Resistance(c.ResistanceMOhm), ChannelJudge.ResistanceCriteria));
                leakage.Add(MeasureCard(c.Name + " 누설전류", c.LeakageNA, "nA", ChannelJudge.Leakage(c.LeakageNA), ChannelJudge.LeakageCriteria));
            }

            Header.Status = alarms > 0 ? StatusKind.Alarm : warns > 0 ? StatusKind.Warn : StatusKind.Run;
            Header.StatusText = SummaryText(alarms, warns, waits);

            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add("id", "시료 ID");
            grid.Columns.Add("mode", "불량 모드");
            grid.Columns.Add("loc", "위치");
            grid.Columns.Add("time", "판정 시각");
            foreach (DataGridViewColumn col in grid.Columns) col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (DefectRow d in source.GetDefects()) grid.Rows.Add(d.SampleId, d.Mode, d.Location, d.JudgedAt);

            var title = SectionTitle("불량 분류 결과 (금일)");
            title.Dock = DockStyle.Top;

            Body.Controls.Add(grid);
            Body.Controls.Add(title);
            Body.Controls.Add(Columns(S(120), leakage.ToArray()));
            Body.Controls.Add(Columns(S(120), resistance.ToArray()));
            Body.Controls.Add(Columns(S(120), progress.ToArray()));
            Body.Controls.Add(Columns(S(90), tiles.ToArray()));
        }

        private static ApcValueCard MeasureCard(string title, double? value, string unit, StatusKind status, string criteria)
        {
            return new ApcValueCard
            {
                Title = title,
                Value = value.HasValue ? ChannelJudge.Fmt(value.Value) : "",
                Unit = unit,
                Status = status,
                Footer = criteria,
                ShowLamp = true,
                Dock = DockStyle.Fill
            };
        }

        private static string SummaryText(int alarms, int warns, int waits)
        {
            var parts = new List<string> { "전체" };
            if (alarms > 0) parts.Add("알람 " + alarms);
            if (warns > 0) parts.Add("경고 " + warns);
            if (waits > 0) parts.Add("대기 " + waits);
            if (parts.Count == 1) parts.Add("정상");
            return string.Join(" · ", parts.ToArray());
        }

        private void OpenDetail(ChannelInfo channel)
        {
            using (var dlg = new ChannelDetailDialog(channel)) dlg.ShowDialog(FindForm());
        }
    }
}
