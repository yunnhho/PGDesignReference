using System.Collections.Generic;

namespace CellLeakTester.Data
{
    /// <summary>화면 확인·render용 샘플 데이터.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private static readonly double[] Leaks = { 3.2, 4.1, 5.0, 2.8, 6.3, 3.7, 1.9, 4.6, 5.4, 3.0, 4.4, 2.2, 3.9 };
        private static readonly int[] Gaps = { 0, 38, 41, 36, 47, 39, 42, 35, 44, 40, 37, 46, 38 };
        private readonly List<LeakResult> _history = new List<LeakResult>();

        public SampleMainViewSource()
        {
            int sec = 10 * 3600 + 24 * 60 + 18;
            for (int i = 0; i < Leaks.Length; i++)
            {
                sec -= Gaps[i];
                _history.Add(new LeakResult("CELL26111" + (4880 - i * 3 - i % 2), string.Format("{0:00}:{1:00}:{2:00}", sec / 3600, sec / 60 % 60, sec % 60), Leaks[i]));
            }
        }

        public string CurrentLot { get { return "LOT-20261119-03"; } }
        public string CurrentModel { get { return "NCM-21700"; } }
        public string CurrentOperator { get { return "김현장"; } }
        public int TodayTotal { get { return 248; } }
        public int TodayNg { get { return 9; } }
        public IList<LeakResult> History { get { return _history; } }
        public bool CheckAdminPassword(string password) { return password.Length > 0; }   // 샘플: 실제 비교는 연결 시 구현
    }
}
