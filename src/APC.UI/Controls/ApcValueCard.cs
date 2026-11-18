using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using APC.UI.Drawing;
using APC.UI.Theming;

namespace APC.UI.Controls
{
    /// <summary>
    /// 현재값 카드: 제목 띠(그리드 헤더 그라데이션, ShowLamp면 오른쪽에 램프) + 큰 값·단위 + 아래 한 줄(기준·범위·수신 시각 등).
    /// 값 색은 Status로: 경고/알람만 상태색, 정지는 흐리게. 알람이면 굵은 알람색 테두리. Value가 비면 값·단위 모두 빈칸.
    /// </summary>
    public class ApcValueCard : Control
    {
        private string _title = "";
        private string _value = "";
        private string _unit = "";
        private string _footer = "";
        private StatusKind _status = StatusKind.Run;
        private bool _showLamp;
        private HorizontalAlignment _valueAlign = HorizontalAlignment.Left;

        public ApcValueCard()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(S(260), S(150));
            Margin = new Padding(0, 0, S(12), S(12));
            ApcTheme.Changed += OnThemeChanged;
        }

        [DefaultValue("")] public string Title { get { return _title; } set { _title = value ?? ""; Invalidate(); } }
        [DefaultValue("")] public string Value { get { return _value; } set { _value = value ?? ""; Invalidate(); } }
        [DefaultValue("")] public string Unit { get { return _unit; } set { _unit = value ?? ""; Invalidate(); } }

        /// <summary>아래 한 줄. 비우면 줄이 없다.</summary>
        [DefaultValue("")] public string Footer { get { return _footer; } set { _footer = value ?? ""; Invalidate(); } }

        [DefaultValue(StatusKind.Run)] public StatusKind Status { get { return _status; } set { _status = value; Invalidate(); } }
        [DefaultValue(false)] public bool ShowLamp { get { return _showLamp; } set { _showLamp = value; Invalidate(); } }
        [DefaultValue(HorizontalAlignment.Left)] public HorizontalAlignment ValueAlign { get { return _valueAlign; } set { _valueAlign = value; Invalidate(); } }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            if (ApcTheme.Current == null) { g.Clear(SystemColors.Control); return; }
            var c = ApcTheme.Current.Colors;
            int pad = S(14), titleH = S(36);
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;

            Cards.DrawFrame(g, Size, _status, false);
            int inset = _status == StatusKind.Alarm ? S(3) : 1;
            Gradients.Fill(g, new Rectangle(inset, inset, Width - 2 * inset, titleH - inset), c.GridHeader);
            using (var pen = new Pen(ApcTheme.C(c.Border))) g.DrawLine(pen, inset, titleH, Width - inset, titleH);

            int lamp = _showLamp ? S(20) : 0;
            if (_showLamp)
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(Cards.Lamp(_status), new Rectangle(Width - pad - lamp, (titleH - lamp) / 2, lamp, lamp));
            }
            TextClip.Draw(g, this, _title, ApcTheme.GetFont(FontStyle.Bold), new Rectangle(pad, 0, Width - pad * 2 - lamp, titleH), ApcTheme.C(c.Text), flags);

            int footerH = _footer.Length > 0 ? S(32) : 0;
            var valueArea = Rectangle.FromLTRB(pad, titleH, Width - pad, Height - footerH);
            // 값 글자 크기는 Footer 유무와 관계없이 같은 식(같은 높이 카드 = 같은 글자). Footer 자리는 늘 뺀다.
            DrawValue(g, valueArea, Height - titleH - S(32));

            if (footerH > 0)
            {
                int y = Height - footerH;
                using (var pen = new Pen(ApcTheme.C(c.GridLine))) g.DrawLine(pen, pad, y, Width - pad, y);
                var footColor = _status == StatusKind.Warn || _status == StatusKind.Alarm ? ApcTheme.Status(_status) : ApcTheme.C(c.TextMuted);
                TextClip.Draw(g, this, _footer, ApcTheme.GetFont(), new Rectangle(pad, y, Width - pad * 2, footerH), footColor,
                    flags | AlignFlag(_valueAlign));
            }
        }

        private void DrawValue(Graphics g, Rectangle area, int sizeBase)
        {
            if (_value.Trim().Length == 0) return;   // 값 없음(대기·통신 전)은 빈칸
            string text = _value;
            var muted = ApcTheme.C(ApcTheme.Current.Colors.TextMuted);
            // 값 글자는 영역 높이에 맞춘다(카드를 키우면 값도 커진다).
            float px = Math.Max(12f, Math.Min(sizeBase * 0.55f, S(64)));
            using (var big = new Font(ApcTheme.Current.FontFamily, px, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var unitFont = new Font(ApcTheme.Current.FontFamily, Math.Max(10f, px * 0.42f), FontStyle.Regular, GraphicsUnit.Pixel))
            {
                var noPad = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
                var vs = TextRenderer.MeasureText(g, text, big, Size.Empty, noPad);
                var us = _unit.Length > 0 ? TextRenderer.MeasureText(g, _unit, unitFont, Size.Empty, noPad) : Size.Empty;
                int gap = us.Width > 0 ? S(6) : 0;
                int total = vs.Width + gap + us.Width;
                int x = _valueAlign == HorizontalAlignment.Center ? area.Left + (area.Width - total) / 2
                      : _valueAlign == HorizontalAlignment.Right ? area.Right - total : area.Left;
                int y = area.Top + (area.Height - vs.Height) / 2;
                TextRenderer.DrawText(g, text, big, new Point(x, y), Cards.EmphasisColor(_status), noPad);
                if (us.Width > 0)   // 단위는 값의 아래쪽 기준선에 맞춘다
                    TextRenderer.DrawText(g, _unit, unitFont, new Point(x + vs.Width + gap, y + vs.Height - us.Height - S(2)), muted, noPad);
            }
        }

        private static TextFormatFlags AlignFlag(HorizontalAlignment a)
        {
            return a == HorizontalAlignment.Center ? TextFormatFlags.HorizontalCenter
                 : a == HorizontalAlignment.Right ? TextFormatFlags.Right : TextFormatFlags.Left;
        }

        private void OnThemeChanged(object sender, EventArgs e) { Invalidate(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing) ApcTheme.Changed -= OnThemeChanged;
            base.Dispose(disposing);
        }

        private static int S(int px) { return (int)Math.Round(px * ApcTheme.Scale); }
    }
}
