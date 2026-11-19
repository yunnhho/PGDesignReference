using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using FATester.Data;

namespace FATester.Views
{
    /// <summary>설비 상세 dialog: 상태 줄 + 챔버 온도·습도·누적 사이클·최근 오류 코드 + 온도 추이. 값은 메인에서 넘겨받는다.</summary>
    internal sealed class ChannelDetailDialog : ApcForm
    {
        public ChannelDetailDialog() : this(new SampleMainViewSource().GetChannels()[2]) { }

        public ChannelDetailDialog(ChannelInfo ch)
        {
            SetupDialog(ch.Name + " 설비 상세", 760, 600);

            AddContent(new ApcHeaderBar { Title = ch.Name + (ch.Item == null ? "" : " · " + ch.Item), Status = ch.Status, StatusText = ch.StatusText });
            AddContentRow(ChamberCard("챔버 온도", ch.ChamberTemp, ch.Temp), ChamberCard("챔버 습도", ch.ChamberHumi, ch.Humi));
            AddContentRow(CyclesCard(ch), ErrorCard(ch));
            AddContent(SectionTitle("챔버 온도 추이 (회색 점선 = 설정값, 빨간 점선 = 상한)"));

            var chart = new ApcTrendChart
            {
                Reference = ch.ChamberTemp.SetPoint,
                UpperLimit = ch.ChamberTemp.Upper,
                XLabels = new[] { "-30분", "-15분", "현재" }
            };
            chart.SetValues(ch.TempTrend);
            AddContentFill(chart);

            var close = new ApcButton { Text = "닫기", DialogResult = DialogResult.OK };
            SetFooter(null, close);
            AcceptButton = close;
        }

        private static ApcValueCard ChamberCard(string title, Limit limit, double? value)
        {
            var card = new ApcValueCard { Title = title, Height = ApcTheme.Px(110), ShowLamp = true };
            if (!value.HasValue) { card.Status = StatusKind.Stop; return card; }
            card.Value = limit.Text(value.Value);
            card.Unit = limit.Unit;
            card.Status = limit.Judge(value.Value);
            card.Footer = "설정 " + limit.Text(limit.SetPoint.Value) + " · " + limit.Footer(value.Value);
            return card;
        }

        private static ApcValueCard CyclesCard(ChannelInfo ch)
        {
            var card = new ApcValueCard { Title = "누적 사이클", Height = ApcTheme.Px(110), ShowLamp = true };
            // 판정 기준이 없는 값: 갱신 중이면 Run, 멈췄으면 Stop
            card.Status = ch.ProgressStatus;
            if (ch.Cycles.HasValue) { card.Value = ch.Cycles.Value.ToString("#,0"); card.Unit = "회"; }
            return card;
        }

        private static ApcValueCard ErrorCard(ChannelInfo ch)
        {
            var card = new ApcValueCard { Title = "최근 오류 코드", Height = ApcTheme.Px(110), ShowLamp = true };
            if (ch.ErrorCode == null) { card.Status = StatusKind.Stop; card.Footer = "최근 오류 없음"; return card; }
            card.Value = ch.ErrorCode;
            card.Footer = ch.ErrorName + " · " + ch.ErrorTime;
            // 지금 진행 중인 오류면 Alarm, 지난 기록이면 Info
            card.Status = ch.Process == ProcessState.Error ? StatusKind.Alarm : StatusKind.Info;
            return card;
        }
    }
}
