using System;
using System.Drawing;
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

            string demo = Path.Combine(Root, @"src\APC.UI.Demo\bin\Debug\APC.UI.Demo.exe");
            string theme = Path.Combine(Root, @"themes\default");
            using (var bmp = Renderer.Render(demo, "ProductionView", theme, new Size(1920, 1080)))
            {
                Check(bmp.Width == 1920 && bmp.Height == 1080, "render: requested size");
                Check(bmp.GetPixel(10, 10) != bmp.GetPixel(960, 1070), "render: not a blank image");
            }
            Check(Throws<RenderException>(() => Renderer.Render(demo, "ProductonView", theme, new Size(800, 600)), "ProductionView"), "render: typo lists candidates");
            Check(Throws<RenderException>(() => Renderer.Render(demo, "ProductionView", Path.Combine(Root, "nope"), new Size(800, 600)), "nope"), "render: bad theme path reported");
            using (var a = new Bitmap(100, 50))
            using (var b = new Bitmap(200, 100))
            using (var s = Renderer.SideBySide(a, b))
                Check(s.Width == 400 && s.Height == 100, "render: side-by-side scales to taller height");

            string old = File.ReadAllText(Fx(@"csproj\old-winforms.csproj"));
            string once = CsprojPatcher.Patch(old);
            Check(once.Contains(@"<HintPath>lib\APC.UI.dll</HintPath>") && once.Contains(@"themes\**\*"), "attach: adds reference and theme copy");
            Check(CsprojPatcher.Patch(once) == once, "attach: idempotent");
            Check(Throws<InvalidOperationException>(() => CsprojPatcher.Patch(File.ReadAllText(Fx(@"csproj\sdk.csproj"))), "SDK"), "attach: rejects SDK-style");
            Check(Throws<InvalidOperationException>(() => CsprojPatcher.Patch(File.ReadAllText(Fx(@"csproj\net48-console.csproj"))), "WinForms"), "attach: rejects non-WinForms");
            string hinted = CsprojPatcher.Patch(File.ReadAllText(Fx(@"csproj\old-with-hintpath.csproj")));
            Check(hinted.Split(new[] { "Include=\"APC.UI" }, StringSplitOptions.None).Length == 2 && hinted.Contains(@"lib\APC.UI.dll") && !hinted.Contains("shared"), "attach: rewrites existing APC.UI HintPath without duplicating");
            string v35 = old.Replace("v4.8", "v3.5");
            Check(Throws<InvalidOperationException>(() => CsprojPatcher.Patch(v35), "v3.5"), "attach: rejects non-4.x framework with actual value");
        }
    }
}
