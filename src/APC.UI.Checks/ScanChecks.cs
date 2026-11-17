using APC.UI.Input;

namespace APC.UI.Checks
{
    internal static partial class Program
    {
        /// <summary>문자열을 한 글자씩 넣는다. gaps[i] = i번째 글자 직전 간격(ms), 없으면 10ms.</summary>
        private static string Feed(ScanDetector d, string s, params int[] gaps)
        {
            long t = 1000;
            string result = null;
            for (int i = 0; i < s.Length; i++)
            {
                t += i < gaps.Length ? gaps[i] : 10;
                result = d.Feed(s[i], t);
            }
            return result;
        }

        private static void RunScanChecks()
        {
            var d = new ScanDetector(50, 4);
            Check(Feed(d, "LOT-0032\r") == "LOT-0032", "fast burst is scan");
            d.Reset(); Check(Feed(d, "abc\r") == null, "short fast input not scan");
            // 느린 간격 뒤 남은 빠른 구간("32")이 최소 길이 미만 → 스캔 아님
            d.Reset(); Check(Feed(d, "LOT-0032\r", 10, 10, 10, 10, 10, 10, 200, 10, 10) == null, "one slow gap breaks scan");
            d.Reset(); Feed(d, "ab"); Check(d.Feed('c', 5) == null && d.Feed('\r', 10) == null, "clock going backwards is not a burst");
            d.Reset(); Check(Feed(d, "\r") == null, "empty enter ignored");
            d.Reset(); Feed(d, "abc\r"); Check(Feed(d, "LOT-0033\r") == "LOT-0033", "detector recovers after rejection");
            d.Reset(); Check(Feed(d, "LOT-0032\r", 10, 10, 10, 10, 10, 10, 10, 10, 300) == null, "slow enter after burst not scan");
            d.Reset(); Feed(d, "hello", 300, 300, 300, 300, 300); Check(Feed(d, "LOT-0034\r", 400) == "LOT-0034", "typed prefix does not leak into scan");
        }
    }
}
