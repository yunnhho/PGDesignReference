using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Drawing;
using APC.UI.Theming;

namespace APC.UI.Controls
{
    /// <summary>
    /// 판정 표시판: 상태색 바탕에 큰 글자(OK / NG / READY 등) + 아래 한 줄 안내. 판정 화면의 가장 큰 요소로 쓴다.
    /// OK=Run, NG=Alarm, 대기=Stop, 확인 필요=Warn. 글자 크기는 높이에 맞춘다.
    /// </summary>
    public class ApcJudgePanel : Control
    {
        private StatusKind _status = StatusKind.Stop;
        private string _subText = "";

        public ApcJudgePanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(S(400), S(220));
            ApcTheme.Changed += OnThemeChanged;
        }

        [DefaultValue(StatusKind.Stop)] public StatusKind Status { get { return _status; } set { _status = value; Invalidate(); } }

        /// <summary>큰 글자 아래 한 줄(예: "시리얼을 스캔하세요", "하중 19.4 kN > 상한 18.0").</summary>
        [DefaultValue("")] public string SubText { get { return _subText; } set { _subText = value ?? ""; Invalidate(); } }

        /// <summary>큰 글자(OK / NG / READY 등).</summary>
        public override string Text { get { return base.Text; } set { base.Text = value; } }

        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); }

        /// <summary>바탕 위 글자색. 노랑(경고) 바탕에는 흰 글자가 안 읽혀 기본 글자색.</summary>
        internal static Color ForeFor(StatusKind status)
        {
            var c = ApcTheme.Current.Colors;
            return ApcTheme.C(status == StatusKind.Warn ? c.Text : c.TextInverse);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            if (ApcTheme.Current == null) { g.Clear(SystemColors.Control); return; }
            var rect = ClientRectangle;
            var baseColor = ApcTheme.Status(_status);
            Gradients.Fill(g, rect, ControlPaint.Light(baseColor, 0.25f), baseColor);   // 위가 밝은 입체감
            using (var pen = new Pen(ControlPaint.Dark(baseColor, 0.1f))) g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);

            var fore = ForeFor(_status);
            int subH = _subText.Length > 0 ? Math.Max(S(32), Height / 6) : 0;
            var main = new Rectangle(0, 0, Width, Height - subH);
            float px = Math.Max(14f, Math.Min(main.Height * 0.6f, Width / Math.Max(2f, Text.Length * 0.75f)));
            using (var big = new Font(ApcTheme.Current.FontFamily, px, FontStyle.Bold, GraphicsUnit.Pixel))
                TextRenderer.DrawText(g, Text, big, main, fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            if (subH > 0)
                TextRenderer.DrawText(g, _subText, ApcTheme.GetFont(FontStyle.Bold), new Rectangle(S(8), Height - subH, Width - S(16), subH - S(8)), fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
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
