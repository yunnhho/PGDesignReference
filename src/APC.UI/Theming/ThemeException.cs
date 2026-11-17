using System;

namespace APC.UI.Theming
{
    /// <summary>theme.json을 읽을 수 없거나 규칙 위반. 메시지에 파일 경로와 필드명을 담는다.</summary>
    [Serializable]
    public sealed class ThemeException : Exception
    {
        public ThemeException(string message) : base(message) { }
        public ThemeException(string message, Exception inner) : base(message, inner) { }
    }
}
