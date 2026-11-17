using System.Drawing;
using APC.UI.Theming;

namespace APC.UI.Drawing
{
    /// <summary>입력칸 공통 모양: 흰 바탕 + 테두리 + 안쪽 위 그림자(버튼은 볼록, 입력칸은 오목). 포커스면 파란 2px 테두리.</summary>
    internal static class FieldFrame
    {
        internal static readonly Color FocusColor = Color.FromArgb(0x5A, 0x7F, 0xB0);
        private static readonly Color InsetShadow = Color.FromArgb(48, 0, 0, 0);

        internal static Color Fill(bool enabled, bool readOnly)
        {
            if (ApcTheme.Current == null) return SystemColors.Window;
            var c = ApcTheme.Current.Colors;
            if (!enabled) return ApcTheme.C(c.Background[0]);
            return readOnly ? ApcTheme.C(c.GridLine) : Color.White;
        }

        internal static Color TextColor(bool enabled)
        {
            if (ApcTheme.Current == null) return enabled ? SystemColors.WindowText : SystemColors.GrayText;
            var c = ApcTheme.Current.Colors;
            return ApcTheme.C(enabled ? c.Text : c.TextMuted);
        }

        internal static void DrawBorder(Graphics g, Rectangle r, bool focused)
        {
            if (r.Width < 3 || r.Height < 3) return;
            var border = ApcTheme.Current == null ? SystemColors.ControlDark : ApcTheme.C(ApcTheme.Current.Colors.Border);
            if (focused)
            {
                using (var pen = new Pen(FocusColor))
                {
                    g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
                    g.DrawRectangle(pen, r.X + 1, r.Y + 1, r.Width - 3, r.Height - 3);
                }
                return;
            }
            using (var pen = new Pen(border)) g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
            using (var pen = new Pen(InsetShadow)) g.DrawLine(pen, r.X + 1, r.Y + 1, r.Right - 2, r.Y + 1);
        }
    }
}
