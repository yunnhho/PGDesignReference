using System;
using System.Drawing;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using APC.UI.Input;
using APC.UI.Theming;

namespace APC.UI.Controls
{
    public sealed class ScanEventArgs : EventArgs
    {
        public ScanEventArgs(string code) { Code = code; Accepted = true; }
        public string Code { get; private set; }

        /// <summary>앱이 코드를 거부하면 false로 둔다 → 실패 피드백(빨강 + 경고음). 기본 true.</summary>
        public bool Accepted { get; set; }
    }

    /// <summary>
    /// 바코드 입력란. Attach(form) 하면 그 폼 어디에 포커스가 있든 스캔을 받는다.
    /// 스캔으로 판정되면 포커스 칸을 버스트 시작 전 값으로 되돌리고 코드를 여기로 옮긴 뒤 Scanned를 낸다.
    /// 스캔의 Enter는 삼켜서 폼의 기본 버튼(AcceptButton)을 누르지 않게 한다.
    /// 피드백은 Scanned 처리 후 한 번: e.Accepted에 따라 성공/실패.
    /// </summary>
    public class ApcScanBox : TextBox, IMessageFilter
    {
        public const int FlashMs = 300;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_CHAR = 0x0102;
        private const int VK_RETURN = 0x0D;

        private readonly Timer _flashTimer = new Timer { Interval = FlashMs };
        private ScanDetector _detector;
        private Form _host;

        // 버스트가 시작될 때의 포커스 칸 상태 — 스캔이면 이걸로 되돌린다.
        private TextBoxBase _snapBox;
        private string _snapText;
        private int _snapStart;
        private int _snapLength;

        public event EventHandler<ScanEventArgs> Scanned;

        public ApcScanBox()
        {
            ImeMode = ImeMode.Disable;   // 한글 IME 상태에서도 바코드가 자모로 조합되지 않게
            _flashTimer.Tick += (s, e) => { _flashTimer.Stop(); BackColor = SystemColors.Window; };
            ApcTheme.Changed += OnThemeChanged;
            ApplyTheme();
            ResetDetector();
        }

        internal bool? LastFlashSuccess { get; private set; }
        internal int FlashCount { get; private set; }

        public void Attach(Form host)
        {
            if (host == null) throw new ArgumentNullException("host");
            if (_host != null) Application.RemoveMessageFilter(this);
            _host = host;
            ResetDetector();
            Application.AddMessageFilter(this);
            host.FormClosed += (s, e) => Application.RemoveMessageFilter(this);
        }

        /// <summary>성공 = 초록 플래시 + 짧은 비프, 실패 = 빨강 플래시 + 경고음.</summary>
        public void Flash(bool success)
        {
            LastFlashSuccess = success;
            FlashCount++;
            if (ApcTheme.Current != null)
            {
                var c = ApcTheme.Status(success ? StatusKind.Run : StatusKind.Alarm);
                BackColor = ControlPaint.LightLight(c);
            }
            (success ? SystemSounds.Beep : SystemSounds.Hand).Play();
            _flashTimer.Stop();
            _flashTimer.Start();
        }

        [DllImport("user32.dll")]
        private static extern int GetMessageTime();

        bool IMessageFilter.PreFilterMessage(ref Message m)
        {
            if (_host == null || Form.ActiveForm != _host || !Visible || !Enabled) return false;
            var focused = FindFocused(_host);
            if (focused is ApcScanBox && focused != this) return false;   // 포커스된 다른 스캔 박스에 양보

            // 처리 시각이 아니라 메시지 발생 시각: UI가 바빠 키가 몰려 처리돼도 사람 입력을 스캔으로 오인하지 않는다.
            return Process(m.Msg, m.WParam.ToInt32(), (uint)GetMessageTime(), focused as TextBoxBase);
        }

        /// <summary>필터 본체. true = 메시지를 삼킴. timeMs = 메시지 발생 시각.</summary>
        internal bool Process(int msg, int wParam, long timeMs, TextBoxBase focused)
        {
            if (msg == WM_CHAR)
            {
                char c = (char)wParam;
                if (c == ScanDetector.Terminator) return false;
                _detector.Feed(c, timeMs);
                if (_detector.Length == 1) Snapshot(focused);   // 디스패치 전이라 아직 글자가 들어가지 않은 상태
                return false;
            }

            if (msg == WM_KEYDOWN && wParam == VK_RETURN)
            {
                string code = _detector.Feed(ScanDetector.Terminator, timeMs);
                if (code == null) return false;
                // KEYDOWN을 삼키면 TranslateMessage가 생략돼 WM_CHAR '\r'은 만들어지지 않는다.
                OnScanned(code, focused);
                return true;
            }
            return false;
        }

        private void Snapshot(TextBoxBase box)
        {
            _snapBox = box == this ? null : box;
            if (_snapBox == null) return;
            _snapText = box.Text;
            _snapStart = box.SelectionStart;
            _snapLength = box.SelectionLength;
        }

        private void OnScanned(string code, TextBoxBase focused)
        {
            RestoreSnapshot(focused);
            Text = code;
            SelectAll();
            var args = new ScanEventArgs(code);
            var handler = Scanned;
            if (handler != null) handler(this, args);
            Flash(args.Accepted);
        }

        /// <summary>스캔 글자가 다른 칸에 들어갔으면(선택 영역 덮어쓰기·필터된 글자 포함) 버스트 전 값으로 되돌린다.</summary>
        private void RestoreSnapshot(TextBoxBase focused)
        {
            var box = _snapBox;
            _snapBox = null;
            if (box == null || box != focused || box.IsDisposed || box.ReadOnly) return;
            if (box.Text != _snapText) box.Text = _snapText;
            box.Select(_snapStart, _snapLength);
        }

        private static Control FindFocused(ContainerControl container)
        {
            Control c = container.ActiveControl;
            var inner = c as ContainerControl;
            while (inner != null && inner.ActiveControl != null) { c = inner.ActiveControl; inner = c as ContainerControl; }
            return c;
        }

        private void ResetDetector()
        {
            var scan = ApcTheme.Current == null ? null : ApcTheme.Current.Scan;
            _detector = scan == null ? new ScanDetector(50, 4) : new ScanDetector(scan.MaxKeyIntervalMs, scan.MinLength);
        }

        private void ApplyTheme()
        {
            if (ApcTheme.Current == null) return;
            Font = ApcTheme.GetFont();
            MinimumSize = new Size(0, ApcTheme.MinHeight);
        }

        private void OnThemeChanged(object sender, EventArgs e) { ApplyTheme(); ResetDetector(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Application.RemoveMessageFilter(this);
                ApcTheme.Changed -= OnThemeChanged;
                _flashTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
