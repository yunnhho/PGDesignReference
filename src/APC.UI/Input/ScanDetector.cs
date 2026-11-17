using System.Text;

namespace APC.UI.Input
{
    /// <summary>
    /// 바코드 스캐너 입력 판별. 스캐너는 키를 사람보다 훨씬 빠르게 연달아 보내고 Enter로 끝낸다.
    /// 모든 키 간격(Enter 포함)이 maxKeyIntervalMs 이하이고 길이가 minLength 이상이면 스캔.
    /// </summary>
    public sealed class ScanDetector
    {
        public const char Terminator = '\r';

        private readonly int _maxKeyIntervalMs;
        private readonly int _minLength;
        private readonly StringBuilder _buffer = new StringBuilder();
        private long _lastTime;

        public ScanDetector(int maxKeyIntervalMs, int minLength)
        {
            _maxKeyIntervalMs = maxKeyIntervalMs;
            _minLength = minLength;
        }

        /// <summary>글자 하나를 넣는다. Enter에서 스캔으로 판정되면 코드, 아니면 null.</summary>
        public string Feed(char c, long timestampMs)
        {
            long gap = timestampMs - _lastTime;   // 음수 = 시계 되감김(TickCount 순환) → 버스트 아님
            bool inBurst = _buffer.Length > 0 && gap >= 0 && gap <= _maxKeyIntervalMs;
            _lastTime = timestampMs;

            if (c == Terminator)
            {
                string code = inBurst && _buffer.Length >= _minLength ? _buffer.ToString() : null;
                _buffer.Length = 0;
                return code;
            }

            if (!inBurst) _buffer.Length = 0;   // 느린 키 = 사람 입력, 새로 시작
            _buffer.Append(c);
            return null;
        }

        /// <summary>현재 쌓인 버스트 길이. Feed 직후 1이면 새 버스트가 시작된 것.</summary>
        public int Length { get { return _buffer.Length; } }

        public void Reset()
        {
            _buffer.Length = 0;
            _lastTime = 0;
        }
    }
}
