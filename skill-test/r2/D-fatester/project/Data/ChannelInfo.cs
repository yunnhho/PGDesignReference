using APC.UI.Theming;

namespace FATester.Data
{
    /// <summary>설비 상세 dialog용 챔버 값.</summary>
    internal sealed class ChannelDetail
    {
        public readonly double Temp, TempSet, TempUpper, Humi, HumiSet, HumiUpper;
        public readonly int Cycles;
        public readonly string ErrorCode, ErrorNote;     // 없으면 ""
        public readonly StatusKind ErrorStatus;
        public readonly double[] Trend;                  // 최근 1시간 온도, 마지막 점 = Temp

        public ChannelDetail(double temp, double tempSet, double tempUpper, double humi, double humiSet, double humiUpper,
            int cycles, string errorCode, string errorNote, StatusKind errorStatus, double[] trend)
        {
            Temp = temp; TempSet = tempSet; TempUpper = tempUpper; Humi = humi; HumiSet = humiSet; HumiUpper = humiUpper;
            Cycles = cycles; ErrorCode = errorCode; ErrorNote = errorNote; ErrorStatus = errorStatus; Trend = trend;
        }
    }

    /// <summary>시험 채널 한 개의 현재 상태.</summary>
    internal sealed class ChannelInfo
    {
        public readonly string Name, State, SampleId, TestItem, ErrorText;
        public readonly StatusKind ProcessStatus;
        public readonly double? Progress, Resistance, Leakage;   // null = 값 없음
        public readonly ChannelDetail Detail;

        public ChannelInfo(string name, string state, StatusKind processStatus, double? progress, string sampleId, string testItem,
            double? resistance, double? leakage, string errorText, ChannelDetail detail)
        {
            Name = name; State = state; ProcessStatus = processStatus; Progress = progress; SampleId = sampleId; TestItem = testItem;
            Resistance = resistance; Leakage = leakage; ErrorText = errorText; Detail = detail;
        }

        public StatusKind ResistanceStatus { get { return Judge.Of(Resistance, Limits.ResistanceWarn, Limits.ResistanceUpper); } }
        public StatusKind LeakageStatus { get { return Judge.Of(Leakage, Limits.LeakageWarn, Limits.LeakageUpper); } }

        private static int Severity(StatusKind k) { return k == StatusKind.Alarm ? 2 : k == StatusKind.Warn ? 1 : 0; }

        /// <summary>타일 상태 = 공정 상태와 값 판정 중 나쁜 쪽. 사유가 여럿이면 가장 나쁜 것 하나(같으면 앞쪽).</summary>
        public StatusKind Status
        {
            get
            {
                if (ProcessStatus == StatusKind.Alarm || ProcessStatus == StatusKind.Stop) return ProcessStatus;
                return Severity(LeakageStatus) > Severity(ResistanceStatus) ? LeakageStatus : (Severity(ResistanceStatus) > 0 ? ResistanceStatus : ProcessStatus);
            }
        }

        public string StatusText
        {
            get
            {
                if (ProcessStatus == StatusKind.Alarm) return State + " · " + ErrorText;
                if (ProcessStatus == StatusKind.Stop) return State;
                bool leakWorse = Severity(LeakageStatus) > Severity(ResistanceStatus);
                StatusKind k = leakWorse ? LeakageStatus : ResistanceStatus;
                if (Severity(k) == 0) return State;
                string what = leakWorse ? "누설전류" : "저항";
                return State + " · " + what + (k == StatusKind.Alarm ? " 상한 초과" : " 한계 근접");
            }
        }
    }

    /// <summary>불량 분류 결과 한 행.</summary>
    internal sealed class DefectRow
    {
        public readonly string SampleId, Mode, Location, JudgedAt;
        public DefectRow(string sampleId, string mode, string location, string judgedAt)
        {
            SampleId = sampleId; Mode = mode; Location = location; JudgedAt = judgedAt;
        }
    }
}
