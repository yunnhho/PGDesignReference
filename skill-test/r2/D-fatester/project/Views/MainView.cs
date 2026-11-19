using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using FATester.Data;

namespace FATester.Views
{
    /// <summary>메인 화면. 데이터는 IMainViewSource에서만 받는다(기본: 샘플 데이터).</summary>
    internal sealed class MainView : ApcTemplateView
    {
        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("FA 시험 현황")
        {
            ChannelInfo[] channels = source.GetChannels();

            int alarms = 0, warns = 0, stops = 0;
            var tiles = new Control[channels.Length];
            var resistance = new Control[channels.Length];
            var leakage = new Control[channels.Length];
            var channelGrid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            channelGrid.Columns.Add("ch", "채널");
            channelGrid.Columns.Add("state", "상태");
            channelGrid.Columns.Add("progress", "진행률(%)");
            channelGrid.Columns.Add("sample", "시료 ID");
            channelGrid.Columns.Add("item", "시험 항목");
            foreach (DataGridViewColumn c in channelGrid.Columns) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            for (int i = 0; i < channels.Length; i++)
            {
                ChannelInfo ch = channels[i];
                StatusKind st = ch.Status;
                if (st == StatusKind.Alarm) { alarms++; Alarm.Push(ch.Name + ": " + ch.StatusText); }
                else if (st == StatusKind.Warn) warns++;
                else if (st == StatusKind.Stop) stops++;

                var tile = new ApcStatusTile { Text = ch.Name, Status = st, StatusText = ch.StatusText, Dock = DockStyle.Fill };
                ChannelInfo captured = ch;
                tile.Click += (s, e) => OpenDetail(captured);
                tiles[i] = tile;

                string tail = ch.ProcessStatus == StatusKind.Run && ch.State == "완료" ? "최종값 · " : "";
                resistance[i] = new ApcValueCard
                {
                    Title = ch.Name + " 저항", Value = Judge.F1(ch.Resistance), Unit = "mΩ", Status = ch.ResistanceStatus,
                    Footer = tail + "기준 " + Judge.F1(Limits.ResistanceUpper) + " 이하 · 경고 " + Judge.F1(Limits.ResistanceWarn) + " 이상",
                    ShowLamp = true, Dock = DockStyle.Fill
                };
                leakage[i] = new ApcValueCard
                {
                    Title = ch.Name + " 누설전류", Value = Judge.F1(ch.Leakage), Unit = "nA", Status = ch.LeakageStatus,
                    Footer = tail + "기준 " + Judge.F1(Limits.LeakageUpper) + " 이하 · 경고 " + Judge.F1(Limits.LeakageWarn) + " 이상",
                    ShowLamp = true, Dock = DockStyle.Fill
                };
                channelGrid.Rows.Add(ch.Name, ch.State, Judge.F1(ch.Progress), ch.SampleId, ch.TestItem);
            }

            string summary = "전체";
            if (alarms > 0) summary += " · 알람 " + alarms;
            if (warns > 0) summary += " · 경고 " + warns;
            if (stops > 0) summary += " · 대기 " + stops;
            if (alarms + warns + stops == 0) summary = "전체 · 정상";
            Header.Status = alarms > 0 ? StatusKind.Alarm : warns > 0 ? StatusKind.Warn : stops > 0 ? StatusKind.Stop : StatusKind.Run;
            Header.StatusText = summary;

            var defectGrid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            defectGrid.Columns.Add("sample", "시료 ID");
            defectGrid.Columns.Add("mode", "불량 모드");
            defectGrid.Columns.Add("loc", "위치");
            defectGrid.Columns.Add("at", "판정 시각");
            foreach (DataGridViewColumn c in defectGrid.Columns) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (DefectRow r in source.GetDefects()) defectGrid.Rows.Add(r.SampleId, r.Mode, r.Location, r.JudgedAt);

            // Top 도킹은 나중에 Add한 것이 위 → Fill 먼저, 아래 줄부터 위로
            Body.Controls.Add(defectGrid);
            Body.Controls.Add(Row(SectionTitle("불량 분류 결과")));
            Body.Controls.Add(Columns(S(100), leakage));
            Body.Controls.Add(Columns(S(100), resistance));
            // 표 아래 줄 간격(12)을 위해 Panel 아래 여백으로 감싼다
            var channelBox = new Panel { Dock = DockStyle.Top, Height = ApcGrid.HeightForRows(channels.Length) + S(12), Padding = new Padding(0, 0, 0, S(12)) };
            channelBox.Controls.Add(channelGrid);
            Body.Controls.Add(channelBox);
            Body.Controls.Add(Columns(S(90), tiles));
        }

        private void OpenDetail(ChannelInfo ch)
        {
            using (var dlg = new ChannelDetailDialog(ch)) dlg.ShowDialog(FindForm());
        }
    }
}
