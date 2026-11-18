using System.IO;
using APC.UI.Theming;

namespace APC.UI.Checks
{
    internal static partial class Program
    {
        private static void RunDialogLayoutChecks()
        {
            ApcTheme.Load(Path.Combine(Root, Path.Combine("themes", "default")));
            Check(ApcLayout.Pad == 16 && ApcLayout.RowGap == 12 && ApcLayout.GroupGap == 16 && ApcLayout.LabelGap == 12
                  && ApcLayout.ItemGap == 8 && ApcLayout.FooterButtonWidth == 110 && ApcLayout.FooterButtonHeight == 40
                  && ApcLayout.FieldHeight == 34, "layout: fixed values");
        }
    }
}
