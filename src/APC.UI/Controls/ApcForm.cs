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
    /// 테두리 없는 APCSOFT 창. 타이틀바(로고·제목·최소화·닫기 이미지 버튼), 드래그 이동,
    /// 더블클릭 최대화, 가장자리 리사이즈. 최대화해도 작업표시줄을 가리지 않는다.
    /// </summary>
    public partial class ApcForm : Form
    {
        private const int BaseTitleHeight = 40;
        private const int BaseResizeGrip = 6;
        private const int BaseLogoHeight = 22;
        private const int BaseWinButtonW = 46;
        private const int BaseWinButtonH = 32;

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 1, HTCAPTION = 2;
        private const int HTLEFT = 10, HTRIGHT = 11, HTTOP = 12, HTTOPLEFT = 13, HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15, HTBOTTOMLEFT = 16, HTBOTTOMRIGHT = 17;
        private const int WS_MINIMIZEBOX = 0x20000, WS_MAXIMIZEBOX = 0x10000;

        private enum WinButton { None, Minimize, Close }

        private string _titleText = string.Empty;
        private WinButton _hover;
        private WinButton _pressed;

        public ApcForm()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FormBorderStyle = FormBorderStyle.None;
            Resizable = true;
            UpdatePadding();
            UpdateMaximizedBounds();
            ApcTheme.Changed += OnThemeChanged;
            ApplyThemeFont();
        }

        [DefaultValue("")]
        public string TitleText
        {
            get { return _titleText; }
            set { _titleText = value ?? string.Empty; Text = _titleText; Invalidate(TitleBarRect); }
        }

        private bool _resizable = true;

        [DefaultValue(true)]
        public bool Resizable
        {
            get { return _resizable; }
            set { _resizable = value; UpdatePadding(); }
        }

        public int TitleBarHeight { get { return (int)Math.Round(BaseTitleHeight * ApcTheme.Scale); } }

        /// <summary>가장자리 리사이즈 감지 폭(px).</summary>
        public int ResizeGrip { get { return (int)(BaseResizeGrip * ApcTheme.Scale); } }

        /// <summary>타이틀바·테두리(리사이즈 그립 포함)를 뺀 내용 영역(클라이언트 좌표) = Padding 안쪽.</summary>
        public Rectangle ContentBounds
        {
            get
            {
                var p = Padding;
                return Rectangle.FromLTRB(p.Left, p.Top, Math.Max(p.Left, ClientSize.Width - p.Right), Math.Max(p.Top, ClientSize.Height - p.Bottom));
            }
        }

        internal Rectangle MaximizedArea { get { return MaximizedBounds; } }

        private Rectangle TitleBarRect { get { return new Rectangle(0, 0, ClientSize.Width, TitleBarHeight); } }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX | WS_MAXIMIZEBOX;   // 작업표시줄 최소화·더블클릭 최대화 허용
                return cp;
            }
        }

        private Rectangle ButtonRect(WinButton b)
        {
            float s = ApcTheme.Scale;
            int w = (int)(BaseWinButtonW * s), h = (int)(BaseWinButtonH * s), gap = (int)(4 * s);
            int y = (TitleBarHeight - h) / 2;
            int closeX = ClientSize.Width - gap - w;
            return b == WinButton.Close ? new Rectangle(closeX, y, w, h)
                 : b == WinButton.Minimize ? new Rectangle(closeX - gap / 2 - w, y, w, h)
                 : Rectangle.Empty;
        }

        private WinButton HitButton(Point p)
        {
            if (ButtonRect(WinButton.Close).Contains(p)) return WinButton.Close;
            if (MinimizeBox && ButtonRect(WinButton.Minimize).Contains(p)) return WinButton.Minimize;
            return WinButton.None;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (ApcTheme.Current == null) { base.OnPaintBackground(e); return; }
            Gradients.Fill(e.Graphics, ClientRectangle, ApcTheme.Current.Colors.Background);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (ApcTheme.Current == null) return;
            var g = e.Graphics;
            var c = ApcTheme.Current.Colors;
            float s = ApcTheme.Scale;

            Gradients.Fill(g, TitleBarRect, c.TitleBar);
            using (var pen = new Pen(ApcTheme.C(c.Border)))
            {
                g.DrawLine(pen, 0, TitleBarHeight, ClientSize.Width, TitleBarHeight);
                g.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }

            int x = (int)(10 * s);
            var logo = ApcTheme.GetImage(ApcTheme.Current.Logo);
            int lh = (int)(BaseLogoHeight * s);
            int lw = logo.Width * lh / logo.Height;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(logo, new Rectangle(x, (TitleBarHeight - lh) / 2, lw, lh));
            x += lw + (int)(12 * s);

            var minRect = ButtonRect(WinButton.Minimize);
            TextRenderer.DrawText(g, _titleText, ApcTheme.GetFont(FontStyle.Bold),
                Rectangle.FromLTRB(x, 0, minRect.Left - (int)(8 * s), TitleBarHeight), ApcTheme.C(c.Text),
                TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

            DrawWinButton(g, WinButton.Minimize, "win_minimize", MinimizeBox);
            DrawWinButton(g, WinButton.Close, "win_close", true);
        }

        private void DrawWinButton(Graphics g, WinButton b, string name, bool enabled)
        {
            string state = !enabled ? "disabled" : _pressed == b ? "pressed" : _hover == b ? "hover" : "normal";
            g.DrawImage(ApcTheme.GetImage(name + "_" + state), ButtonRect(b));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            SetHover(HitButton(e.Location));
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            SetHover(WinButton.None);
            _pressed = WinButton.None;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _pressed = HitButton(e.Location); Invalidate(TitleBarRect); }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            var pressed = _pressed;
            _pressed = WinButton.None;
            Invalidate(TitleBarRect);
            if (pressed != WinButton.None && pressed == HitButton(e.Location))
            {
                if (pressed == WinButton.Close) Close();
                else WindowState = FormWindowState.Minimized;
            }
            base.OnMouseUp(e);
        }

        private void SetHover(WinButton b)
        {
            if (_hover == b) return;
            _hover = b;
            Invalidate(TitleBarRect);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg != WM_NCHITTEST || (int)m.Result != HTCLIENT) return;

            var p = PointToClient(new Point(unchecked((short)(long)m.LParam), unchecked((short)((long)m.LParam >> 16))));
            int hit = HitTest(p);
            if (hit != HTCLIENT) m.Result = (IntPtr)hit;
        }

        private int HitTest(Point p)
        {
            if (Resizable && WindowState == FormWindowState.Normal)
            {
                int grip = ResizeGrip;
                bool l = p.X < grip, r = p.X >= ClientSize.Width - grip;
                bool t = p.Y < grip, b = p.Y >= ClientSize.Height - grip;
                if (t && l) return HTTOPLEFT;
                if (t && r) return HTTOPRIGHT;
                if (b && l) return HTBOTTOMLEFT;
                if (b && r) return HTBOTTOMRIGHT;
                if (l) return HTLEFT;
                if (r) return HTRIGHT;
                if (t) return HTTOP;
                if (b) return HTBOTTOM;
            }
            if (p.Y < TitleBarHeight && HitButton(p) == WinButton.None) return HTCAPTION;
            return HTCLIENT;
        }

        /// <summary>폼이 뜰 때 첫 입력칸이 포커스를 받으며 글자 전체가 파랗게 선택되는 것을 막는다(커서는 끝으로).</summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // 숫자칸·날짜칸은 안쪽 입력칸이 포커스를 받는다 → 컨테이너의 ActiveControl을 따라 내려간다.
            Control active = ActiveControl;
            while (active is ContainerControl inner && inner.ActiveControl != null) active = inner.ActiveControl;
            var box = active as TextBoxBase;
            if (box != null && box.SelectionLength == box.TextLength) box.Select(box.TextLength, 0);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            UpdateMaximizedBounds();
            base.OnLocationChanged(e);
        }

        /// <summary>MaximizedBounds는 해당 모니터 기준 좌표여야 한다(보조 모니터 대응).</summary>
        private void UpdateMaximizedBounds()
        {
            var screen = IsHandleCreated ? Screen.FromHandle(Handle) : Screen.PrimaryScreen;
            var wa = screen.WorkingArea;
            MaximizedBounds = new Rectangle(wa.X - screen.Bounds.X, wa.Y - screen.Bounds.Y, wa.Width, wa.Height);
        }

        /// <summary>
        /// 리사이즈 가능하면 Padding을 그립 폭만큼 잡는다. Dock=Fill 자식이 가장자리를 덮으면
        /// WM_NCHITTEST가 자식에게 가서 리사이즈가 1px 테두리에서만 된다.
        /// </summary>
        private void UpdatePadding()
        {
            int edge = _resizable && WindowState == FormWindowState.Normal ? Math.Max(1, ResizeGrip) : 1;
            Padding = new Padding(edge, TitleBarHeight + 1, edge, edge);
        }

        protected override void OnResize(EventArgs e)
        {
            UpdatePadding();   // 최대화 시 그립 여백 제거
            base.OnResize(e);
        }

        private void ApplyThemeFont()
        {
            if (ApcTheme.Current != null) Font = ApcTheme.GetFont();
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            ApplyThemeFont();
            UpdatePadding();
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) ApcTheme.Changed -= OnThemeChanged;
            base.Dispose(disposing);
        }
    }
}
