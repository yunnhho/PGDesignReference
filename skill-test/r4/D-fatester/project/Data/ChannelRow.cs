using APC.UI.Theming;

namespace FATester.Data
{
    /// <summary>시험 채널 한 개의 현재 상태(불변). 판정은 ChannelRules 한 곳에서만 한다.</summary>
    internal sealed class ChannelRow
    {
        public ChannelRow(string name, string processText, StatusKind processStatus, string errorDetail,
            string testItem, string sampleId, int? progress, double? resistance, double? leak,
            double? chamberTemp, double? chamberHumi, int cycles,
            string lastErrorCode, string lastErrorText, string lastErrorTime)
        {
            Name = name; ProcessText = processText; ProcessStatus = processStatus; ErrorDetail = errorDetail;
            TestItem = testItem; SampleId = sampleId; Progress = progress; Resistance = resistance; Leak = leak;
            ChamberTemp = chamberTemp; ChamberHumi = chamberHumi; Cycles = cycles;
            LastErrorCode = lastErrorCode; LastErrorText = lastErrorText; LastErrorTime = lastErrorTime;
            ChannelRules.Summarize(this, out _status, out _statusText);
        }

        private readonly StatusKind _status;
        private readonly string _statusText;

        public string Name { get; private set; }
        /// <summary>시험중 / 완료 / 오류 / 대기</summary>
        public string ProcessText { get; private set; }
        public StatusKind ProcessStatus { get; private set; }
        /// <summary>오류일 때 사유(예: 전류 인가 실패)</summary>
        public string ErrorDetail { get; private set; }
        public string TestItem { get; private set; }
        public string SampleId { get; private set; }
        public int? Progress { get; private set; }
        public double? Resistance { get; private set; }
        public double? Leak { get; private set; }
        public double? ChamberTemp { get; private set; }
        public double? ChamberHumi { get; private set; }
        public int Cycles { get; private set; }
        public string LastErrorCode { get; private set; }
        public string LastErrorText { get; private set; }
        public string LastErrorTime { get; private set; }

        /// <summary>공정 상태와 값 판정 중 나쁜 쪽.</summary>
        public StatusKind Status { get { return _status; } }
        public string StatusText { get { return _statusText; } }
    }

    internal sealed class DefectRow
    {
        public DefectRow(string sampleId, string mode, string position, string judgedAt)
        {
            SampleId = sampleId; Mode = mode; Position = position; JudgedAt = judgedAt;
        }
        public string SampleId { get; private set; }
        public string Mode { get; private set; }
        public string Position { get; private set; }
        public string JudgedAt { get; private set; }
    }
}
