using System.IO;
using APC.UI.Theming;

namespace APC.UI.Checks
{
    internal static partial class Program
    {
        private static void RunAssetChecks()
        {
            Check(AssetResolver.PickScale(96f) == "1x", "scale 100%");
            Check(AssetResolver.PickScale(120f) == "1.5x", "scale 125% rounds up");
            Check(AssetResolver.PickScale(144f) == "1.5x", "scale 150%");
            Check(AssetResolver.PickScale(168f) == "2x", "scale 175% rounds up");
            Check(AssetResolver.PickScale(240f) == "2x", "scale 250% caps at 2x");

            var dir = Path.Combine(Root, Path.Combine("themes", Path.Combine("default", "assets")));
            Check(AssetResolver.Resolve(dir, "lamp_run", 192f).EndsWith("lamp_run@2x.png"), "resolve 2x");
            Check(AssetResolver.Resolve(dir, "logo_apcsoft", 192f).EndsWith("logo_apcsoft@1x.png"), "fallback to 1x");
            Check(Throws<FileNotFoundException>(() => AssetResolver.Resolve(dir, "btn_nope_normal", 96f), "btn_nope_normal@1x.png"), "missing names expected path");

            ApcTheme.Load(Path.Combine(Root, Path.Combine("themes", "default")));
            Check(ApcTheme.GetImage("lamp_run") != null, "theme: image loads");
            Check(ReferenceEquals(ApcTheme.GetImage("lamp_run"), ApcTheme.GetImage("lamp_run")), "theme: image cached");
            Check(ApcTheme.MinHeight >= 28, "theme: min height >= 28");
        }
    }
}
