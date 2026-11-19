using APC.UI.Theming;

namespace FATester.Data
{
    /// <summary>
    /// 측정값 판정 기준과 판정 함수(화면·알람·상세 모두 여기 하나).
    /// 기준값은 요구서에 없어 정한 값이다 — 현장 기준으로 고친다.
    /// </summary>
    internal static class ChannelRules
    {
        public const double ResistanceUpper = 100.0;   // mΩ, 초과부터 알람
        public const double ResistanceWarn = 90.0;     // mΩ, 이상부터 경고
        public const double LeakUpper = 50.0;          // nA, 초과부터 알람
        public const double LeakWarn = 45.0;           // nA, 이상부터 경고

        public static StatusKind Judge(double value, double warn, double upper)
        {
            if (value > upper) return StatusKind.Alarm;
            if (value >= warn) return StatusKind.Warn;
            return StatusKind.Run;
        }

        public static StatusKind JudgeResistance(double v) { return Judge(v, ResistanceWarn, ResistanceUpper); }
        public static StatusKind JudgeLeak(double v) { return Judge(v, LeakWarn, LeakUpper); }

        private static int Rank(StatusKind k)
        {
            switch (k)
            {
                case StatusKind.Alarm: return 4;
                case StatusKind.Warn: return 3;
                case StatusKind.Stop: return 2;
                default: return 1;
            }
        }

        private static string Reason(string name, string unit, double v, StatusKind kind, double upper)
        {
            if (kind == StatusKind.Alarm) return name + " 상한 초과 " + v.ToString("0.0") + " " + unit + " (상한 " + upper.ToString("0.0") + ")";
            if (kind == StatusKind.Warn) return name + " 한계 근접 " + v.ToString("0.0") + " " + unit + " (상한 " + upper.ToString("0.0") + ")";
            return null;
        }

        /// <summary>채널 상태 = 공정 상태와 값 판정 중 나쁜 쪽, 문구는 "공정 상태 · 사유". 사유가 같은 등급이면 항목 순서(저항, 누설) 앞쪽.</summary>
        public static void Summarize(ChannelRow c, out StatusKind status, out string text)
        {
            status = c.ProcessStatus;
            text = c.ProcessText;
            if (c.ProcessStatus == StatusKind.Alarm && !string.IsNullOrEmpty(c.ErrorDetail))
            {
                text = c.ProcessText + " · " + c.ErrorDetail;
            }

            StatusKind rk = c.Resistance.HasValue ? JudgeResistance(c.Resistance.Value) : StatusKind.Run;
            StatusKind lk = c.Leak.HasValue ? JudgeLeak(c.Leak.Value) : StatusKind.Run;
            string reason = null;
            StatusKind worst = StatusKind.Run;
            if (Rank(rk) > 1) { worst = rk; reason = Reason("저항", "mΩ", c.Resistance.Value, rk, ResistanceUpper); }
            if (Rank(lk) > Rank(worst)) { worst = lk; reason = Reason("누설전류", "nA", c.Leak.Value, lk, LeakUpper); }

            if (reason != null && Rank(worst) > Rank(status))
            {
                status = worst;
                text = c.ProcessText + " · " + reason;
            }
        }
    }
}
