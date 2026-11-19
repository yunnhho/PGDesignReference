using System;

namespace FATester.Data
{
    /// <summary>불량 분류 결과 한 행(값 객체).</summary>
    internal sealed class DefectRow
    {
        public DefectRow(string sampleId, string mode, string position, DateTime judgedAt)
        {
            SampleId = sampleId; Mode = mode; Position = position; JudgedAt = judgedAt;
        }

        public string SampleId { get; private set; }
        public string Mode { get; private set; }
        public string Position { get; private set; }
        public DateTime JudgedAt { get; private set; }
    }
}
