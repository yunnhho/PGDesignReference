using System.Globalization;
using APC.UI.Theming;

namespace FATester.Data
{
    /// <summary>
    /// 채널 판정 한 곳. 카드·타일·헤더가 모두 이 함수를 쓴다.
    /// 기준값은 요구서에 없어 정한 값이다(고객 확인 후 여기만 고친다).
    /// </summary>
    internal static class ChannelJudge
    {
        public const double ResistanceUpper = 50.0;       // mΩ, 이 값 초과부터 알람
        public const double ResistanceWarnStart = 45.0;   // mΩ, 이 값 이상부터 경고
        public const double LeakageUpper = 100.0;         // nA, 이 값 초과부터 알람
        public const double LeakageWarnStart = 90.0;      // nA, 이 값 이상부터 경고

        public const string ResistanceCriteria = "기준 50.0 이하 · 경고 45.0 이상";
        public const string LeakageCriteria = "기준 100.0 이하 · 경고 90.0 이상";

        public static StatusKind Judge(double value, double warnStart, double upper)
        {
            if (value > upper) return StatusKind.Alarm;
            if (value >= warnStart) return StatusKind.Warn;
            return StatusKind.Run;
        }

        public static StatusKind Resistance(double? v)
        {
            return v.HasValue ? Judge(v.Value, ResistanceWarnStart, ResistanceUpper) : StatusKind.Stop;
        }

        public static StatusKind Leakage(double? v)
        {
            return v.HasValue ? Judge(v.Value, LeakageWarnStart, LeakageUpper) : StatusKind.Stop;
        }

        /// <summary>타일·표 행 상태 = 공정 상태와 값 판정 중 나쁜 쪽.</summary>
        public static StatusKind Kind(ChannelInfo c)
        {
            if (c.Process == ChannelProcess.Error) return StatusKind.Alarm;
            if (c.Process == ChannelProcess.Waiting && !c.ResistanceMOhm.HasValue && !c.LeakageNA.HasValue) return StatusKind.Stop;
            return Worse(Resistance(c.ResistanceMOhm), Leakage(c.LeakageNA));
        }

        public static string ProcessText(ChannelProcess p)
        {
            switch (p)
            {
                case ChannelProcess.Testing: return "시험중";
                case ChannelProcess.Done: return "완료";
                case ChannelProcess.Error: return "오류";
                default: return "대기";
            }
        }

        /// <summary>"공정 상태 · 사유". 사유는 가장 나쁜 것 하나(같으면 저항 → 누설전류).</summary>
        public static string Text(ChannelInfo c)
        {
            string head = ProcessText(c.Process);
            if (c.Process == ChannelProcess.Error) return head + " · " + c.ErrorCode + " " + c.ErrorText;
            if (c.Process == ChannelProcess.Waiting) return head;
            var r = Resistance(c.ResistanceMOhm);
            var l = Leakage(c.LeakageNA);
            if (r == StatusKind.Alarm) return head + " · 저항 상한 초과";
            if (l == StatusKind.Alarm) return head + " · 누설전류 상한 초과";
            if (r == StatusKind.Warn) return head + " · 저항 한계 근접";
            if (l == StatusKind.Warn) return head + " · 누설전류 한계 근접";
            return head + " · 정상";
        }

        public static string Fmt(double v) { return v.ToString("0.0", CultureInfo.InvariantCulture); }

        private static StatusKind Worse(StatusKind a, StatusKind b)
        {
            return Rank(a) >= Rank(b) ? a : b;
        }

        private static int Rank(StatusKind k)
        {
            switch (k)
            {
                case StatusKind.Alarm: return 3;
                case StatusKind.Warn: return 2;
                case StatusKind.Run: return 1;
                default: return 0;
            }
        }
    }
}
