using System.Drawing;
using APC.UI.Theming;

namespace APC.UI.Drawing
{
    /// <summary>카드형 표시 컨트롤(값 카드·설비 타일) 공통 규칙.</summary>
    internal static class Cards
    {
        /// <summary>값·상태 글자색: 경고/알람만 상태색, 정지는 흐리게, 나머지는 기본 글자색. 정상까지 초록으로 칠하면 이상이 안 띈다.</summary>
        internal static Color EmphasisColor(StatusKind status)
        {
            var c = ApcTheme.Current.Colors;
            if (status == StatusKind.Warn || status == StatusKind.Alarm) return ApcTheme.Status(status);
            return ApcTheme.C(status == StatusKind.Stop ? c.TextMuted : c.Text);
        }

        /// <summary>흰 바탕 + 테두리. 알람이면 굵은 알람색 테두리(멀리서도 어느 카드인지 보이게).</summary>
        internal static void DrawFrame(Graphics g, Size size, StatusKind status, bool highlight)
        {
            g.Clear(Color.White);
            var c = ApcTheme.Current.Colors;
            using (var pen = new Pen(ApcTheme.C(c.Border))) g.DrawRectangle(pen, 0, 0, size.Width - 1, size.Height - 1);
            if (status == StatusKind.Alarm)
            {
                int w = (int)System.Math.Round(3 * ApcTheme.Scale);
                using (var pen = new Pen(ApcTheme.Status(StatusKind.Alarm), w))
                    g.DrawRectangle(pen, w / 2, w / 2, size.Width - w, size.Height - w);
            }
            else if (highlight)
            {
                using (var pen = new Pen(FieldFrame.FocusColor))
                {
                    g.DrawRectangle(pen, 0, 0, size.Width - 1, size.Height - 1);
                    g.DrawRectangle(pen, 1, 1, size.Width - 3, size.Height - 3);
                }
            }
        }

        internal static Image Lamp(StatusKind status)
        {
            return ApcTheme.GetImage("lamp_" + status.ToString().ToLowerInvariant());
        }
    }
}
