using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using FATester.Data;

namespace FATester.Views
{
    /// <summary>설비 상세 dialog: 채널 상태 줄 + 챔버 온도·습도·누적 사이클·최근 오류 코드. 값은 메인에서 넘겨받는다.</summary>
    internal sealed class ChannelDetailDialog : ApcForm
    {
        public ChannelDetailDialog() : this(new SampleMainViewSource().GetChannels()[0]) { }

        public ChannelDetailDialog(ChannelInfo c)
        {
            SetupDialog(c.Name + " 설비 상세", 720);

            var head = new ApcHeaderBar { Title = c.Name + (c.TestItem.Length > 0 ? " · " + c.TestItem : ""), Status = ChannelJudge.Kind(c), StatusText = ChannelJudge.Text(c) };
            AddContent(head);

            bool erroring = c.Process == ChannelProcess.Error;
            AddContentRow(
                new ApcValueCard { Title = "챔버 온도", Value = ChannelJudge.Fmt(c.ChamberTemp), Unit = "℃", Status = StatusKind.Run, Footer = "", Height = ApcTheme.Px(120) },
                new ApcValueCard { Title = "챔버 습도", Value = ChannelJudge.Fmt(c.ChamberHumidity), Unit = "%RH", Status = StatusKind.Run, Footer = "", Height = ApcTheme.Px(120) });
            AddContentRow(
                new ApcValueCard { Title = "누적 사이클", Value = c.Cycles.ToString("#,0"), Unit = "회", Status = StatusKind.Run, Footer = "", Height = ApcTheme.Px(120) },
                new ApcValueCard
                {
                    Title = "최근 오류 코드",
                    Value = c.ErrorCode,
                    Unit = "",
                    Status = erroring ? StatusKind.Alarm : StatusKind.Run,
                    Footer = c.ErrorTime + " · " + c.ErrorText,
                    Height = ApcTheme.Px(120)
                });

            var close = new ApcButton { Text = "닫기", DialogResult = DialogResult.OK };
            SetFooter(null, close);
            AcceptButton = close;
        }
    }
}
