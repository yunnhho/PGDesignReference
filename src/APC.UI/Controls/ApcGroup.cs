using System;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Drawing;
using APC.UI.Theming;

namespace APC.UI.Controls
{
    /// <summary>
    /// 제목 띠가 있는 묶음 상자(설정 dialog의 블록 등, 표준 GroupBox 대신). 제목 띠는 값 카드와 같은 그리드 헤더 그라데이션.
    /// 안에 넣는 컨트롤은 Padding(제목 띠 아래 + 여백) 안쪽에 배치된다 — Dock이나 FlowLayoutPanel(Dock=Fill)을 쓰면 된다.
    /// </summary>
    public class ApcGroup : Panel
    {
        private const int BaseTitleH = 30, BasePad = 10;

        public ApcGroup()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            ApcTheme.Changed += OnThemeChanged;
            ApplyPadding();
        }

        /// <summary>제목 띠 글자.</summary>
        public override string Text { get { return base.Text; } set { base.Text = value; Invalidate(); } }

        internal int TitleHeight { get { return ApcTheme.Px(BaseTitleH); } }

        /// <summary>안 내용 높이(배율 적용 px)를 담는 묶음 높이 = 제목 띠 + 위아래 여백 + 내용. 화면에서 Columns() 줄을 담으면 내용 = 줄 높이 합.</summary>
        public int HeightFor(int contentHeight) { return Padding.Vertical + contentHeight; }

        private void ApplyPadding()
        {
            int p = ApcTheme.Px(BasePad);
            Padding = new Padding(p, TitleHeight + p, p, p);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            if (ApcTheme.Current == null) return;
            var c = ApcTheme.Current.Colors;
            Gradients.Fill(g, new Rectangle(1, 1, Width - 2, TitleHeight - 1), c.GridHeader);
            using (var pen = new Pen(ApcTheme.C(c.Border)))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                g.DrawLine(pen, 0, TitleHeight, Width, TitleHeight);
            }
            TextClip.Draw(g, this, Text, ApcTheme.GetFont(FontStyle.Bold), new Rectangle(ApcTheme.Px(BasePad), 0, Width - 2 * ApcTheme.Px(BasePad), TitleHeight),
                ApcTheme.C(c.Text), TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }

        private void OnThemeChanged(object sender, EventArgs e) { ApplyPadding(); Invalidate(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing) ApcTheme.Changed -= OnThemeChanged;
            base.Dispose(disposing);
        }
    }
}
