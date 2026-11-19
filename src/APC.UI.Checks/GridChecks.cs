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
            using (var g = new ApcGrid())
            {
                var icon = new ApcIconColumn { IconName = "icon_reset" };
                var lamp = new ApcLampColumn();
                g.Columns.Add(icon); g.Columns.Add(lamp);
                g.Rows.Add(true, new ApcLampValue(StatusKind.Warn, "종료 임박"));
                g.Rows.Add(false, StatusKind.Run);
                g.Rows.Add(null, null);
                var ic0 = (ApcIconCell)g.Rows[0].Cells[0];
                Check(ic0.ImageNameFor(true) == "icon_reset_normal" && ic0.ImageNameFor(false) == "icon_reset_disabled" && ic0.ImageNameFor(null) == null, "grid: icon column image per enabled state");
                Check(ApcIconColumn.IsEnabled(g.Rows[0].Cells[0]) && !ApcIconColumn.IsEnabled(g.Rows[1].Cells[0]) && !ApcIconColumn.IsEnabled(g.Rows[2].Cells[0]), "grid: icon cell enabled only when value true");
                Check(((ApcIconColumn)icon.Clone()).IconName == "icon_reset", "grid: icon column clone keeps IconName");
                var lc = (ApcLampCell)g.Rows[0].Cells[1];
                Check(lc.ImageNameFor(new ApcLampValue(StatusKind.Warn, "x")) == "lamp_warn" && lc.ImageNameFor(StatusKind.Run) == "lamp_run", "grid: lamp column accepts StatusKind and ApcLampValue");
                Check(new ApcLampValue(StatusKind.Run, "b").CompareTo(new ApcLampValue(StatusKind.Alarm, "a")) < 0
                      && new ApcLampValue(StatusKind.Run, "a").CompareTo(new ApcLampValue(StatusKind.Run, "b")) < 0, "grid: lamp value sorts by status then text");
                Check(g.CellBorderStyle == DataGridViewCellBorderStyle.Single && g.GridColor != ApcTheme.C(ApcTheme.Current.Colors.GridLine), "grid: vertical lines with visible line color");
            }
            using (var g = new ApcGrid())
                Check(ApcGrid.HeightForRows(3) == g.ColumnHeadersHeight + 3 * ApcTheme.MinHeight + 2, "grid: HeightForRows = header + rows + border");
            using (var form = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) })
            {
                var g = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = false, ClearInitialSelection = false };
                g.Columns.Add("zone", "구역"); g.Columns.Add("lo", "하한");
                g.Columns[0].ReadOnly = true;
                g.Rows.Add("A1", "20.0"); g.Rows.Add("A2", "20.0");
                form.Controls.Add(g);
                form.Show();
                Application.DoEvents();
                Check(g.CurrentCell != null && g.CurrentCell.ColumnIndex == 1 && g.CurrentCell.RowIndex == 0, "grid: input table starts on first editable cell, not read-only column");
                g.BeginEdit(true);
                Application.DoEvents();
                var ec = g.EditingControl;
                Check(ec != null && ec.Font.Unit == GraphicsUnit.Pixel && ec.Font.Size == (float)Math.Ceiling(g.Font.SizeInPoints * g.CreateGraphics().DpiY / 72f),
                    "grid: edit box font rounded up to whole px like cell text (was " + (ec == null ? "none" : ec.Font.Size + " " + ec.Font.Unit) + ")");
            }
            using (var g = new FormatGrid())
            {
                g.Columns.Add("v", "값");
                g.Rows.Add("61.8");
                var alarm = ApcTheme.Status(StatusKind.Alarm);
                var style = new DataGridViewCellStyle(g.DefaultCellStyle) { ForeColor = alarm };
                Check(g.Format(style).SelectionForeColor == alarm, "grid: status-colored cell keeps its color when selected");
                Check(g.Format(new DataGridViewCellStyle(g.DefaultCellStyle)).SelectionForeColor == g.DefaultCellStyle.SelectionForeColor, "grid: plain cell keeps default selection color");
            }
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

            // 입력 표: ClearInitialSelection=false면 앱이 정한 현재 셀이 남고, 읽기 전용 열은 회색 바탕
            using (var form = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) })
            {
                var g = new ApcGrid { Dock = DockStyle.Fill, ClearInitialSelection = false };
                g.Columns.Add("slot", "칸");
                g.Columns.Add("lot", "LOT");
                g.Columns[0].ReadOnly = true;
                g.Rows.Add("B1", ""); g.Rows.Add("B2", "");
                form.Controls.Add(g);
                form.Show();
                g.CurrentCell = g.Rows[0].Cells[1];
                g.Focus();
                Application.DoEvents();
                Check(g.CurrentCell != null && g.CurrentCell.ColumnIndex == 1, "grid: ClearInitialSelection=false keeps app's current cell");
                using (var bmp = new Bitmap(g.Width, g.Height))
                {
                    g.DrawToBitmap(bmp, new Rectangle(Point.Empty, g.Size));
                    int y = g.ColumnHeadersHeight + g.Rows[0].Height + g.Rows[1].Height / 2;   // 선택되지 않은 둘째 행
                    int gray = ApcTheme.C(ApcTheme.Current.Colors.Background[0]).ToArgb();
                    Check(bmp.GetPixel(g.Columns[0].Width - 5, y).ToArgb() == gray && bmp.GetPixel(g.Columns[0].Width + g.Columns[1].Width - 5, y).ToArgb() != gray,
                        "grid: read-only column shaded in editable grid");
                }
                form.Hide();
            }

            using (var grp = new ApcGroup { Text = "BLOCK#1" })
                Check(grp.Padding.Top > grp.TitleHeight && grp.Padding.Left > 0, "group: content starts below title band");

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
    
        private sealed class FormatGrid : ApcGrid
        {
            public DataGridViewCellStyle Format(DataGridViewCellStyle style)
            {
                var e = new DataGridViewCellFormattingEventArgs(0, 0, "61.8", typeof(string), style);
                OnCellFormatting(e);
                return e.CellStyle;
            }
        }
    }
}
