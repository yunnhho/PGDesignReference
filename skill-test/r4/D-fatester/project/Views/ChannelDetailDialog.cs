using APC.UI.Controls;
using APC.UI.Theming;
using FATester.Data;

namespace FATester.Views
{
    /// <summary>설비 상세 dialog: 선택한 채널의 챔버 온도·습도·누적 사이클·최근 오류 코드. 값은 메인 화면에서 넘겨받는다. 매개변수 없는 생성자는 render용 샘플.</summary>
    internal sealed class ChannelDetailDialog : ApcForm
    {
        public ChannelDetailDialog() : this(new SampleMainViewSource().GetChannels()[2]) { }

        public ChannelDetailDialog(ChannelRow ch)
        {
            SetupDialog(ch.Name + " 설비 상세", 640);

            string item = string.IsNullOrEmpty(ch.TestItem) ? "" : " · " + ch.TestItem;
            var head = new ApcHeaderBar { Title = ch.Name + item, Status = ch.Status, StatusText = ch.StatusText };

            // 판정 기준이 없는 값: 갱신 중이면 Run, 멈췄으면 Stop
            StatusKind live = ch.ProcessText == "시험중" ? StatusKind.Run : StatusKind.Stop;

            var temp = new ApcValueCard { Title = "챔버 온도", Value = Fmt(ch.ChamberTemp), Unit = "℃", Status = live, ShowLamp = true };
            var humi = new ApcValueCard { Title = "챔버 습도", Value = Fmt(ch.ChamberHumi), Unit = "%RH", Status = live, ShowLamp = true };
            var cycles = new ApcValueCard { Title = "누적 사이클", Value = ch.Cycles.ToString("#,0"), Unit = "회", Status = live, ShowLamp = true };

            bool hasError = !string.IsNullOrEmpty(ch.LastErrorCode);
            StatusKind errKind = ch.ProcessStatus == StatusKind.Alarm ? StatusKind.Alarm : StatusKind.Stop;
            var error = new ApcValueCard
            {
                Title = "최근 오류 코드",
                Value = hasError ? ch.LastErrorCode : "",
                Footer = hasError ? ch.LastErrorText + " · " + ch.LastErrorTime : "최근 오류 없음",
                Status = hasError ? errKind : StatusKind.Stop,
                ShowLamp = true
            };

            AddContent(head);
            AddContentRow(temp, humi);
            AddContentRow(cycles, error);

            var close = new ApcButton { Text = "닫기", DialogResult = System.Windows.Forms.DialogResult.OK };
            SetFooter(null, close);
            AcceptButton = close;
        }

        private static string Fmt(double? v) { return v.HasValue ? v.Value.ToString("0.0") : ""; }
    }
}
