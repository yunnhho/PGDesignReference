using System;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Drawing;
using APC.UI.Theming;

namespace APC.UI.Controls
{
    /// <summary>테마 스타일을 입힌 DataGridView. 헤더 그라데이션, 행 배경 한 색(번갈아 칠하지 않음), 행 전체 선택.</summary>
    public class ApcGrid : DataGridView
    {
        public ApcGrid()
        {
            DoubleBuffered = true;
            EnableHeadersVisualStyles = false;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            RowHeadersVisible = false;
            AllowUserToAddRows = false;
            AllowUserToResizeRows = false;
            BorderStyle = BorderStyle.FixedSingle;
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ApcTheme.Changed += OnThemeChanged;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            RowTemplate.Height = ApcTheme.MinHeight;
            ColumnHeadersHeight = (int)Math.Round(ApcTheme.MinHeight * 1.15);
            if (ApcTheme.Current == null) return;

            var c = ApcTheme.Current.Colors;
            var text = ApcTheme.C(c.Text);
            Font = ApcTheme.GetFont();
            BackgroundColor = Color.White;
            GridColor = ApcTheme.C(c.GridLine);
            DefaultCellStyle.ForeColor = text;
            DefaultCellStyle.SelectionBackColor = ApcTheme.C(c.GridSelected);
            DefaultCellStyle.SelectionForeColor = text;
            ColumnHeadersDefaultCellStyle.Font = ApcTheme.GetFont(FontStyle.Bold);
            ColumnHeadersDefaultCellStyle.ForeColor = text;
            ColumnHeadersDefaultCellStyle.SelectionBackColor = ApcTheme.C(c.GridHeader[1]);
            foreach (DataGridViewRow row in Rows) row.Height = RowTemplate.Height;
        }

        /// <summary>기본 폭 100px은 한글 폰트에서 코드값(WO-24117-01 등)이 잘린다.</summary>
        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            if (!(e.Column is ApcLampColumn) && e.Column.Width == 100)
                e.Column.Width = (int)Math.Round(130 * ApcTheme.Scale);
            base.OnColumnAdded(e);
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            base.OnCellPainting(e);
            if (e.Handled || e.RowIndex != -1 || ApcTheme.Current == null) return;

            var c = ApcTheme.Current.Colors;
            Gradients.Fill(e.Graphics, e.CellBounds, c.GridHeader);
            using (var pen = new Pen(ApcTheme.C(c.Border)))
            {
                e.Graphics.DrawLine(pen, e.CellBounds.Right - 1, e.CellBounds.Top + 4, e.CellBounds.Right - 1, e.CellBounds.Bottom - 5);
                e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
            }
            e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);
            e.Handled = true;
        }

        private void OnThemeChanged(object sender, EventArgs e) { ApplyTheme(); Invalidate(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing) ApcTheme.Changed -= OnThemeChanged;
            base.Dispose(disposing);
        }
    }
}
