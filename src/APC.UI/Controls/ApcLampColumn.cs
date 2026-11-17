using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using APC.UI.Theming;

namespace APC.UI.Controls
{
    /// <summary>값이 StatusKind인 셀을 상태 램프 이미지로 그리는 컬럼. 그 외 값(null 등)은 빈 셀.</summary>
    public class ApcLampColumn : DataGridViewColumn
    {
        public ApcLampColumn() : base(new ApcLampCell())
        {
            Width = (int)Math.Round(56 * ApcTheme.Scale);
            SortMode = DataGridViewColumnSortMode.Automatic;
            HeaderText = "상태";
            DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
    }

    public class ApcLampCell : DataGridViewTextBoxCell
    {
        private const int BaseLamp = 18;

        public override Type ValueType { get { return typeof(object); } }

        internal string ImageNameFor(object value)
        {
            return value is StatusKind ? "lamp_" + value.ToString().ToLowerInvariant() : null;
        }

        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle,
            TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            return string.Empty;   // 글자는 그리지 않는다
        }

        protected override void Paint(Graphics g, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
            DataGridViewElementStates cellState, object value, object formattedValue, string errorText,
            DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            base.Paint(g, clipBounds, cellBounds, rowIndex, cellState, value, string.Empty, errorText, cellStyle, advancedBorderStyle, paintParts);

            var name = ImageNameFor(value);
            if (name == null || ApcTheme.Current == null || (paintParts & DataGridViewPaintParts.ContentForeground) == 0) return;

            int side = Math.Min((int)(BaseLamp * ApcTheme.Scale), cellBounds.Height - 4);
            var r = new Rectangle(cellBounds.X + (cellBounds.Width - side) / 2, cellBounds.Y + (cellBounds.Height - side) / 2, side, side);
            var old = g.InterpolationMode;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(ApcTheme.GetImage(name), r);
            g.InterpolationMode = old;
        }
    }
}
