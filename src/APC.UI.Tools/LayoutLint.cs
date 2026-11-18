using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace APC.UI.Tools
{
    /// <summary>
    /// 배치가 끝난(화면에 띄운) 컨트롤 트리를 검사해 사람이 render PNG에서 지적하던 문제를 글로 돌려준다:
    /// 글자 잘림, 부모 밖 넘침·형제 겹침, 같은 줄 입력칸·버튼 위·높이 어긋남, 표준 컨트롤, 표 스크롤바, [취소][확인] 순서.
    /// APC.UI 내부(입력칸 속 TextBox, 표 스크롤바 등)는 들어가지 않는다. APC.UI 사본이 둘일 수 있어 형식은 이름으로 비교한다.
    /// </summary>
    public static class LayoutLint
    {
        private const string ApcAssembly = "APC.UI";
        private const int TextPad = 8;   // 버튼·이름표 안쪽 여유(양쪽 합)
        private static readonly HashSet<string> Forbidden = new HashSet<string>
        {
            "Button", "TextBox", "MaskedTextBox", "RichTextBox", "ComboBox", "ListBox", "NumericUpDown",
            "DateTimePicker", "DataGridView", "GroupBox", "CheckedListBox", "ListView",
        };
        private static readonly HashSet<string> FieldTypes = new HashSet<string>
        {
            "ApcTextBox", "ApcScanBox", "ApcComboBox", "ApcNumericBox", "ApcDateBox", "ApcButton",
        };
        private static readonly HashSet<string> ConfirmTexts = new HashSet<string> { "확인", "저장", "적용", "등록", "삭제" };

        /// <summary>문제가 아닌 참고(판단은 사람·에이전트 몫). 건수에 넣지 않는다.</summary>
        public const string NotePrefix = "참고: ";

        public static bool IsNote(string issue) { return issue.StartsWith(NotePrefix, StringComparison.Ordinal); }

        public static List<string> Inspect(Control root)
        {
            var issues = new List<string>();
            Walk(root, root.GetType().Name, issues);
            return issues;
        }

        private static void Walk(Control c, string path, List<string> issues)
        {
            var shown = c.Controls.Cast<Control>().Where(x => x.Visible).ToList();
            foreach (var z in shown.Where(x => x.Width <= 0 || x.Height <= 0))
                issues.Add(path + " > " + Describe(z) + ": 크기 0으로 접힘(" + z.Width + "x" + z.Height + ") — 자리가 모자란다");
            var visible = shown.Where(x => x.Width > 0 && x.Height > 0).ToList();
            CheckSiblings(c, visible, path, issues);
            foreach (var child in visible)
            {
                string p = path + " > " + Describe(child);
                CheckOne(child, p, issues);
                if (!IsApc(child) || Is(child, "ApcGroup")) Walk(child, p, issues);
            }
        }

        private static void CheckOne(Control c, string path, List<string> issues)
        {
            var t = c.GetType();
            if (t.Namespace == "System.Windows.Forms" && Forbidden.Contains(t.Name))
                issues.Add(path + ": 표준 " + t.Name + " — Apc 컨트롤로 바꾼다");
            else if (t.BaseType == typeof(Control) && t.Assembly != typeof(Control).Assembly && !IsApc(c))
                issues.Add(path + ": Control을 직접 상속해 그림 — Apc 표시 컨트롤로 바꾼다");

            if (c is DataGridView grid) CheckGrid(grid, path, issues);
            if (c is TextBoxBase tb && tb.SelectionLength > 0 && tb.SelectionLength == tb.TextLength && (tb.Focused || !tb.HideSelection))
                issues.Add(path + ": 글자 전체가 선택된 채 표시(파란 칠) — 첫 포커스 칸은 SelectionStart = Text.Length로");
            int need = TextWidth(c);
            if (need > 0 && need > c.ClientSize.Width)
                issues.Add(path + ": 글자 잘림(필요 " + need + "px > 폭 " + c.ClientSize.Width + "px)");
        }

        // 잘림 판정에 쓸 필요 폭. 0이면 검사하지 않는다.
        private static int TextWidth(Control c)
        {
            if (c is Label l && !l.AutoSize && l.Text.Length > 0)
            {
                // 여러 줄 이름표는 높이로 본다
                var wrapped = TextRenderer.MeasureText(l.Text, l.Font, new Size(l.ClientSize.Width, 0), TextFormatFlags.WordBreak);
                return wrapped.Height > l.ClientSize.Height ? TextRenderer.MeasureText(l.Text, l.Font).Width : 0;
            }
            if (Is(c, "ApcButton") && c.Text.Length > 0)
                return TextRenderer.MeasureText(c.Text, c.Font).Width + TextPad;
            if (c is ComboBox combo && combo.Items.Count > 0)
            {
                int widest = combo.Items.Cast<object>().Max(i => TextRenderer.MeasureText(combo.GetItemText(i), combo.Font).Width);
                return widest + Math.Max(c.Height - 2, 24) + TextPad;   // 화살표 영역(FieldFrame.ButtonWidth와 같은 식)
            }
            return 0;
        }

        private static void CheckGrid(DataGridView grid, string path, List<string> issues)
        {
            // 실제 스크롤바 컨트롤을 본다(숨긴 행·이미 스크롤된 표도 맞게).
            if (grid.Controls.OfType<VScrollBar>().Any(v => v.Visible))
                issues.Add(path + ": 표 세로 스크롤바(행 " + grid.RowCount + " 중 " + grid.DisplayedRowCount(false) + "행 보임) — 샘플 행을 줄이거나 표를 키운다");
            if (grid.Controls.OfType<HScrollBar>().Any(h => h.Visible))
                issues.Add(path + ": 표 가로 스크롤바 — 열 폭을 줄이거나 Fill로");

            // 읽기 전용 표(이력·목록)는 샘플로 높이를 채운다. 입력 표(ReadOnly=false)는 칸 수가 정해져 있어 제외.
            if (!grid.ReadOnly || grid.RowTemplate.Height <= 0) return;
            int body = grid.ClientSize.Height - (grid.ColumnHeadersVisible ? grid.ColumnHeadersHeight : 0);
            int fits = body / grid.RowTemplate.Height;
            if (grid.RowCount < fits)
                issues.Add(NotePrefix + path + ": 표 아래가 빔(행 " + grid.RowCount + ", " + fits + "행까지 들어감) — 이력·목록이면 샘플 행을 " + fits + "개로, 설비 수처럼 정해진 목록이면 그대로");
        }

        private static void CheckSiblings(Control parent, List<Control> kids, string path, List<string> issues)
        {
            var area = parent.ClientRectangle;
            bool scrolls = parent is ScrollableControl s && s.AutoScroll;
            foreach (var k in kids)
                if (!scrolls && !area.Contains(k.Bounds))
                    issues.Add(path + " > " + Describe(k) + ": 부모 밖으로 넘침(" + Fmt(k.Bounds) + " / 부모 " + area.Width + "x" + area.Height + ")");

            for (int i = 0; i < kids.Count; i++)
                for (int j = i + 1; j < kids.Count; j++)
                {
                    var a = kids[i];
                    var b = kids[j];
                    if (a.Bounds.IntersectsWith(b.Bounds))
                        issues.Add(path + ": " + Describe(a) + "(" + Fmt(a.Bounds) + ")와(과) " + Describe(b) + "(" + Fmt(b.Bounds) + ") 겹침");
                    else if (SameRow(a, b) && (a.Top != b.Top || a.Height != b.Height))
                        issues.Add(path + ": 같은 줄 " + Describe(a) + "(위 " + a.Top + ", 높이 " + a.Height + ")와(과) " + Describe(b) + "(위 " + b.Top + ", 높이 " + b.Height + ") 어긋남");
                    CheckOrder(a, b, path, issues);
                }
        }

        private static bool SameRow(Control a, Control b)
        {
            if (!FieldTypes.Contains(a.GetType().Name) && !FieldTypes.Contains(Base(a))) return false;
            if (!FieldTypes.Contains(b.GetType().Name) && !FieldTypes.Contains(Base(b))) return false;
            // 세로 범위가 겹치고 중심선이 가까워야 같은 줄(2열 폼의 엇갈린 행은 제외)
            int ca = a.Top + a.Height / 2, cb = b.Top + b.Height / 2;
            return a.Top < b.Bottom && b.Top < a.Bottom && Math.Abs(ca - cb) < Math.Min(a.Height, b.Height) / 2;
        }

        // [취소][확인]: 확정 버튼이 오른쪽 끝(ApcDialog와 같은 순서)
        private static void CheckOrder(Control a, Control b, string path, List<string> issues)
        {
            if (!Is(a, "ApcButton") || !Is(b, "ApcButton") || a.Top >= b.Bottom || b.Top >= a.Bottom) return;
            var cancel = a.Text == "취소" ? a : b.Text == "취소" ? b : null;
            var confirm = cancel == a ? b : a;
            if (cancel == null || !ConfirmTexts.Contains(confirm.Text)) return;
            if (cancel.Left > confirm.Left)
                issues.Add(path + ": 버튼 순서 [" + confirm.Text + "][취소] — [취소][" + confirm.Text + "]로");
        }

        private static bool IsApc(Control c) { return c.GetType().Assembly.GetName().Name == ApcAssembly; }
        private static string Base(Control c) { return c.GetType().BaseType == null ? "" : c.GetType().BaseType.Name; }

        private static bool Is(Control c, string apcType)
        {
            for (var t = c.GetType(); t != null; t = t.BaseType)
                if (t.Name == apcType && t.Assembly.GetName().Name == ApcAssembly) return true;
            return false;
        }

        private static string Describe(Control c)
        {
            string name = string.IsNullOrEmpty(c.Name) ? c.GetType().Name : c.Name;
            string text = c.Text;
            if (string.IsNullOrEmpty(text) || c is TextBoxBase || c is ComboBox) return name;
            if (text.Length > 12) text = text.Substring(0, 12) + "…";
            return name + " '" + text + "'";
        }

        private static string Fmt(Rectangle r) { return r.X + "," + r.Y + " " + r.Width + "x" + r.Height; }
    }
}
