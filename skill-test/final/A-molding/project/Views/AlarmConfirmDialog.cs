using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using MoldingLineMonitor.Data;

namespace MoldingLineMonitor.Views
{
    /// <summary>알람 확인: 알람 한 건 상세(읽기 전용) + 조치 메모 입력. 확인하면 Memo에 담고 OK로 닫는다.</summary>
    internal sealed class AlarmConfirmDialog : ApcForm
    {
        private readonly ApcTextBox _memo = new ApcTextBox { Multiline = true, Height = 110, ScrollBars = ScrollBars.Vertical };

        public AlarmConfirmDialog() : this(new AlarmRow(StatusKind.Alarm, "14:52:10", "IM-04", Criterion.Clamp.Message(508.4))) { }

        public AlarmConfirmDialog(AlarmRow alarm)
        {
            SetupDialog("알람 확인", 520);

            AddContent(new ApcHeaderBar { Title = alarm.Machine, Status = alarm.Status, StatusText = StatusWord(alarm.Status) });
            AddFieldRow("발생 시각", ReadOnlyBox(alarm.Time));
            AddFieldRow("설비", ReadOnlyBox(alarm.Machine));
            AddFieldRow("내용", ReadOnlyBox(alarm.Text));
            AddContent(SectionTitle("조치 메모"));
            AddContent(_memo);

            var ok = new ApcButton { Text = "확인", Variant = ApcButtonVariant.Primary };
            ok.Click += (s, e) => Confirm();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, ok);
        }

        public string Memo { get; private set; }

        private void Confirm()
        {
            if (_memo.Text.Trim().Length == 0) { ApcDialog.Alert(this, "조치 메모를 입력하세요."); return; }
            if (!ApcDialog.Confirm(this, "알람을 확인 처리할까요?", "확인", ApcButtonVariant.Primary)) return;
            Memo = _memo.Text.Trim();
            DialogResult = DialogResult.OK;
        }

        private static ApcTextBox ReadOnlyBox(string text) { return new ApcTextBox { Text = text, ReadOnly = true }; }

        private static string StatusWord(StatusKind k)
        {
            return k == StatusKind.Alarm ? "알람" : k == StatusKind.Warn ? "경고" : k == StatusKind.Stop ? "정지" : k == StatusKind.Info ? "기록" : "정상";
        }
    }
}
