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
            // 화면에 뜰 때 첫 행이 저절로 선택(파란 줄)되지 않는다. 앱이 고른 선택은 남는다.
            using (var form = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) })
            {
                var g1 = new ApcGrid { Dock = DockStyle.Top, Height = 100 };
                var g2 = new ApcGrid { Dock = DockStyle.Top, Height = 100 };
                foreach (var g in new[] { g1, g2 })
                {
                    g.Columns.Add("a", "A");
                    g.Rows.Add("1"); g.Rows.Add("2");
                    form.Controls.Add(g);
                }
                form.Show();
                Application.DoEvents();
                Check(g1.SelectedRows.Count == 0, "grid: no automatic first-row selection when shown (was " + g1.SelectedRows.Count + ")");
                g1.Rows.Add("3");
                Application.DoEvents();
                Check(g1.SelectedRows.Count == 0, "grid: adding rows does not select one");
                g2.Rows[1].Selected = true;
                Application.DoEvents();
                Check(g2.SelectedRows.Count == 1 && g2.Rows[1].Selected, "grid: explicit selection by app is kept");
                form.Hide();
            }

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
