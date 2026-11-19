using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    /// <summary>측정값 판정 기준과 판정 함수 하나. 타일·카드·알람 이력·레시피가 모두 이 기준을 쓴다. 경계값은 그 상태에 넣는다(>=).</summary>
    internal static class Judging
    {
        public const double MoldTempSet = 60.0, MoldTempWarn = 63.0, MoldTempAlarm = 65.0;   // ℃
        public const double PressureWarn = 140.0, PressureAlarm = 150.0;                     // bar
        public const double CycleWarn = 32.0, CycleAlarm = 35.0;                             // s
        public const int CycleTarget = 30;                                                   // s (시간 설정은 정수)

        public static StatusKind Judge(double value, double warnAt, double alarmAt)
        {
            if (value >= alarmAt) return StatusKind.Alarm;
            if (value >= warnAt) return StatusKind.Warn;
            return StatusKind.Run;
        }

        public static StatusKind MoldTemp(double v) { return Judge(v, MoldTempWarn, MoldTempAlarm); }
        public static StatusKind Pressure(double v) { return Judge(v, PressureWarn, PressureAlarm); }
        public static StatusKind Cycle(double v) { return Judge(v, CycleWarn, CycleAlarm); }

        public static StatusKind Worst(StatusKind a, StatusKind b)
        {
            return Rank(b) > Rank(a) ? b : a;
        }

        private static int Rank(StatusKind k) { return k == StatusKind.Alarm ? 2 : k == StatusKind.Warn ? 1 : 0; }
    }
}
