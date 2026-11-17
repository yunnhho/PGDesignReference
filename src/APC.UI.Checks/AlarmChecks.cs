using APC.UI.Controls;
using APC.UI.Theming;

namespace APC.UI.Checks
{
    internal static partial class Program
    {
        private static void RunAlarmChecks()
        {
            var bar = new ApcAlarmBar();
            Check(!bar.Visible, "alarm bar hidden when empty");
            bar.Push("금형 온도 상한 초과");
            bar.Push("원자재 부족");
            Check(bar.Visible && bar.Count == 2, "alarm bar shows queued");
            Check(bar.CurrentMessage == "금형 온도 상한 초과", "alarm bar shows oldest first");

            int acked = 0;
            bar.Acknowledged += (s, e) => acked++;
            bar.AcknowledgeCurrent();
            Check(bar.Count == 1 && acked == 1, "ack removes one and raises event");
            bar.AcknowledgeCurrent();
            Check(!bar.Visible, "alarm bar hides after last ack");
            bar.AcknowledgeCurrent();
            Check(acked == 2, "ack on empty bar is a no-op");

            var lamp = new ApcLamp { Status = StatusKind.Warn };
            Check(lamp.ImageName == "lamp_warn", "lamp image name follows status");
        }
    }
}
