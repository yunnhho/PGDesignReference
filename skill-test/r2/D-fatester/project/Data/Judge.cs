using APC.UI.Theming;

namespace FATester.Data
{
    /// <summary>측정값 기준(요구서에 없어 가정한 값). 상한과 같은 값은 정상, 초과부터 알람, 경고 시작값과 같으면 경고.</summary>
    internal static class Limits
    {
        public const double ResistanceUpper = 50.0;   // mΩ
        public const double ResistanceWarn = 45.0;
        public const double LeakageUpper = 100.0;     // nA
        public const double LeakageWarn = 90.0;
        public const double TempMargin = 2.0;         // 챔버 온도 경고 = 상한 - 2.0 ℃
        public const double HumiMargin = 5.0;         // 챔버 습도 경고 = 상한 - 5.0 %RH
    }

    /// <summary>값 판정 함수 하나. 카드·타일·dialog 표가 모두 이것을 쓴다.</summary>
    internal static class Judge
    {
        public static StatusKind Of(double? value, double warnStart, double upper)
        {
            if (value == null) return StatusKind.Stop;       // 값 없음은 판정하지 않는다
            if (value.Value > upper) return StatusKind.Alarm;
            if (value.Value >= warnStart) return StatusKind.Warn;
            return StatusKind.Run;
        }

        public static string F1(double v) { return v.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture); }
        public static string F1(double? v) { return v == null ? "" : F1(v.Value); }
    }
}
