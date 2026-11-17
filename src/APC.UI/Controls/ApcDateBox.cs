using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Theming;

namespace APC.UI.Controls
{
    /// <summary>
    /// 날짜 입력: 입력칸(yyyy-MM-dd) + 달력 버튼. 버튼을 누르면 달력이 펼쳐지고, 날짜를 누르면 닫힌다.
    /// 직접 입력은 yyyy-MM-dd / yyyyMMdd를 받아 Enter 또는 칸을 떠날 때 확정, 틀리면 이전 값. ↑↓ 키는 하루씩.
    /// </summary>
    public class ApcDateBox : UserControl
    {
        public const string Format = "yyyy-MM-dd";
        /// <summary>받는 범위. 달력 이동·하루 이동이 DateTime 끝에서 예외를 내지 않게.</summary>
        public static readonly DateTime MinDate = new DateTime(1900, 1, 1), MaxDate = new DateTime(2999, 12, 31);
        private static readonly string[] InputFormats = { "yyyy-MM-dd", "yyyyMMdd", "yyyy-M-d", "yyyy/MM/dd", "yyyy/M/d", "yyyy.MM.dd" };

        private readonly ApcTextBox _text = new ApcTextBox { TextAlign = HorizontalAlignment.Center };
        private readonly ApcButton _open = new ApcButton { Text = "▼", TabStop = false };
        private DateTime _value = DateTime.Today;
        private ToolStripDropDown _popup;
        private DateTime _popupClosedAt;

        public event EventHandler ValueChanged;

        public ApcDateBox()
        {
            BackColor = Color.Transparent;
            _open.Click += (s, e) => ShowCalendar();
            _text.Leave += (s, e) => Commit();
            _text.KeyDown += OnTextKeyDown;
            Controls.Add(_text);
            Controls.Add(_open);
            Size = new Size(S(160), S(ApcTextBox.BaseHeight));
            ShowValue();
        }

        public DateTime Value
        {
            get { return _value; }
            set
            {
                var v = value.Date < MinDate ? MinDate : value.Date > MaxDate ? MaxDate : value.Date;
                bool changed = v != _value;
                _value = v;
                ShowValue();
                if (changed) OnValueChanged(EventArgs.Empty);
            }
        }

        [Browsable(false)]
        public ApcTextBox TextBox { get { return _text; } }

        internal static bool TryParse(string text, out DateTime value)
        {
            return DateTime.TryParseExact((text ?? "").Trim(), InputFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out value)
                && value >= MinDate && value <= MaxDate;
        }

        /// <summary>입력칸 글자를 확정한다. 날짜가 아니면 이전 값을 다시 보여 준다.</summary>
        public void Commit()
        {
            DateTime d;
            if (TryParse(_text.Text, out d)) Value = d;
            ShowValue();
        }

        private void ShowValue() { _text.Text = _value.ToString(Format, CultureInfo.InvariantCulture); }

        private void OnTextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F4 || (e.Alt && e.KeyCode == Keys.Down)) { ShowCalendar(); e.Handled = true; }
            else if (e.KeyCode == Keys.Up) { Commit(); if (_value < MaxDate) Value = _value.AddDays(1); e.Handled = true; }
            else if (e.KeyCode == Keys.Down) { Commit(); if (_value > MinDate) Value = _value.AddDays(-1); e.Handled = true; }
            else if (e.KeyCode == Keys.Enter) Commit();
        }

        private void ShowCalendar()
        {
            // 열린 달력은 ▼ 버튼의 마우스다운에서 먼저 닫힌다. 바로 이어진 Click은 다시 열지 않는다(토글).
            if ((DateTime.Now - _popupClosedAt).TotalMilliseconds < 250) return;
            Commit();
            if (_popup != null) _popup.Dispose();
            var cal = new ApcCalendar(_value);
            var host = new ToolStripControlHost(cal) { Margin = Padding.Empty, Padding = Padding.Empty, AutoSize = false, Size = cal.Size };
            _popup = new ToolStripDropDown { Padding = Padding.Empty, DropShadowEnabled = true };
            _popup.Items.Add(host);
            _popup.Closed += (s, e) => _popupClosedAt = DateTime.Now;
            cal.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) _popup.Close(); };
            cal.DateSelected += (s, e) => { Value = cal.Value; _popup.Close(); _text.Focus(); };
            _popup.Show(this, new Point(0, Height));
            cal.Focus();   // Esc로 닫을 수 있게
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            var h = ValueChanged;
            if (h != null) h(this, e);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            int h = Height, gap = S(4);
            _open.SetBounds(Width - h, 0, h, h);
            _text.SetBounds(0, 0, Math.Max(S(20), Width - h - gap), h);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, Math.Max(height, ApcTheme.MinHeight), specified);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _popup != null) _popup.Dispose();
            base.Dispose(disposing);
        }

        private static int S(int px) { return (int)Math.Round(px * ApcTheme.Scale); }
    }
}
