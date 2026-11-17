using System.IO;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;

namespace APC.UI.Checks
{
    /// <summary>최종 리뷰 Important 1~8 재현 점검.</summary>
    internal static partial class Program
    {
        private const int WM_KEYDOWN = 0x0100, WM_CHAR = 0x0102, VK_RETURN = 0x0D;
        private static string DefaultTheme { get { return Path.Combine(Root, Path.Combine("themes", "default")); } }

        private static void RunReviewFixChecks()
        {
            // #1 같은 테마 재로드 후 컨트롤 폰트가 살아 있어야 한다
            ApcTheme.Load(DefaultTheme);
            var btn = new ApcButton();
            ApcTheme.Load(DefaultTheme);
            Check(NoThrow(() => { var h = btn.Font.Height; }), "#1 font still usable after theme reload");

            // #2 스캔 Enter를 삼킨 뒤, 다음 사람 Enter의 WM_CHAR는 통과
            var box = new ApcScanBox();
            ScanChars(box, "LOT-0001", 1000, 10, null);
            Check(box.Process(WM_KEYDOWN, VK_RETURN, 1090, null), "#2 scan Enter keydown swallowed");
            Check(!box.Process(WM_CHAR, '\r', 5000, null), "#2 next human Enter char passes through");

            // #3 판정은 메시지 시각 기준: 사람이 300ms 간격으로 친 키가 한꺼번에 처리돼도 스캔 아님
            var box3 = new ApcScanBox();
            ScanChars(box3, "1200", 10000, 300, null);
            Check(!box3.Process(WM_KEYDOWN, VK_RETURN, 11500, null), "#3 queued human keys (by message time) not a scan");

            // #4 앱이 거부하면 피드백은 실패 한 번
            var box4 = new ApcScanBox();
            box4.Scanned += (s, e) => e.Accepted = false;
            ScanChars(box4, "BAD-0001", 20000, 10, null);
            box4.Process(WM_KEYDOWN, VK_RETURN, 20090, null);
            Check(box4.LastFlashSuccess == false && box4.FlashCount == 1, "#4 rejected scan flashes failure once");

            // #5 포커스 칸 원래 값(전체 선택 상태에서 덮어써짐)이 복원
            var field = new TextBox { Text = "120" };
            field.SelectAll();
            var box5 = new ApcScanBox();
            ScanChars(box5, "LOT-0005", 30000, 10, field);
            box5.Process(WM_KEYDOWN, VK_RETURN, 30090, field);
            Check(field.Text == "120", "#5 focused field restored after scan (was '" + field.Text + "')");
            Check(box5.Text == "LOT-0005", "#5 code moved to scan box");

            // #5b 숫자만 받는 칸(일부 글자만 들어감)도 복원
            var num = new TextBox { Text = "1" };
            num.SelectionStart = 1;
            var box5b = new ApcScanBox();
            ScanChars(box5b, "LOT-0001", 40000, 10, num, digitsOnly: true);
            box5b.Process(WM_KEYDOWN, VK_RETURN, 40090, num);
            Check(num.Text == "1", "#5b filtered field restored (was '" + num.Text + "')");

            // #7 리사이즈 가능하면 자식이 그립 영역을 덮지 않게 Padding 확보
            using (var f = new ApcForm())
            {
                Check(f.Padding.Left >= f.ResizeGrip && f.Padding.Right >= f.ResizeGrip && f.Padding.Bottom >= f.ResizeGrip, "#7 padding covers resize grip");
                f.Resizable = false;
                Check(f.Padding.Left == 1, "#7 non-resizable keeps 1px border");
                Check(f.ContentBounds.Left == f.Padding.Left, "#7 content bounds follow padding");
            }

            // #8 이미지가 빠진 테마는 Load 시점에 ThemeException (기존 테마 유지)
            Check(Throws<ThemeException>(() => ApcTheme.Load(Fx("alarm-dark")), "@1x.png"), "#8 missing assets rejected at load");
            Check(ApcTheme.ThemeDir == DefaultTheme, "#8 previous theme kept after failed load");
        }

        /// <summary>WM_CHAR를 필터에 넣고, 필터가 통과시킨 글자는 실제 디스패치처럼 field에 입력한다.</summary>
        private static void ScanChars(ApcScanBox box, string s, long t0, int gap, TextBox field, bool digitsOnly = false)
        {
            long t = t0;
            foreach (char c in s)
            {
                t += gap;
                bool swallowed = box.Process(WM_CHAR, c, t, field);
                if (!swallowed && field != null && (!digitsOnly || char.IsDigit(c))) field.SelectedText = c.ToString();
            }
        }

        private static bool NoThrow(System.Action a)
        {
            try { a(); return true; }
            catch (System.Exception) { return false; }
        }
    }
}
