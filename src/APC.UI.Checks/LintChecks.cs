using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using APC.UI.Tools;

namespace APC.UI.Checks
{
    internal static partial class Program
    {
        private static readonly string[] Templates =
        {
            "EquipmentView", "TempHumiView", "JudgeView", "FailureAnalysisView", "WorkStartView", "ProductionView",
            "DefectView", "HistoryView", "EquipmentDetailDialog", "RecipeDialog", "AdminPasswordDialog", "LotInputDialog", "SettingsDialog",
        };

        private static void RunLintChecks()
        {
            ApcTheme.Load(Path.Combine(Root, Path.Combine("themes", "default")));
            CheckLintCatches();

            // 템플릿은 스킬의 출발점이다 → 점검 0건이어야 첫 실행도 0건에서 시작한다.
            string demo = Path.Combine(Root, @"src\APC.UI.Demo\bin\Debug\APC.UI.Demo.exe");
            string theme = Path.Combine(Root, @"themes\default");
            foreach (var name in Templates)
            {
                var issues = new List<string>();
                using (Renderer.Render(demo, name, theme, null, issues)) { }
                foreach (var i in issues) Console.WriteLine("      " + i);
                Check(issues.Count(i => !LayoutLint.IsNote(i)) == 0, "lint: template " + name + " has no layout issues");
            }
            using (var bmp = Renderer.Render(demo, "RecipeDialog", theme, null, null))
                Check(bmp.Width < 1920, "render: fixed-size dialog without --size uses its own size");
        }

        private static void CheckLintCatches()
        {
            Check(Lint(p => p.Controls.Add(new Label { Text = "아주 긴 이름표 글자입니다", AutoSize = false, Width = 40, Height = 20 })).Any(i => i.Contains("글자 잘림")), "lint: clipped label");
            Check(Lint(p => p.Controls.Add(new ApcButton { Text = "레시피 불러오기", Width = 40 })).Any(i => i.Contains("글자 잘림")), "lint: clipped button");
            Check(Lint(p => { var c = new ApcComboBox { Width = 60 }; c.Items.Add("모델 A-1234567 긴 이름"); p.Controls.Add(c); }).Any(i => i.Contains("글자 잘림")), "lint: narrow combo");
            Check(Lint(p => p.Controls.Add(new ApcButton { Text = "A", Left = 420, Width = 60 })).Any(i => i.Contains("넘침")), "lint: overflow parent");
            Check(Lint(p => { p.Controls.Add(new ApcButton { Text = "A", Width = 60 }); p.Controls.Add(new ApcButton { Text = "B", Left = 30, Width = 60 }); }).Any(i => i.Contains("겹침")), "lint: overlap");
            Check(Lint(p => { p.Controls.Add(new ApcTextBox { Top = 3, Width = 100 }); p.Controls.Add(new ApcButton { Text = "B", Left = 120, Width = 60, Height = 34 }); }).Any(i => i.Contains("어긋남")), "lint: misaligned row");
            Check(Lint(p => p.Controls.Add(new TextBox())).Any(i => i.Contains("표준 TextBox")), "lint: standard control");
            Check(Lint(p => { var t = new ApcTextBox { Text = "6205-2RS", Width = 150 }; p.Controls.Add(t); t.HideSelection = false; t.SelectAll(); }).Any(i => i.Contains("선택된 채")), "lint: fully selected text reported");
            Check(Lint(p => p.Controls.Add(new ApcButton { Text = "A", Width = 0 })).Any(i => i.Contains("크기 0")), "lint: collapsed control reported");
            Check(Lint(p => { p.Controls.Add(new ApcButton { Text = "저장", Width = 70 }); p.Controls.Add(new ApcButton { Text = "취소", Left = 80, Width = 70 }); }).Any(i => i.Contains("버튼 순서")), "lint: confirm before cancel");
            Check(Lint(p =>
            {
                var g = new ApcGrid { Width = 300, Height = 100 };
                g.Columns.Add("a", "A");
                for (int r = 0; r < 20; r++) g.Rows.Add("x");
                p.Controls.Add(g);
            }).Any(i => i.Contains("세로 스크롤바")), "lint: grid scrollbar");
            Check(Lint(p =>
            {
                var g = new ApcGrid { Width = 300, Height = 250, ReadOnly = true };
                g.Columns.Add("a", "A");
                g.Rows.Add("x");
                p.Controls.Add(g);
            }).Any(i => LayoutLint.IsNote(i) && i.Contains("표 아래가 빔")), "lint: underfilled read-only grid is a note");
            using (var g = new ApcGrid())
                Check(g.DefaultCellStyle.Alignment == DataGridViewContentAlignment.MiddleCenter && g.ColumnHeadersDefaultCellStyle.Alignment == DataGridViewContentAlignment.MiddleCenter, "grid: cells and headers centered by default");
            Check(Lint(p => { p.Controls.Add(new ApcButton { Text = "취소", Width = 70 }); p.Controls.Add(new ApcButton { Text = "저장", Left = 80, Width = 70 }); }).Count == 0, "lint: clean row passes");
        }

        private static List<string> Lint(Action<Panel> build)
        {
            using (var host = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000), Size = new Size(460, 300) })
            {
                var panel = new Panel { Dock = DockStyle.Fill };
                host.Controls.Add(panel);
                build(panel);
                host.Show();
                Application.DoEvents();
                return LayoutLint.Inspect(panel);
            }
        }
    }
}
