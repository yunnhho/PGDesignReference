using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Drawing;
using APC.UI.Theming;

namespace APC.UI.Controls
{
    /// <summary>
    /// 하단 알람 띠. 들어온 순서대로 쌓고, 여러 개면 3초마다 순환 표시.
    /// '확인'은 현재 보이는 알람 하나를 지운다. 비면 숨는다.
    /// </summary>
    public class ApcAlarmBar : Panel
    {
        public const int CycleIntervalMs = 3000;
        private const int BaseHeight = 36;
        private const int BasePadding = 12;

        private readonly List<string> _messages = new List<string>();
        private readonly Timer _cycleTimer = new Timer { Interval = CycleIntervalMs };
        private readonly ApcButton _ackButton;
        private int _index;

        public event EventHandler Acknowledged;

        public ApcAlarmBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            float s = ApcTheme.Scale;
            Dock = DockStyle.Bottom;
            Height = (int)Math.Round(BaseHeight * s);
            Padding = new Padding((int)(4 * s));

            _ackButton = new ApcButton { Text = "확인", Dock = DockStyle.Right, Width = (int)(80 * s) };
            _ackButton.Click += (sender, e) => AcknowledgeCurrent();
            Controls.Add(_ackButton);

            _cycleTimer.Tick += (sender, e) => { if (_messages.Count > 1) { _index = (_index + 1) % _messages.Count; Invalidate(); } };
            ApcTheme.Changed += OnThemeChanged;
            Visible = false;
        }

        public int Count { get { return _messages.Count; } }

        public string CurrentMessage { get { return _messages.Count == 0 ? null : _messages[_index]; } }

        public void Push(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            _messages.Add(message);
            Refresh(true);
        }

        public void AcknowledgeCurrent()
        {
            if (_messages.Count == 0) return;
            _messages.RemoveAt(_index);
            if (_index >= _messages.Count) _index = 0;
            Refresh(false);
            var handler = Acknowledged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void Refresh(bool keepIndex)
        {
            if (!keepIndex && _messages.Count > 0) _index %= _messages.Count;
            Visible = _messages.Count > 0;
            _cycleTimer.Enabled = _messages.Count > 1;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var alarm = ApcTheme.Current == null ? Color.Firebrick : ApcTheme.Status(StatusKind.Alarm);
            Gradients.Fill(g, ClientRectangle, ControlPaint.Light(alarm, 0.4f), ControlPaint.Dark(alarm, 0.1f));
            if (_messages.Count == 0 || ApcTheme.Current == null) return;

            string text = "⚠  " + CurrentMessage + (_messages.Count > 1 ? "   (" + (_index + 1) + "/" + _messages.Count + ")" : "");
            int pad = (int)(BasePadding * ApcTheme.Scale);
            var rect = Rectangle.FromLTRB(pad, 0, _ackButton.Left - pad, Height);
            TextClip.Draw(g, this, text, ApcTheme.GetFont(FontStyle.Bold), rect, ApcTheme.C(ApcTheme.Current.Colors.TextInverse),
                TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }

        private void OnThemeChanged(object sender, EventArgs e) { Invalidate(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ApcTheme.Changed -= OnThemeChanged;
                _cycleTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
