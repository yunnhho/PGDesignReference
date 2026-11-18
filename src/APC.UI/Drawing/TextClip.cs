using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace APC.UI.Drawing
{
    /// <summary>
    /// 말줄임(…)으로 그리는 글자. render 점검(APC.UI.Tools LayoutLint)이 Log를 리플렉션으로 켜면 잘린 글자를 기록한다.
    /// 평소에는 Log가 null이라 그리기만 한다.
    /// </summary>
    internal static class TextClip
    {
        internal static List<Tuple<Control, string>> Log;

        internal static void Draw(Graphics g, Control owner, string text, Font font, Rectangle rect, Color color, TextFormatFlags flags)
        {
            TextRenderer.DrawText(g, text, font, rect, color, flags);
            if (Log == null || string.IsNullOrEmpty(text) || rect.Width <= 0) return;
            int need = TextRenderer.MeasureText(g, text, font, Size.Empty, flags & ~TextFormatFlags.EndEllipsis).Width;
            if (need > rect.Width) Log.Add(Tuple.Create(owner, "'" + text + "' 말줄임(…)으로 잘림(필요 " + need + "px > 폭 " + rect.Width + "px)"));
        }
    }
}
