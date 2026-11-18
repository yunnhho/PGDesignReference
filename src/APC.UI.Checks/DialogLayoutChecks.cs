using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using APC.UI.Controls;
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
            CheckFieldDialog();
            CheckFillDialog();
            Check(Throws(() => new FillDialog(0, false)), "dialog: fill without height throws");
            Check(Throws(() => new FillDialog(400, true)), "dialog: second fill throws");
        }

        private static void CheckFieldDialog()
        {
            using (var d = new FieldDialog())
            {
                ShowOffscreen(d);
                int px(int v) => ApcTheme.Px(v);
                int left0 = d.PointToClient(d.Name0.Parent.PointToScreen(d.Name0.Location)).X;
                int left1 = d.PointToClient(d.Speed.Parent.PointToScreen(d.Speed.Location)).X;
                int left2 = d.PointToClient(d.InGroup.Parent.PointToScreen(d.InGroup.Location)).X;
                Check(left0 == left1 && left1 == left2, "dialog: field left edges equal (" + left0 + "," + left1 + "," + left2 + ")");
                Check(d.Speed.Top - d.Name0.Bottom == px(ApcLayout.RowGap), "dialog: row gap is RowGap");
                var cb = d.ContentBounds;
                Check(d.Cancel.Right + px(ApcLayout.ItemGap) == d.Save.Left && d.Save.Right == cb.Right - px(ApcLayout.Pad)
                      && d.Save.Size == new Size(px(110), px(40)) && d.Cancel.Size == d.Save.Size && d.CancelButton == d.Cancel,
                      "dialog: footer order and size");
                Check(d.Save.Bottom == cb.Bottom - px(ApcLayout.Pad) && d.Save.Top - d.Group.Bottom == px(ApcLayout.GroupGap),
                      "dialog: auto height");
                Check(d.TitleText == "t" && !d.Resizable && !d.MinimizeBox && !d.ShowInTaskbar, "dialog: SetupDialog sets dialog properties");
            }
        }

        private static void CheckFillDialog()
        {
            using (var d = new FillDialog(400, false))
            {
                ShowOffscreen(d);
                Check(d.Chart.Bottom + ApcTheme.Px(ApcLayout.GroupGap) == d.CloseButton.Top && d.Chart.Height > 0 && d.CancelButton == d.CloseButton,
                      "dialog: fill content");
            }
        }

        private static void ShowOffscreen(Form f)
        {
            f.StartPosition = FormStartPosition.Manual;
            f.Location = new Point(-32000, -32000);
            f.Show();
            Application.DoEvents();
        }

        private static bool Throws(Action a)
        {
            try
            {
                a();
                return false;
            }
            catch (InvalidOperationException) { return true; }
        }

        private sealed class FieldDialog : ApcForm
        {
            public readonly ApcTextBox Name0 = new ApcTextBox();
            public readonly ApcNumericBox Speed = new ApcNumericBox();
            public readonly ApcTextBox InGroup = new ApcTextBox();
            public readonly ApcButton Cancel = new ApcButton { Text = "취소" };
            public readonly ApcButton Save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            public readonly ApcGroup Group;

            public FieldDialog()
            {
                SetupDialog("t", 460);
                AddFieldRow("모델명", Name0);
                AddFieldRow("압입 속도 상한 (mm/s)", Speed);
                Group = AddGroup("묶음");
                AddFieldRow(Group, "비고", InGroup);
                SetFooter(Cancel, Save);
            }
        }

        private sealed class FillDialog : ApcForm
        {
            public readonly ApcTrendChart Chart = new ApcTrendChart();
            public readonly ApcButton CloseButton = new ApcButton { Text = "닫기" };

            public FillDialog(int height, bool twoFills)
            {
                SetupDialog("t", 600, height);
                AddContent(new ApcGrid { Height = ApcTheme.Px(100) });
                AddContentFill(Chart);
                if (twoFills) AddContentFill(new ApcTrendChart());
                SetFooter(null, CloseButton);
            }
        }
    }
}
