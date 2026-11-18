using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using APC.UI.Theming;

namespace APC.UI.Controls
{
    /// <summary>
    /// dialog 배치 헬퍼. 생성자에서 SetupDialog → AddFieldRow/AddGroup/AddContent/AddContentFill(위에서 아래 순서) → SetFooter.
    /// 치수는 ApcLayout이 정하고, 표시 직전(OnLoad) 한 번에 배치한다: 이름표 열 하나(가장 긴 이름표 폭), 입력칸 왼쪽 끝 통일,
    /// 하단 [취소][확정] 오른쪽 끝, 높이는 내용으로 계산(Fill 내용이 있으면 SetupDialog 높이).
    /// </summary>
    public partial class ApcForm
    {
        private enum BlockKind { Row, Group, Content, Fill, Title }

        private sealed class Block
        {
            public BlockKind Kind;
            public Label Caption;
            public Control Field;
            public Control[] Trailing;
            public ApcGroup Group;
            public readonly List<Block> Rows = new List<Block>();
            public Control Content;
        }

        private readonly List<Block> _blocks = new List<Block>();
        private int _dialogWidth, _dialogHeight;
        private ApcButton _footerCancel, _footerConfirm;
        private ApcButton[] _footerLeft = new ApcButton[0];

        /// <summary>dialog 기본 속성과 크기(배율 전 px). height 0 = 내용으로 자동(AddContentFill을 쓰면 필수).</summary>
        protected void SetupDialog(string title, int width, int height = 0)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException("width");
            TitleText = title;
            Resizable = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            _dialogWidth = width;
            _dialogHeight = height;
            ClientSize = new Size(ApcTheme.Px(width), height > 0 ? ApcTheme.Px(height) : ClientSize.Height);
        }

        /// <summary>이름표 + 입력칸 한 줄. trailing이 없으면 입력칸이 남은 폭을 채우고, 있으면 모두 자기 Width로 왼→오.</summary>
        protected void AddFieldRow(string caption, Control field, params Control[] trailing)
        {
            _blocks.Add(NewRow(caption, field, trailing, this));
        }

        /// <summary>묶음 안 한 줄. 이름표 열은 묶음 밖 행과 같은 위치에 맞춘다.</summary>
        protected void AddFieldRow(ApcGroup group, string caption, Control field, params Control[] trailing)
        {
            var block = _blocks.FirstOrDefault(b => b.Group == group);
            if (block == null) throw new InvalidOperationException("AddGroup으로 만든 묶음이 아닙니다.");
            block.Rows.Add(NewRow(caption, field, trailing, group));
        }

        /// <summary>제목 있는 묶음(ApcGroup). 행은 AddFieldRow(group, ...)로 넣는다.</summary>
        protected ApcGroup AddGroup(string title)
        {
            RequireSetup();
            var g = new ApcGroup { Text = title };
            Controls.Add(g);
            _blocks.Add(new Block { Kind = BlockKind.Group, Group = g });
            return g;
        }

        /// <summary>표·판정판 등 자유 요소. 높이는 c.Height(ApcTheme.Px로 지정), 폭은 내용 폭.</summary>
        protected void AddContent(Control c)
        {
            RequireSetup();
            c.Dock = DockStyle.None;   // ApcHeaderBar처럼 기본 Dock이 있는 컨트롤도 헬퍼가 둔 자리에
            Controls.Add(c);
            _blocks.Add(new Block { Kind = c is Label && _sectionTitles.Contains(c) ? BlockKind.Title : BlockKind.Content, Content = c });
        }

        /// <summary>남은 높이를 채우는 요소(그래프 등). SetupDialog에 높이를 줘야 하고 한 dialog에 하나만.</summary>
        protected void AddContentFill(Control c)
        {
            RequireSetup();
            if (_dialogHeight <= 0) throw new InvalidOperationException("AddContentFill은 SetupDialog에 높이를 줘야 합니다.");
            if (_blocks.Any(b => b.Kind == BlockKind.Fill)) throw new InvalidOperationException("AddContentFill은 한 번만 쓸 수 있습니다.");
            c.Dock = DockStyle.None;
            Controls.Add(c);
            _blocks.Add(new Block { Kind = BlockKind.Fill, Content = c });
        }

        /// <summary>하단 버튼 줄: 오른쪽 [취소][확정](cancel은 null 가능), 왼쪽 보조 버튼. Esc = 취소(없으면 확정).</summary>
        protected void SetFooter(ApcButton cancel, ApcButton confirm, params ApcButton[] left)
        {
            RequireSetup();
            if (confirm == null) throw new ArgumentNullException("confirm");
            _footerCancel = cancel;
            _footerConfirm = confirm;
            _footerLeft = left ?? new ApcButton[0];
            foreach (var b in _footerLeft.Concat(new[] { cancel, confirm }).Where(b => b != null)) Controls.Add(b);
            // CancelButton으로 지정하면 WinForms가 DialogResult 없는 버튼에 Cancel을 넣어 Click 처리 전에 닫힌다
            // → 확정 버튼은 DialogResult가 이미 있을 때(보기용 닫기 = OK)만 Esc에 묶는다.
            if (cancel != null) CancelButton = cancel;
            else if (confirm.DialogResult != DialogResult.None) CancelButton = confirm;
        }

        private readonly HashSet<Control> _sectionTitles = new HashSet<Control>();

        /// <summary>구획 제목(굵게). AddContent로 넣으면 아래 요소와의 간격이 좁다.</summary>
        protected Label SectionTitle(string text)
        {
            var l = new Label
            {
                Text = text, AutoSize = false, Height = ApcTheme.Px(28), TextAlign = ContentAlignment.BottomLeft, BackColor = Color.Transparent,
                Font = ApcTheme.GetFont(FontStyle.Bold), ForeColor = ApcTheme.C(ApcTheme.Current.Colors.Text),
            };
            _sectionTitles.Add(l);
            return l;
        }

        /// <summary>행 안 trailing 이름표(보통 굵기, 글자 폭).</summary>
        protected static Label FieldCaption(string text)
        {
            var font = ApcTheme.GetFont();
            return new Label
            {
                Text = text, AutoSize = false, Width = TextRenderer.MeasureText(text, font).Width + ApcTheme.Px(4),
                TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent,
                Font = font, ForeColor = ApcTheme.C(ApcTheme.Current.Colors.Text),
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            if (_dialogWidth > 0) LayoutDialog();   // 가운데 배치(base.OnLoad) 전에 크기를 정한다
            base.OnLoad(e);
        }

        private void RequireSetup()
        {
            if (_dialogWidth <= 0) throw new InvalidOperationException("SetupDialog를 먼저 호출해야 합니다.");
        }

        private Block NewRow(string caption, Control field, Control[] trailing, Control parent)
        {
            RequireSetup();
            if (field == null) throw new ArgumentNullException("field");
            var label = new Label
            {
                Text = caption, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent,
                Font = ApcTheme.GetFont(), ForeColor = ApcTheme.C(ApcTheme.Current.Colors.Text),
            };
            parent.Controls.Add(label);
            parent.Controls.Add(field);
            foreach (var t in trailing ?? new Control[0]) parent.Controls.Add(t);
            return new Block { Kind = BlockKind.Row, Caption = label, Field = field, Trailing = trailing ?? new Control[0] };
        }

        private void LayoutDialog()
        {
            int pad = ApcTheme.Px(ApcLayout.Pad), fieldH = ApcTheme.Px(ApcLayout.FieldHeight);
            int btnW = ApcTheme.Px(ApcLayout.FooterButtonWidth), btnH = ApcTheme.Px(ApcLayout.FooterButtonHeight);
            bool hasFooter = _footerConfirm != null;

            // 이름표 열: 묶음 안 이름표는 묶음 안쪽 여백만큼 좁게 → 입력칸 왼쪽 끝이 dialog 전체에서 같다
            int labelW = 0;
            foreach (var b in _blocks)
            {
                if (b.Kind == BlockKind.Row) labelW = Math.Max(labelW, TextW(b.Caption));
                if (b.Kind == BlockKind.Group) foreach (var r in b.Rows) labelW = Math.Max(labelW, TextW(r.Caption) + b.Group.Padding.Left);
            }

            int fixedH = 0;
            for (int i = 0; i < _blocks.Count; i++) fixedH += Gap(i) + BlockHeight(_blocks[i], fieldH);
            int footerSpace = hasFooter ? (_blocks.Count > 0 ? ApcTheme.Px(ApcLayout.GroupGap) : 0) + btnH : 0;
            if (_dialogHeight <= 0)
                ClientSize = new Size(ClientSize.Width, Padding.Top + pad + fixedH + footerSpace + pad + Padding.Bottom);

            var cb = ContentBounds;
            int x = cb.Left + pad, w = cb.Width - 2 * pad, y = cb.Top + pad;
            int fillH = Math.Max(0, cb.Height - 2 * pad - fixedH - footerSpace);
            for (int i = 0; i < _blocks.Count; i++)
            {
                var b = _blocks[i];
                y += Gap(i);
                int h = b.Kind == BlockKind.Fill ? fillH : BlockHeight(b, fieldH);
                switch (b.Kind)
                {
                    case BlockKind.Row:
                        PlaceRow(b, x, y, x + w, labelW, fieldH);
                        break;
                    case BlockKind.Group:
                        b.Group.SetBounds(x, y, w, h);
                        var gp = b.Group.Padding;
                        int ry = gp.Top;
                        foreach (var r in b.Rows)
                        {
                            PlaceRow(r, gp.Left, ry, w - gp.Right, labelW - gp.Left, fieldH);
                            ry += fieldH + ApcTheme.Px(ApcLayout.RowGap);
                        }
                        break;
                    default:
                        b.Content.SetBounds(x, y, w, h);
                        break;
                }
                y += h;
            }

            if (!hasFooter) return;
            int by = cb.Bottom - pad - btnH, gap = ApcTheme.Px(ApcLayout.ItemGap);
            _footerConfirm.SetBounds(cb.Right - pad - btnW, by, btnW, btnH);
            if (_footerCancel != null) _footerCancel.SetBounds(_footerConfirm.Left - gap - btnW, by, btnW, btnH);
            int lx = x;
            foreach (var l in _footerLeft)
            {
                l.SetBounds(lx, by, btnW, btnH);
                lx += btnW + gap;
            }
        }

        // 블록 i 앞 간격: 입력 행끼리 RowGap, 구획 제목 다음 ItemGap, 그 밖 GroupGap
        private int Gap(int i)
        {
            if (i == 0) return 0;
            var prev = _blocks[i - 1].Kind;
            var cur = _blocks[i].Kind;
            if (prev == BlockKind.Row && cur == BlockKind.Row) return ApcTheme.Px(ApcLayout.RowGap);
            if (prev == BlockKind.Title) return ApcTheme.Px(ApcLayout.ItemGap);
            return ApcTheme.Px(ApcLayout.GroupGap);
        }

        private static int BlockHeight(Block b, int fieldH)
        {
            switch (b.Kind)
            {
                case BlockKind.Row: return fieldH;
                case BlockKind.Group:
                    int n = b.Rows.Count;
                    return b.Group.Padding.Vertical + n * fieldH + Math.Max(0, n - 1) * ApcTheme.Px(ApcLayout.RowGap);
                case BlockKind.Fill: return 0;
                default: return b.Content.Height;
            }
        }

        private static void PlaceRow(Block r, int x, int y, int right, int labelW, int fieldH)
        {
            r.Caption.SetBounds(x, y, labelW, fieldH);
            int fx = x + labelW + ApcTheme.Px(ApcLayout.LabelGap);
            if (r.Trailing.Length == 0)
            {
                r.Field.SetBounds(fx, y, right - fx, fieldH);
                return;
            }
            r.Field.SetBounds(fx, y, r.Field.Width, fieldH);
            int tx = r.Field.Right + ApcTheme.Px(ApcLayout.ItemGap);
            foreach (var t in r.Trailing)
            {
                t.SetBounds(tx, y, t.Width, fieldH);
                tx = t.Right + ApcTheme.Px(ApcLayout.ItemGap);
            }
        }

        private static int TextW(Label l) { return TextRenderer.MeasureText(l.Text, l.Font).Width + ApcTheme.Px(4); }
    }
}
