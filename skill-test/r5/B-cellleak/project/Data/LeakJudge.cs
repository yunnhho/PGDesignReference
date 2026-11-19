using System.Globalization;

namespace CellLeakTester.Data
{
    /// <summary>누설 판정 기준. 화면·이력·문구가 모두 이 한 곳을 쓴다.</summary>
    internal static class LeakJudge
    {
        // 정한 기준값(요구서 상한 5.0 Pa/s). 하한·경고 구간 없음.
        public const double UpperLimit = 5.0;

        /// <summary>상한과 같으면 OK, 초과부터 NG.</summary>
        public static bool IsNg(double leak) { return leak > UpperLimit; }
        public static string Format(double leak) { return leak.ToString("0.0", CultureInfo.InvariantCulture); }
        public static string LimitText { get { return Format(UpperLimit); } }
    }
}
