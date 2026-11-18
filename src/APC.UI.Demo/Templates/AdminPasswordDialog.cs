using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;

namespace APC.UI.Demo.Templates
{
    /// <summary>
    /// 관리자 확인 dialog 골격(참고: Failure Analysis 관리자 확인). 설정 화면 진입 등 권한이 필요한 동작 앞에 둔다.
    /// 입력값 확인은 부른 쪽이 한다: <c>using (var d = new AdminPasswordDialog()) if (d.ShowDialog(owner) == DialogResult.OK && Check(d.Password)) ...</c>
    /// 여기서는 Enter = 확인(비밀번호 입력 뒤 Enter가 자연스럽다). 바코드 스캔이 오는 화면이 아니다.
    /// </summary>
    internal sealed class AdminPasswordDialog : ApcForm
    {
        private readonly ApcTextBox _password = new ApcTextBox { UseSystemPasswordChar = true, MaxLength = 32 };

        public AdminPasswordDialog() : this("관리자 확인") { }

        public AdminPasswordDialog(string title)
        {
            TitleText = title;
            Resizable = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(ApcTheme.Px(400), ApcTheme.Px(250));

            int w = ClientSize.Width, top = ContentBounds.Top;
            var caption = new Label
            {
                Text = "비밀번호", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent,
                Font = ApcTheme.GetFont(FontStyle.Bold), ForeColor = ApcTheme.C(ApcTheme.Current.Colors.Text),
                Bounds = new Rectangle(0, top + ApcTheme.Px(20), w, ApcTheme.Px(28)),
            };
            int fieldW = ApcTheme.Px(220);
            _password.SetBounds((w - fieldW) / 2, caption.Bottom + ApcTheme.Px(6), fieldW, ApcTheme.Px(ApcTextBox.BaseHeight));
            _password.TextAlign = HorizontalAlignment.Center;

            int bw = ApcTheme.Px(120), bh = ApcTheme.Px(44), gap = ApcTheme.Px(10);
            var ok = new ApcButton { Text = "확인", Variant = ApcButtonVariant.Primary, DialogResult = DialogResult.OK };
            var cancel = new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel };
            int by = ClientSize.Height - ApcTheme.Px(20) - bh;
            cancel.SetBounds(w / 2 - gap / 2 - bw, by, bw, bh);   // [취소][확인] — 확정 버튼이 오른쪽
            ok.SetBounds(w / 2 + gap / 2, by, bw, bh);

            Controls.Add(caption);
            Controls.Add(_password);
            Controls.Add(ok);
            Controls.Add(cancel);
            AcceptButton = ok;
            CancelButton = cancel;
            Shown += (s, e) => _password.Focus();
        }

        public string Password { get { return _password.Text; } }
    }
}
