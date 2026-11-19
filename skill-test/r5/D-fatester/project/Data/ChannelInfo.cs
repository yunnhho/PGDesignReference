using APC.UI.Theming;

namespace FATester.Data
{
    internal enum ProcessState { Running, Done, Error, Idle }

    /// <summary>시험 채널 한 개의 현재 상태(값 객체). 값이 없으면 null/빈 문자열.</summary>
    internal sealed class ChannelInfo
    {
        // ---- 정한 기준값: 한 곳에 둔다(요구서에 없어 정함) ----
        public static readonly Limit Resistance = new Limit("저항", "mΩ", "0.0", 13.5, 15.0);
        public static readonly Limit Leakage = new Limit("누설전류", "nA", "0.0", 45.0, 50.0);

        public ChannelInfo(string name, ProcessState process, string sample, string item, double? progress,
            double? resistance, double? leakage, Limit chamberTemp, double? temp, Limit chamberHumi, double? humi,
            int? cycles, string errorCode, string errorName, string errorTime, double[] tempTrend)
        {
            Name = name; Process = process; Sample = sample; Item = item; Progress = progress;
            ResistanceValue = resistance; LeakageValue = leakage;
            ChamberTemp = chamberTemp; Temp = temp; ChamberHumi = chamberHumi; Humi = humi;
            Cycles = cycles; ErrorCode = errorCode; ErrorName = errorName; ErrorTime = errorTime; TempTrend = tempTrend;
        }

        public string Name { get; private set; }
        public ProcessState Process { get; private set; }
        public string Sample { get; private set; }
        public string Item { get; private set; }
        public double? Progress { get; private set; }
        public double? ResistanceValue { get; private set; }
        public double? LeakageValue { get; private set; }
        public Limit ChamberTemp { get; private set; }
        public double? Temp { get; private set; }
        public Limit ChamberHumi { get; private set; }
        public double? Humi { get; private set; }
        public int? Cycles { get; private set; }
        public string ErrorCode { get; private set; }
        public string ErrorName { get; private set; }
        public string ErrorTime { get; private set; }
        public double[] TempTrend { get; private set; }

        public string ProcessText
        {
            get
            {
                switch (Process)
                {
                    case ProcessState.Running: return "시험중";
                    case ProcessState.Done: return "완료";
                    case ProcessState.Error: return "오류";
                    default: return "대기";
                }
            }
        }

        public StatusKind ProcessStatus
        {
            get
            {
                switch (Process)
                {
                    case ProcessState.Running:
                    case ProcessState.Done: return StatusKind.Run;
                    case ProcessState.Error: return StatusKind.Alarm;
                    default: return StatusKind.Stop;
                }
            }
        }

        /// <summary>진행률 카드 상태: 갱신 중(시험중·완료)이면 Run, 멈췄으면 Stop.</summary>
        public StatusKind ProgressStatus
        {
            get { return Process == ProcessState.Running || Process == ProcessState.Done ? StatusKind.Run : StatusKind.Stop; }
        }

        public StatusKind ResistanceStatus { get { return ResistanceValue.HasValue ? Resistance.Judge(ResistanceValue.Value) : StatusKind.Stop; } }
        public StatusKind LeakageStatus { get { return LeakageValue.HasValue ? Leakage.Judge(LeakageValue.Value) : StatusKind.Stop; } }

        private static int Rank(StatusKind k) { return k == StatusKind.Alarm ? 2 : (k == StatusKind.Warn ? 1 : 0); }

        /// <summary>타일·헤더 상태 = 공정 상태와 값 판정 중 나쁜 쪽(오류·대기는 공정 상태를 따른다).</summary>
        public StatusKind Status
        {
            get
            {
                if (Process == ProcessState.Error || Process == ProcessState.Idle) return ProcessStatus;
                return Rank(ResistanceStatus) >= Rank(LeakageStatus)
                    ? (Rank(ResistanceStatus) > 0 ? ResistanceStatus : ProcessStatus)
                    : LeakageStatus;
            }
        }

        /// <summary>"공정 상태 · 사유". 사유가 여럿이면 가장 나쁜 것 하나(같으면 저항 먼저).</summary>
        public string StatusText
        {
            get
            {
                if (Process == ProcessState.Error) return ProcessText + " · " + ErrorName + " " + ErrorCode;
                if (Process == ProcessState.Idle) return ProcessText;
                string reason = null;
                if (Rank(ResistanceStatus) > 0 && Rank(ResistanceStatus) >= Rank(LeakageStatus)) reason = Resistance.Reason(ResistanceValue.Value);
                else if (Rank(LeakageStatus) > 0) reason = Leakage.Reason(LeakageValue.Value);
                return reason == null ? ProcessText + " · 정상" : ProcessText + " · " + reason;
            }
        }
    }
}
