namespace CellLeakTester.Data
{
    /// <summary>누설 판정 기준과 판정 함수(화면 전체가 이것 하나를 쓴다). 요구서의 상한 외 값은 없다.</summary>
    internal static class LeakJudgement
    {
        /// <summary>누설량 상한 (Pa/s). 요구서 지정. 상한과 같은 값은 OK, 초과부터 NG.</summary>
        public const decimal UpperLimit = 5.0m;

        public const string Unit = "Pa/s";
        public const string LimitText = "5.0 이하 OK";

        public static bool IsOk(decimal leak)
        {
            return leak <= UpperLimit;
        }

        public static string ResultText(decimal leak)
        {
            return IsOk(leak) ? "OK" : "NG · 상한 초과";
        }

        public static string Format(decimal leak)
        {
            return leak.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
