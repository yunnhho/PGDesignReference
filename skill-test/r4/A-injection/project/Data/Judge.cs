using System.Globalization;
using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    /// <summary>
    /// 판정 기준(요구서에 없어 정한 값)과 판정 함수. 타일·카드·알람 이력·레시피 dialog 기본값이 모두 이 한 곳을 쓴다.
    /// 상한과 같은 값은 알람이 아니다(초과부터). 경고는 시작값과 같은 값부터(>=, 하한은 <=).
    /// </summary>
    internal static class Judge
    {
        public const double TempSetpoint = 60.0;       // ℃ 금형 온도 설정값
        public const double TempUpper = 65.0;          // ℃ 상한
        public const double TempUpperWarn = 63.0;      // ℃ 상한 근접 시작
        public const double TempLower = 55.0;          // ℃ 하한
        public const double TempLowerWarn = 56.0;      // ℃ 하한 근접 시작
        public const double PressureUpper = 120.0;     // bar 사출 압력 상한
        public const double PressureUpperWarn = 114.0; // bar 상한 근접 시작
        public const double CycleTarget = 25;          // s 사이클 타임 목표(시간 설정 = 정수)
        public const double CycleUpper = 28;           // s 상한
        public const double CycleUpperWarn = 27;       // s 상한 근접 시작

        public static string F1(double v) { return v.ToString("0.0", CultureInfo.InvariantCulture); }
        public static string F0(double v) { return v.ToString("0", CultureInfo.InvariantCulture); }

        public static StatusKind Temp(double v, out string reason)
        {
            reason = null;
            if (v > TempUpper) { reason = "금형 온도 상한 초과"; return StatusKind.Alarm; }
            if (v < TempLower) { reason = "금형 온도 하한 미만"; return StatusKind.Alarm; }
            if (v >= TempUpperWarn) { reason = "금형 온도 상한 근접"; return StatusKind.Warn; }
            if (v <= TempLowerWarn) { reason = "금형 온도 하한 근접"; return StatusKind.Warn; }
            return StatusKind.Run;
        }

        public static StatusKind Pressure(double v, out string reason)
        {
            reason = null;
            if (v > PressureUpper) { reason = "사출 압력 상한 초과"; return StatusKind.Alarm; }
            if (v >= PressureUpperWarn) { reason = "사출 압력 상한 근접"; return StatusKind.Warn; }
            return StatusKind.Run;
        }

        public static StatusKind Cycle(double v, out string reason)
        {
            reason = null;
            if (v > CycleUpper) { reason = "사이클 타임 상한 초과"; return StatusKind.Alarm; }
            if (v >= CycleUpperWarn) { reason = "사이클 타임 상한 근접"; return StatusKind.Warn; }
            return StatusKind.Run;
        }

        /// <summary>설비 상태 = 공정 상태와 값 판정 중 나쁜 쪽. 문구는 "공정 상태 · 사유".</summary>
        public static StatusKind Machine(MachineInfo m, out string text)
        {
            StatusKind worst = StatusKind.Run;
            string worstReason = null;
            Consider(m.Temp.HasValue, m.Temp ?? 0, Temp, ref worst, ref worstReason);
            Consider(m.Pressure.HasValue, m.Pressure ?? 0, Pressure, ref worst, ref worstReason);
            Consider(m.Cycle.HasValue, m.Cycle ?? 0, Cycle, ref worst, ref worstReason);
            if (worst == StatusKind.Alarm) { text = "알람 · " + worstReason; return worst; }
            if (worst == StatusKind.Warn) { text = "경고 · " + worstReason; return worst; }
            if (m.Process == "가동") { text = "가동 · 정상"; return StatusKind.Run; }
            text = m.Process == "정지" ? "정지 · 계획 정지" : "대기";
            return StatusKind.Stop;
        }

        private delegate StatusKind Rule(double v, out string reason);

        private static void Consider(bool has, double v, Rule rule, ref StatusKind worst, ref string worstReason)
        {
            if (!has) return;
            string reason;
            StatusKind k = rule(v, out reason);
            if (k == StatusKind.Alarm && worst != StatusKind.Alarm) { worst = k; worstReason = reason; }
            else if (k == StatusKind.Warn && worst == StatusKind.Run) { worst = k; worstReason = reason; }
        }
    }
}
