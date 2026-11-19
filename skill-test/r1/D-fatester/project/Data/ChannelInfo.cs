using APC.UI.Theming;

namespace FATester.Data
{
    /// <summary>시험 채널 한 개의 현재 상태. 값이 없으면(대기) null / 빈 문자열.</summary>
    internal sealed class ChannelInfo
    {
        public readonly string Name;
        public readonly string StateText;     // 시험중 / 완료 / 오류 / 대기
        public readonly StatusKind BaseStatus;
        public readonly string SampleId;
        public readonly string Item;          // 열충격 / 고온고습 / 전기특성
        public readonly int? ProgressPercent;
        public readonly double? ResistanceMohm;
        public readonly double? LeakageNa;
        public readonly double ChamberTemp;   // ℃
        public readonly double Humidity;      // %RH
        public readonly int Cycles;
        public readonly string LastError;     // 없으면 "없음"
        public readonly string LastErrorText;

        public ChannelInfo(string name, string stateText, StatusKind baseStatus, string sampleId, string item,
            int? progressPercent, double? resistanceMohm, double? leakageNa,
            double chamberTemp, double humidity, int cycles, string lastError, string lastErrorText)
        {
            Name = name; StateText = stateText; BaseStatus = baseStatus; SampleId = sampleId; Item = item;
            ProgressPercent = progressPercent; ResistanceMohm = resistanceMohm; LeakageNa = leakageNa;
            ChamberTemp = chamberTemp; Humidity = humidity; Cycles = cycles; LastError = lastError; LastErrorText = lastErrorText;
        }

        public StatusKind ResistanceStatus { get { return Limits.Judge(ResistanceMohm, Limits.ResistanceMohm); } }
        public StatusKind LeakageStatus { get { return Limits.Judge(LeakageNa, Limits.LeakageNa); } }

        /// <summary>타일·헤더 요약용: 채널 상태와 측정값 판정 중 나쁜 쪽.</summary>
        public StatusKind OverallStatus
        {
            get { return Limits.Worse(BaseStatus, Limits.Worse(ResistanceStatus, LeakageStatus)); }
        }

        /// <summary>타일 둘째 줄: 측정값 이상 사유, 없으면 시험 항목.</summary>
        public string Detail
        {
            get
            {
                string r = Reason("저항", ResistanceStatus);
                if (r.Length > 0) return r;
                r = Reason("누설전류", LeakageStatus);
                return r.Length > 0 ? r : Item;
            }
        }

        public string OverallText { get { return Detail.Length == 0 ? StateText : StateText + " · " + Detail; } }

        private static string Reason(string name, StatusKind k)
        {
            if (k == StatusKind.Alarm) return name + " 상한 도달";
            if (k == StatusKind.Warn) return name + " 한계 근접";
            return "";
        }
    }

    /// <summary>불량 분류 결과 한 행.</summary>
    internal sealed class DefectRow
    {
        public readonly string SampleId, Mode, Position, JudgedAt;
        public readonly StatusKind Status;

        public DefectRow(StatusKind status, string sampleId, string mode, string position, string judgedAt)
        {
            Status = status; SampleId = sampleId; Mode = mode; Position = position; JudgedAt = judgedAt;
        }
    }

    /// <summary>측정 기준과 판정 함수 하나(타일·카드·dialog 공통). 상한과 같은 값은 알람, 상한의 90% 이상은 경고.</summary>
    internal static class Limits
    {
        public const double ResistanceMohm = 50.0;
        public const double LeakageNa = 100.0;
        private const double WarnRatio = 0.9;

        public static StatusKind Judge(double? value, double limit)
        {
            if (!value.HasValue) return StatusKind.Stop;
            if (value.Value >= limit) return StatusKind.Alarm;
            if (value.Value >= limit * WarnRatio) return StatusKind.Warn;
            return StatusKind.Run;
        }

        public static StatusKind Worse(StatusKind a, StatusKind b) { return Rank(a) >= Rank(b) ? a : b; }

        private static int Rank(StatusKind k)
        {
            switch (k)
            {
                case StatusKind.Alarm: return 3;
                case StatusKind.Warn: return 2;
                case StatusKind.Stop: return 1;
                default: return 0;
            }
        }
    }
}
