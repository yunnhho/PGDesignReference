using System.Globalization;
using APC.UI.Theming;

namespace ModuleLeakTester.Data
{
    /// <summary>
    /// 판정 함수 한 곳. 메인 카드·이력 표·알람 문구가 모두 이것을 쓴다.
    /// 상·하한은 허용 한계: 상한과 같은 값은 OK, 초과부터 NG(하한은 미만부터). 경고 구간은 없다.
    /// </summary>
    internal static class LeakJudge
    {
        // 요구서에 없어 정한 기준값(사용자 확인 필요): 아래 샘플 모델의 목표·허용은 요구서(200.0 ± 5.0, 상한 4.5)를 따르고
        // 나머지 모델 값은 예시다. 모델이 바뀌면 기준은 LeakModel 값을 따른다.
        public const double DefaultPressure = 200.0, DefaultTolerance = 5.0, DefaultLeakLimit = 4.5;

        public static string Fmt(double v)
        {
            return v.ToString("0.0", CultureInfo.InvariantCulture);
        }

        public static StatusKind PressureStatus(LeakModel m, double kPa)
        {
            return kPa > m.PressureMax || kPa < m.PressureMin ? StatusKind.Alarm : StatusKind.Run;
        }

        public static StatusKind LeakStatus(LeakModel m, double paPerSec)
        {
            return paPerSec > m.LeakLimit ? StatusKind.Alarm : StatusKind.Run;
        }

        /// <summary>NG 사유. OK면 null. 압력 이탈을 누설량보다 앞에 둔다(화면 항목 순서).</summary>
        public static string NgReason(LeakModel m, double kPa, double paPerSec)
        {
            if (kPa > m.PressureMax) return "압력 상한 초과 (" + Fmt(kPa) + " > " + Fmt(m.PressureMax) + ")";
            if (kPa < m.PressureMin) return "압력 하한 미만 (" + Fmt(kPa) + " < " + Fmt(m.PressureMin) + ")";
            if (paPerSec > m.LeakLimit) return "누설량 상한 초과 (" + Fmt(paPerSec) + " > " + Fmt(m.LeakLimit) + ")";
            return null;
        }

        public static string PressureCriterion(LeakModel m)
        {
            return "기준 " + Fmt(m.Pressure) + " ± " + Fmt(m.Tolerance) + " kPa (" + Fmt(m.PressureMin) + " ~ " + Fmt(m.PressureMax) + ")";
        }

        public static string LeakCriterion(LeakModel m)
        {
            return "기준 " + Fmt(m.LeakLimit) + " 이하 OK";
        }
    }
}
