using System;
using System.IO;
using System.Windows.Forms;
using APC.UI.Theming;

namespace APC.UI.Demo
{
    internal static class Program
    {
        /// <summary>사용: APC.UI.Demo.exe [--theme &lt;name&gt;]  (기본 default, 폴더 themes/&lt;name&gt;)</summary>
        [STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string name = "default";
            int i = Array.IndexOf(args, "--theme");
            if (i >= 0 && i + 1 < args.Length) name = args[i + 1];

            string themeDir = Path.Combine(FindThemesRoot(), name);
            try
            {
                ApcTheme.Load(themeDir);
            }
            catch (ThemeException ex)
            {
                MessageBox.Show(ex.Message, "테마 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }

            Application.Run(new GalleryForm());
            return 0;
        }

        /// <summary>exe 옆 themes/ → 없으면 저장소 루트(APC.UI.sln 위치)의 themes/</summary>
        private static string FindThemesRoot()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            for (var d = dir; d != null; d = d.Parent)
                if (Directory.Exists(Path.Combine(d.FullName, "themes"))) return Path.Combine(d.FullName, "themes");
            return Path.Combine(dir.FullName, "themes");
        }
    }
}
