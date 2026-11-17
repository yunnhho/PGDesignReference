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

            using (var v = new ProbeView())
            {
                Check(v.H.Dock == DockStyle.Top && v.A.Dock == DockStyle.Bottom && v.B.Dock == DockStyle.Fill, "template view: header top, alarm bottom, body fill");
                Check(((ApcHeaderBar)v.H).Title == "probe", "template view: title goes to header");
            }
            Check(typeof(ApcTemplateView).IsPublic, "template view is public in APC.UI");
        }

        private sealed class ProbeView : ApcTemplateView
        {
            public ProbeView() : base("probe") { }
            public Control H { get { return Header; } }
            public Control A { get { return Alarm; } }
            public Control B { get { return Body; } }
        }
    }
}
