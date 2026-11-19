using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using FATester.Data;

namespace FATester.Views
{
    /// <summary>메인 화면: 채널 타일 → 진행률·저항·누설전류 카드 → 불량 분류 결과 표.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("Failure Analysis 시험기")
        {
            ChannelInfo[] channels = source.GetChannels();

            var tiles = new Control[channels.Length];
            var progress = new Control[channels.Length];
            var resistance = new Control[channels.Length];
            var leakage = new Control[channels.Length];
            int alarms = 0, warns = 0, idles = 0;
            for (int i = 0; i < channels.Length; i++)
            {
                ChannelInfo ch = channels[i];
                var tile = new ApcStatusTile { Text = ch.Name, Status = ch.Status, StatusText = ch.StatusText, Dock = DockStyle.Fill };
                tile.Click += (s, e) => OpenDetail(ch);
                tiles[i] = tile;
                if (ch.Status == StatusKind.Alarm) { alarms++; Alarm.Push(ch.Name + ": " + ch.StatusText); }
                else if (ch.Status == StatusKind.Warn) warns++;
                else if (ch.Status == StatusKind.Stop) idles++;

                progress[i] = ProgressCard(ch);
                resistance[i] = ValueCard(ch.Name + " 저항", ch.ResistanceValue, ChannelInfo.Resistance, ch.ResistanceStatus);
                leakage[i] = ValueCard(ch.Name + " 누설전류", ch.LeakageValue, ChannelInfo.Leakage, ch.LeakageStatus);
            }
            SetHeader(alarms, warns, idles);

            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add("sample", "시료 ID");
            grid.Columns.Add("mode", "불량 모드");
            grid.Columns.Add("pos", "위치");
            grid.Columns.Add("time", "판정 시각");
            foreach (DataGridViewColumn c in grid.Columns) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (DefectRow d in source.GetDefects())
                grid.Rows.Add(d.SampleId, d.Mode, d.Position, d.JudgedAt.ToString("yyyy-MM-dd HH:mm:ss"));

            // Fill 먼저, Top 줄은 아래쪽 것부터
            Body.Controls.Add(grid);
            Body.Controls.Add(Row(SectionTitle("불량 분류 결과")));
            Body.Controls.Add(Columns(S(100), leakage));
            Body.Controls.Add(Columns(S(100), resistance));
            Body.Controls.Add(Columns(S(100), progress));
            Body.Controls.Add(Columns(S(90), tiles));
        }

        private void SetHeader(int alarms, int warns, int idles)
        {
            string text = "전체";
            if (alarms > 0) text += " · 알람 " + alarms;
            if (warns > 0) text += " · 경고 " + warns;
            if (idles > 0) text += " · 대기 " + idles;
            Header.StatusText = alarms + warns + idles == 0 ? "전체 · 정상" : text;
            Header.Status = alarms > 0 ? StatusKind.Alarm : (warns > 0 ? StatusKind.Warn : (idles > 0 ? StatusKind.Stop : StatusKind.Run));
        }

        private static ApcValueCard ProgressCard(ChannelInfo ch)
        {
            var card = new ApcValueCard { Title = ch.Name + " 진행률", Status = ch.ProgressStatus, ShowLamp = true, Dock = DockStyle.Fill };
            if (ch.Progress.HasValue)
            {
                card.Value = ch.Progress.Value.ToString("0.0");
                card.Unit = "%";
                card.Footer = ch.Sample + " · " + ch.Item;
            }
            else card.Footer = "대기";
            return card;
        }

        private static ApcValueCard ValueCard(string title, double? value, Limit limit, StatusKind status)
        {
            var card = new ApcValueCard { Title = title, Status = status, ShowLamp = true, Dock = DockStyle.Fill };
            if (value.HasValue)
            {
                card.Value = limit.Text(value.Value);
                card.Unit = limit.Unit;
                card.Footer = limit.Footer(value.Value);
            }
            return card;
        }

        private void OpenDetail(ChannelInfo ch)
        {
            using (var dlg = new ChannelDetailDialog(ch)) dlg.ShowDialog(FindForm());
        }
    }
}
