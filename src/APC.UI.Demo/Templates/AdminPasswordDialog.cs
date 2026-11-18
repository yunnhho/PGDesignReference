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
            SetupDialog(title, 400);
            AddFieldRow("비밀번호", _password);

            var ok = new ApcButton { Text = "확인", Variant = ApcButtonVariant.Primary, DialogResult = DialogResult.OK };
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, ok);
            AcceptButton = ok;
            Shown += (s, e) => _password.Focus();
        }

        public string Password { get { return _password.Text; } }
    }
}
