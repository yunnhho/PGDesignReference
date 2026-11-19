using System.Globalization;
using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    /// <summary>판정 결과: 상태 + 사유(타일·카드용) + 값이 든 문구(알람 이력용).</summary>
    internal struct Verdict
    {
        public readonly StatusKind Status;
        public readonly string Reason;
        public readonly string Detail;

        public Verdict(StatusKind status, string reason, string detail)
        {
            Status = status; Reason = reason; Detail = detail;
        }
    }

    /// <summary>
    /// 요구서에 없어 정한 기준값(고객 확인 필요)과 판정 함수. 타일·카드·알람 이력·그래프가 모두 여기 한 곳을 쓴다.
    /// 상·하한과 같은 값은 정상 범위(초과/미만부터 알람), 경고 시작값과 같으면 경고.
    /// </summary>
    internal static class Judgement
    {
        // 금형 온도 (℃)
        public const double TempSetpoint = 180.0;
        public const double TempUpper = 190.0;
        public const double TempLower = 170.0;
        public const double TempWarnHigh = 185.0;
        public const double TempWarnLow = 175.0;

        // 사출 압력 (bar)
        public const double PressureUpper = 1200.0;
        public const double PressureWarnHigh = 1100.0;

        // 사이클 타임 (s): 목표는 설정(정수), 측정값은 0.0
        public const int CycleTarget = 30;
        public const double CycleUpper = 35.0;
        public const double CycleWarnHigh = 32.0;

        public static string F(double v)
        {
            return v.ToString("0.0", CultureInfo.InvariantCulture);
        }

        public static Verdict Temp(double v)
        {
            string val = F(v) + " ℃ (";
            if (v > TempUpper) return Alarm("금형 온도 상한 초과", val + "상한 " + F(TempUpper) + ")");
            if (v < TempLower) return Alarm("금형 온도 하한 미만", val + "하한 " + F(TempLower) + ")");
            if (v >= TempWarnHigh) return Warn("금형 온도 상한 근접", val + "상한 " + F(TempUpper) + ")");
            if (v <= TempWarnLow) return Warn("금형 온도 하한 근접", val + "하한 " + F(TempLower) + ")");
            return Normal();
        }

        public static Verdict Pressure(double v)
        {
            string val = F(v) + " bar (상한 " + F(PressureUpper) + ")";
            if (v > PressureUpper) return Alarm("사출 압력 상한 초과", val);
            if (v >= PressureWarnHigh) return Warn("사출 압력 상한 근접", val);
            return Normal();
        }

        public static Verdict Cycle(double v)
        {
            string val = F(v) + " s (상한 " + F(CycleUpper) + ")";
            if (v > CycleUpper) return Alarm("사이클 타임 초과", val);
            if (v >= CycleWarnHigh) return Warn("사이클 타임 지연", val);
            return Normal();
        }

        private static Verdict Normal() { return new Verdict(StatusKind.Run, "", ""); }
        private static Verdict Warn(string reason, string rest) { return new Verdict(StatusKind.Warn, reason, reason + " " + rest); }
        private static Verdict Alarm(string reason, string rest) { return new Verdict(StatusKind.Alarm, reason, reason + " " + rest); }
    }
}
