using System;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;

namespace APC.UI.Checks
{
    internal static partial class Program
    {
        private static void RunGridChecks()
        {
            using (var grid = new ApcGrid())
            {
                Check(grid.RowTemplate.Height >= ApcTheme.MinHeight, "grid row height >= min");
                Check(grid.SelectionMode == DataGridViewSelectionMode.FullRowSelect && !grid.AllowUserToAddRows, "grid defaults");
                Check(grid.AlternatingRowsDefaultCellStyle.BackColor.IsEmpty, "grid rows use one background color (no alternating)");

                var col = new ApcLampColumn { Name = "status" };
                grid.Columns.Add(col);
                grid.Rows.Add();
                grid.Rows[0].Cells[0].Value = null;
                bool threw = false;
                try { RenderOffscreen(grid); }
                catch (Exception) { threw = true; }
                Check(!threw, "lamp column renders null cell without exception");

                grid.Rows[0].Cells[0].Value = StatusKind.Alarm;
                Check(((ApcLampCell)grid.Rows[0].Cells[0]).ImageNameFor(StatusKind.Alarm) == "lamp_alarm", "lamp cell maps status to image");
                Check(((ApcLampCell)grid.Rows[0].Cells[0]).ImageNameFor("garbage") == null, "lamp cell ignores non-status values");
            }
        }

        private static void RenderOffscreen(Control c)
        {
            c.Size = new Size(300, 120);
            using (var bmp = new Bitmap(300, 120)) c.DrawToBitmap(bmp, new Rectangle(0, 0, 300, 120));
        }
    }
}
