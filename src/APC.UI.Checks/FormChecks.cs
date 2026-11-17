using System.Windows.Forms;
using APC.UI.Controls;

namespace APC.UI.Checks
{
    internal static partial class Program
    {
        private static void RunFormChecks()
        {
            using (var f = new ApcForm())
            {
                Check(f.FormBorderStyle == FormBorderStyle.None, "form is borderless");
                Check(f.ContentBounds.Top >= f.TitleBarHeight, "content starts below title bar");
                var area = Screen.FromControl(f).WorkingArea;
                Check(f.MaximizedArea.Height <= area.Height && f.MaximizedArea.Width <= area.Width, "maximize stays inside working area");
            }

            using (var dlg = ApcDialog.Create("삭제할까요?"))
            {
                Check(dlg.CancelButton != null && dlg.AcceptButton == dlg.CancelButton, "confirm: Enter and Esc both cancel");
                ApcButton first = null;
                foreach (Control c in dlg.Controls)
                {
                    var b = c as ApcButton;
                    if (b != null && (first == null || b.TabIndex < first.TabIndex)) first = b;
                }
                Check(first != null && first.DialogResult == DialogResult.Cancel, "confirm: cancel is first in tab order (initial focus)");
                Check(first != null && first.Variant == ApcButtonVariant.Normal, "confirm: cancel is not the danger button");
            }
        }
    }
}
