using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Theming;

namespace APC.UI.Controls
{
    /// <summary>
    /// 숫자 입력: 칸 안 오른쪽에 ▲(위)/▼(아래) 버튼 영역(콤보 화살표와 같은 모양). 버튼·↑↓ 키·마우스 휠은 Increment만큼, 직접 입력은 Enter 또는 칸을 떠날 때 확정.
    /// 범위 밖은 Minimum/Maximum으로 잘리고, 숫자가 아니면 이전 값으로 되돌린다. 소수 자릿수는 DecimalPlaces.
    /// </summary>
    public class ApcNumericBox : ApcButtonField
    {
        private readonly ApcTextBox _text;
        private decimal _value;
        private decimal _minimum;
        private decimal _maximum = 100;
        private decimal _increment = 1;
        private int _decimalPlaces;

        public event EventHandler ValueChanged;

        public ApcNumericBox() : base(new ApcTextBox { TextAlign = HorizontalAlignment.Right })
        {
            _text = TextBox;
            _text.KeyDown += OnTextKeyDown;
            _text.KeyPress += OnTextKeyPress;
            _text.Leave += (s, e) => Commit();
            _text.MouseWheel += (s, e) => Step(Math.Sign(e.Delta));
            ShowValue();
        }

        [DefaultValue(typeof(decimal), "0")]
        public decimal Value
        {
            get { return _value; }
            set
            {
                var v = Clamp(Math.Round(value, _decimalPlaces, MidpointRounding.AwayFromZero), _minimum, _maximum);
                bool changed = v != _value;
                _value = v;
                ShowValue();
                if (changed) OnValueChanged(EventArgs.Empty);
            }
        }

        [DefaultValue(typeof(decimal), "0")]
        public decimal Minimum
        {
            get { return _minimum; }
            set { _minimum = value; if (_maximum < value) _maximum = value; Value = _value; }
        }

        [DefaultValue(typeof(decimal), "100")]
        public decimal Maximum
        {
            get { return _maximum; }
            set { _maximum = value; if (_minimum > value) _minimum = value; Value = _value; }
        }

        [DefaultValue(typeof(decimal), "1")]
        public decimal Increment
        {
            get { return _increment; }
            set { _increment = value; }
        }

        [DefaultValue(0)]
        public int DecimalPlaces
        {
            get { return _decimalPlaces; }
            set { _decimalPlaces = Math.Max(0, Math.Min(10, value)); Value = _value; }
        }

        /// <summary>입력 글자를 값으로. 숫자가 아니면 false. 범위·자릿수는 맞춰서 돌려준다.</summary>
        internal static bool TryParse(string text, decimal min, decimal max, int decimals, out decimal value)
        {
            value = 0;
            decimal parsed;
            if (!decimal.TryParse((text ?? "").Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out parsed)) return false;
            value = Clamp(Math.Round(parsed, decimals, MidpointRounding.AwayFromZero), min, max);
            return true;
        }

        private static decimal Clamp(decimal v, decimal min, decimal max)
        {
            return v < min ? min : v > max ? max : v;
        }

        /// <summary>입력칸 글자를 확정한다. 숫자가 아니면 이전 값을 다시 보여 준다.</summary>
        public void Commit()
        {
            decimal v;
            if (TryParse(_text.Text, _minimum, _maximum, _decimalPlaces, out v)) Value = v;
            ShowValue();
        }

        private void Step(int direction)
        {
            if (direction == 0 || !Enabled) return;
            Commit();
            Value = _value + direction * _increment;
            _text.SelectAll();
        }

        private void ShowValue()
        {
            _text.Text = _value.ToString("F" + _decimalPlaces, CultureInfo.CurrentCulture);
        }

        private void OnTextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up) { Step(1); e.Handled = true; }
            else if (e.KeyCode == Keys.Down) { Step(-1); e.Handled = true; }
            else if (e.KeyCode == Keys.Enter) Commit();
        }

        private void OnTextKeyPress(object sender, KeyPressEventArgs e)
        {
            var nf = CultureInfo.CurrentCulture.NumberFormat;
            char c = e.KeyChar;
            bool ok = char.IsControl(c) || char.IsDigit(c)
                || (_decimalPlaces > 0 && nf.NumberDecimalSeparator.IndexOf(c) >= 0)
                || (_minimum < 0 && nf.NegativeSign.IndexOf(c) >= 0);
            e.Handled = !ok;
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            var h = ValueChanged;
            if (h != null) h(this, e);
        }

        internal override int SegmentCount { get { return 2; } }
        internal override bool SegmentPointsUp(int index) { return index == 0; }
        internal override void OnSegmentClick(int index) { Step(index == 0 ? 1 : -1); }
    }
}
