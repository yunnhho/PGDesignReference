using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;

namespace ModuleLeakTester
{
    internal sealed class MainForm : ApcForm
    {
        public MainForm()
        {
            float s = ApcTheme.Scale;
            TitleText = "모듈 기밀 검사";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size((int)(1600 * s), (int)(900 * s));
            MinimumSize = new Size((int)(1024 * s), (int)(640 * s));
            Controls.Add(new Views.MainView());
        }
    }
}
