namespace FATester.Data
{
    internal enum ChannelProcess { Testing, Done, Error, Waiting }

    /// <summary>시험 채널 한 개의 현재 상태(읽기 전용 값 객체). 값이 없으면 null/빈 문자열.</summary>
    internal sealed class ChannelInfo
    {
        public ChannelInfo(string name, ChannelProcess process, string sampleId, string testItem, int? progressPercent,
            double? resistanceMOhm, double? leakageNA, string errorCode, string errorText, string errorTime,
            double chamberTemp, double chamberHumidity, int cycles)
        {
            Name = name; Process = process; SampleId = sampleId; TestItem = testItem; ProgressPercent = progressPercent;
            ResistanceMOhm = resistanceMOhm; LeakageNA = leakageNA; ErrorCode = errorCode; ErrorText = errorText; ErrorTime = errorTime;
            ChamberTemp = chamberTemp; ChamberHumidity = chamberHumidity; Cycles = cycles;
        }

        public string Name { get; private set; }
        public ChannelProcess Process { get; private set; }
        public string SampleId { get; private set; }
        public string TestItem { get; private set; }
        public int? ProgressPercent { get; private set; }
        public double? ResistanceMOhm { get; private set; }
        public double? LeakageNA { get; private set; }
        public string ErrorCode { get; private set; }
        public string ErrorText { get; private set; }
        public string ErrorTime { get; private set; }
        public double ChamberTemp { get; private set; }
        public double ChamberHumidity { get; private set; }
        public int Cycles { get; private set; }
    }

    /// <summary>불량 분류 결과 한 행.</summary>
    internal sealed class DefectRow
    {
        public DefectRow(string sampleId, string mode, string location, string judgedAt)
        {
            SampleId = sampleId; Mode = mode; Location = location; JudgedAt = judgedAt;
        }

        public string SampleId { get; private set; }
        public string Mode { get; private set; }
        public string Location { get; private set; }
        public string JudgedAt { get; private set; }
    }
}
