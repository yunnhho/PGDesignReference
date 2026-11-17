using System.IO;
using APC.UI.Tools;

namespace APC.UI.Checks
{
    internal static partial class Program
    {
        private static void RunToolChecks()
        {
            string dll = Path.Combine(Root, @"src\APC.UI\bin\Debug\APC.UI.dll");
            var md = ApiWriter.Write(dll);
            Check(md.StartsWith("# APC.UI API (v1.1.0.0)"), "api: header carries version");
            Check(md.Contains("## ApcTemplateView") && md.Contains("## ApcButton"), "api: lists public controls");
            Check(md.Contains("Load(string themeDir)"), "api: lists ApcTheme.Load signature");
            Check(md.Contains("모든 기능 화면의 공통 틀"), "api: includes xml summary text");
            Check(md == ApiWriter.Write(dll), "api: deterministic");
        }
    }
}
