using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;

namespace APC.UI.Demo.Templates
{
    /// <summary>
    /// 통신·장비 설정 dialog 골격(참고: Failure Analysis 시스템 설정). 블록(설비 묶음)마다 ApcGroup 하나에
    /// 포트·신호·로직 선택 + 연결 시도, 아래 공통 설정 묶음, 맨 아래 왼쪽 [비밀번호 변경] / 오른쪽 [취소][저장].
    /// 저장은 Confirm("저장") 뒤 결과를 넘긴다. 실제 연결 시도·저장은 부른 쪽 데이터 인터페이스가 한다.
    /// </summary>
    internal sealed class SettingsDialog : ApcForm
    {
        private static readonly string[] Ports = { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6" };
        private static readonly string[] Signals = { "INB", "RESET", "START" };
        private static readonly string[] Logics = { "LOW", "HIGH" };
        private const int RowH = 34;

        public SettingsDialog() : this(new[] { "BLOCK#1 · 저속 연마기 ST-GR-01~03", "BLOCK#2 · 저속 연마기 ST-GR-04~06", "BLOCK#3 · 고속 연마기 ST-GB-01~08" }) { }

        public SettingsDialog(string[] blocks)
        {
            TitleText = "시스템 설정";
            Resizable = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;

            int pad = ApcTheme.Px(14), gap = ApcTheme.Px(10), groupH = ApcTheme.Px(30 + 10 + RowH + 10), btnH = ApcTheme.Px(40);
            ClientSize = new Size(ApcTheme.Px(680), ContentBounds.Top + pad + (groupH + gap) * (blocks.Length + 1) + btnH + pad + Padding.Bottom);

            var content = new Panel { Bounds = ContentBounds, Padding = new Padding(pad), BackColor = Color.Transparent };

            // 맨 아래 버튼 줄: 왼쪽 비밀번호 변경, 오른쪽 [취소][저장]
            var buttons = new Panel { Dock = DockStyle.Bottom, Height = btnH, BackColor = Color.Transparent };
            var password = new ApcButton { Text = "비밀번호 변경", Width = ApcTheme.Px(150), Dock = DockStyle.Left };
            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary, Width = ApcTheme.Px(120), Dock = DockStyle.Right };
            var cancel = new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel, Width = ApcTheme.Px(110), Dock = DockStyle.Right };
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(new Panel { Width = gap, Dock = DockStyle.Right, BackColor = Color.Transparent });
            buttons.Controls.Add(save);
            buttons.Controls.Add(password);
            password.Click += (s, e) => { using (var d = new AdminPasswordDialog("비밀번호 변경")) d.ShowDialog(this); };
            save.Click += (s, e) => { if (ApcDialog.Confirm(this, "설정을 저장할까요?", "저장", ApcButtonVariant.Primary)) DialogResult = DialogResult.OK; };

            // 묶음: Top 도킹은 나중 Add가 위 → 공통 먼저, 블록은 뒤에서부터
            content.Controls.Add(buttons);
            content.Controls.Add(Group("블럭 통신 공통", groupH, gap,
                Caption("응답대기(ms)"), new ApcNumericBox { Minimum = 10, Maximum = 5000, Increment = 10, Value = 100, Width = ApcTheme.Px(120) },
                Caption("속도"), Combo(new[] { "9600", "19200", "38400", "115200" }, 0, 100),
                Caption("갱신주기(ms)"), new ApcNumericBox { Minimum = 100, Maximum = 10000, Increment = 100, Value = 500, Width = ApcTheme.Px(120) }));
            for (int i = blocks.Length - 1; i >= 0; i--)
            {
                var port = Ports[i % Ports.Length];
                var connect = new ApcButton { Text = "연결 시도", Width = ApcTheme.Px(100), Height = ApcTheme.Px(RowH) };
                connect.Click += (s, e) => ApcDialog.Alert(this, port + " 연결 시도 결과를 여기에 표시(샘플)");
                content.Controls.Add(Group(blocks[i], groupH, gap,
                    Caption("포트"), Combo(Ports, i % Ports.Length, 100),
                    Caption("신호"), Combo(Signals, i == 2 ? 1 : 0, 110),
                    Caption("로직"), Combo(Logics, 0, 100), connect));
            }
            Controls.Add(content);
            CancelButton = cancel;
        }

        /// <summary>제목 묶음 하나(한 줄 입력) + 아래 간격. Top 도킹용 패널을 돌려준다.</summary>
        private static Panel Group(string title, int height, int gapBelow, params Control[] row)
        {
            var g = new ApcGroup { Text = title, Dock = DockStyle.Fill };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, BackColor = Color.Transparent, Margin = Padding.Empty };
            foreach (var c in row)
            {
                c.Height = ApcTheme.Px(RowH);
                c.Margin = new Padding(0, 0, ApcTheme.Px(8), 0);
                flow.Controls.Add(c);
            }
            g.Controls.Add(flow);
            // 묶음 사이 간격 = 감싼 패널의 아래 Padding
            var wrap = new Panel { Dock = DockStyle.Top, Height = height + gapBelow, Padding = new Padding(0, 0, 0, gapBelow), BackColor = Color.Transparent };
            wrap.Controls.Add(g);
            return wrap;
        }

        private static Label Caption(string text)
        {
            return new Label
            {
                Text = text, AutoSize = false, Width = TextRenderer.MeasureText(text, ApcTheme.GetFont()).Width + ApcTheme.Px(4),
                TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent,
                Font = ApcTheme.GetFont(), ForeColor = ApcTheme.C(ApcTheme.Current.Colors.Text),
            };
        }

        private static ApcComboBox Combo(string[] items, int selected, int width)
        {
            var c = new ApcComboBox { Width = ApcTheme.Px(width) };
            c.Items.AddRange(items);
            c.SelectedIndex = selected;
            return c;
        }
    }
}
