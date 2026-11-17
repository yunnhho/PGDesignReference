using System.Drawing;
using APC.UI.Controls;
using APC.UI.Drawing;
using APC.UI.Theming;

namespace APC.UI.Checks
{
    internal static partial class Program
    {
        private static void RunDisplayChecks()
        {
            double lo, hi, step;
            ApcTrendChart.NiceRange(157.6, 176.4, 5, out lo, out hi, out step);
            Check(lo == 155 && hi == 180 && step == 5, "trend: nice range 155~180 step 5 (was " + lo + "~" + hi + " / " + step + ")");
            ApcTrendChart.NiceRange(5, 5, 5, out lo, out hi, out step);
            Check(lo < 5 && hi > 5 && step > 0, "trend: flat data still gets a range");
            ApcTrendChart.NiceRange(0, 0.37, 5, out lo, out hi, out step);
            Check(lo == 0 && hi >= 0.37 && step < 0.2, "trend: small values get fractional steps");
            using (var chart = new ApcTrendChart())
            {
                chart.SetValues(new[] { 1.0, double.NaN, 2.0 });
                Check(chart.ValueCount == 2, "trend: NaN (missing) values skipped");
                chart.UpperLimit = double.NaN;
                chart.Reference = double.PositiveInfinity;
                Check(chart.UpperLimit == null && chart.Reference == null, "trend: NaN/infinite limit means no line");
            }

            var c = ApcTheme.Current.Colors;
            Check(Cards.EmphasisColor(StatusKind.Run) == ApcTheme.C(c.Text), "card: normal value uses text color (not green)");
            Check(Cards.EmphasisColor(StatusKind.Alarm) == ApcTheme.Status(StatusKind.Alarm), "card: alarm value uses alarm color");
            Check(Cards.EmphasisColor(StatusKind.Stop) == ApcTheme.C(c.TextMuted), "card: stopped value is muted");
            Check(ApcJudgePanel.ForeFor(StatusKind.Warn) == ApcTheme.C(c.Text), "judge: dark text on yellow");
            Check(ApcJudgePanel.ForeFor(StatusKind.Alarm) == ApcTheme.C(c.TextInverse), "judge: white text on red");

            using (var card = new ApcValueCard { Status = StatusKind.Alarm, Title = "건조로 2", Value = "176.4", Unit = "℃" })
            using (var bmp = new Bitmap(card.Width, card.Height))
            {
                card.DrawToBitmap(bmp, new Rectangle(Point.Empty, card.Size));
                Check(bmp.GetPixel(1, card.Height / 2).ToArgb() == ApcTheme.Status(StatusKind.Alarm).ToArgb(), "card: alarm draws thick alarm border");
            }
            using (var tile = new ApcStatusTile { Status = StatusKind.Run, Text = "전처리" })
            using (var bmp = new Bitmap(tile.Width, tile.Height))
            {
                tile.DrawToBitmap(bmp, new Rectangle(Point.Empty, tile.Size));
                Check(bmp.GetPixel(0, tile.Height / 2).ToArgb() == ApcTheme.C(c.Border).ToArgb(), "tile: normal border when idle");
                Check(tile.TabStop, "tile: reachable by Tab");
            }
        }
    }
}
