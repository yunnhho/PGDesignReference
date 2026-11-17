using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Drawing;
using APC.UI.Theming;

namespace APC.UI.Checks
{
    internal static partial class Program
    {
        private static void RunButtonChecks()
        {
            var d = new ApcButton { Height = 20 };
            Check(d.Height >= ApcTheme.MinHeight, "button clamps to 28*dpi");
            var big = new ApcButton { Height = 80 };
            Check(big.Height == 80, "larger height kept");

            // 9-slice: 넓게 늘려도 모서리 픽셀(0,0)은 원본 모서리와 같아야 한다.
            var src = ApcTheme.GetImage("btn_normal_normal");
            using (var bmp = new Bitmap(300, 60))
            using (var g = Graphics.FromImage(bmp))
            {
                NineSlice.Draw(g, src, new Rectangle(0, 0, 300, 60), new Padding(8), ApcTheme.ImageScale(src));
                var corner = ((Bitmap)src).GetPixel(0, 0).A;
                Check(bmp.GetPixel(0, 0).A == corner, "nine-slice keeps corner transparency");
                Check(bmp.GetPixel(150, 30).A == 255, "nine-slice fills center");
            }
        }
    }
}
