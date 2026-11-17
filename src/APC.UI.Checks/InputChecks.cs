using System;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;

namespace APC.UI.Checks
{
    internal static partial class Program
    {
        private static void RunInputChecks()
        {
            int fieldH = (int)Math.Round(ApcTextBox.BaseHeight * ApcTheme.Scale);

            using (var box = new ApcTextBox { Width = 200 })
            {
                var h = box.Handle;   // 비클라이언트 계산은 핸들이 있어야 일어난다
                Check(box.Height == fieldH, "textbox: default height 34*dpi (was " + box.Height + ")");
                Check(box.ClientSize.Height == box.Font.Height, "textbox: client area is one text line (vertically centered)");
                Check(box.ClientSize.Width < box.Width, "textbox: side padding reserved");
                var ins = box.FrameInsets(box.Height);
                Check(Math.Abs(ins.Top - ins.Bottom) <= 1, "textbox: text line centered (top " + ins.Top + " / bottom " + ins.Bottom + ")");
                box.Height = 10;
                Check(box.Height >= ApcTheme.MinHeight, "textbox: clamps to min height");
                box.Height = fieldH;
                using (var bmp = new Bitmap(box.Width, box.Height))
                {
                    box.DrawToBitmap(bmp, new Rectangle(0, 0, box.Width, box.Height));
                    Check(bmp.GetPixel(0, box.Height / 2).ToArgb() == ApcTheme.C(ApcTheme.Current.Colors.Border).ToArgb(),
                        "textbox: render draws themed border (WM_PRINT path)");
                }
                box.ReadOnly = true;
                Check(box.BackColor.ToArgb() == ApcTheme.C(ApcTheme.Current.Colors.GridLine).ToArgb(), "textbox: read-only fill");
            }
            Check(typeof(ApcTextBox).IsAssignableFrom(typeof(ApcScanBox)), "scanbox shares the field look (derives ApcTextBox)");

            using (var cb = new ApcComboBox())
            {
                cb.Items.AddRange(new object[] { "A", "B" });
                var h = cb.Handle;
                Check(cb.DropDownStyle == ComboBoxStyle.DropDownList, "combo: list only");
                Check(Math.Abs(cb.Height - fieldH) <= 2, "combo: same height as text field (was " + cb.Height + ")");
            }

            decimal v;
            Check(ApcNumericBox.TryParse("12.345", 0, 100, 2, out v) && v == 12.35m, "numeric: rounds to decimal places");
            Check(ApcNumericBox.TryParse("150", 0, 100, 0, out v) && v == 100m, "numeric: clamps to maximum");
            Check(ApcNumericBox.TryParse("-5", 0, 100, 0, out v) && v == 0m, "numeric: clamps to minimum");
            Check(!ApcNumericBox.TryParse("abc", 0, 100, 0, out v), "numeric: rejects non-number");
            using (var nb = new ApcNumericBox { Maximum = 10 })
            {
                int changed = 0;
                nb.ValueChanged += (s, e) => changed++;
                nb.Value = 20;
                Check(nb.Value == 10 && nb.TextBox.Text == "10" && changed == 1, "numeric: value clamped and shown, event once");
                nb.TextBox.Text = "x";
                nb.Commit();
                Check(nb.Value == 10 && nb.TextBox.Text == "10", "numeric: invalid text reverts");
                nb.TextBox.Text = "7";
                nb.Commit();
                Check(nb.Value == 7 && changed == 2, "numeric: typed value committed");
                nb.Minimum = 8;
                Check(nb.Value == 8, "numeric: raising minimum re-clamps value");
            }

            DateTime d;
            Check(ApcDateBox.TryParse("20261117", out d) && d == new DateTime(2026, 11, 17), "date: accepts yyyyMMdd");
            Check(!ApcDateBox.TryParse("2026-13-01", out d), "date: rejects bad month");
            Check(!ApcDateBox.TryParse("0001-01-01", out d), "date: rejects dates outside 1900~2999");
            using (var edge = new ApcDateBox { Value = DateTime.MinValue })
                Check(edge.Value == ApcDateBox.MinDate, "date: value clamped to MinDate");
            using (var cal = new ApcCalendar(ApcDateBox.MinDate))
            {
                cal.ShiftMonth(-1);
                Check(cal.Month == ApcDateBox.MinDate, "calendar: cannot move before MinDate");
            }
            using (var multi = new ApcTextBox { Multiline = true })
                Check(multi.BorderStyle == BorderStyle.FixedSingle, "textbox: multiline falls back to native border (room for scrollbars)");
            using (var db = new ApcDateBox { Value = new DateTime(2026, 11, 17, 15, 30, 0) })
            {
                Check(db.TextBox.Text == "2026-11-17" && db.Value.TimeOfDay == TimeSpan.Zero, "date: shows yyyy-MM-dd, drops time");
                db.TextBox.Text = "어제";
                db.Commit();
                Check(db.TextBox.Text == "2026-11-17", "date: invalid text reverts");
            }

            Check(ApcCalendar.FirstCell(new DateTime(2026, 12, 1)) == new DateTime(2026, 11, 29), "calendar: grid starts on Sunday before the 1st");
            Check(ApcCalendar.FirstCell(new DateTime(2026, 11, 20)) == new DateTime(2026, 11, 1), "calendar: month starting Sunday has no lead days");
            using (var cal = new ApcCalendar(new DateTime(2026, 12, 15)))
            {
                int s40 = (int)Math.Round(40 * ApcTheme.Scale), s28 = (int)Math.Round(28 * ApcTheme.Scale);
                Check(cal.DateAt(new Point(1, s40 + s28 + 1)) == new DateTime(2026, 11, 29), "calendar: first cell hit");
                Check(cal.DateAt(new Point(s40 * 6 + 1, s40 + s28 + s40 * 5 + 1)) == new DateTime(2027, 1, 9), "calendar: last cell hit");
                Check(cal.DateAt(new Point(5, 5)) == null, "calendar: header is not a date");
                cal.ShiftMonth(1);
                Check(cal.Month == new DateTime(2027, 1, 1), "calendar: next month crosses year");
            }

            using (var dlg = ApcDialog.Create("저장할까요?", "저장", ApcButtonVariant.Primary))
            {
                ApcButton ok = null;
                foreach (Control c in dlg.Controls) { var b = c as ApcButton; if (b != null && b.DialogResult == DialogResult.OK) ok = b; }
                Check(ok != null && ok.Text == "저장" && ok.Variant == ApcButtonVariant.Primary, "confirm: ok text/variant overridable");
                Check(dlg.AcceptButton == dlg.CancelButton, "confirm: Enter still cancels with custom ok");
            }
            using (var alert = ApcDialog.CreateAlert("하중 하한이 상한보다 큽니다"))
            {
                int buttons = 0;
                foreach (Control c in alert.Controls) if (c is ApcButton) buttons++;
                var only = alert.AcceptButton as ApcButton;
                Check(buttons == 1 && only != null && alert.CancelButton == only && only.DialogResult == DialogResult.OK,
                    "alert: single ok button closes on Enter and Esc");
            }
        }
    }
}
