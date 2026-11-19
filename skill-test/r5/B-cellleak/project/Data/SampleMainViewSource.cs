using System;
using System.Collections.Generic;

namespace CellLeakTester.Data
{
    /// <summary>화면 확인·render용 샘플 데이터.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private static readonly double[] Leaks = { 4.3, 3.8, 5.0, 4.6, 6.1, 2.9, 3.4, 4.1, 4.9, 3.1, 5.4, 3.7, 2.6, 4.4, 3.3, 7.8, 4.0, 3.6, 4.8 };
        private static readonly int[] GapSeconds = { 37, 52, 44, 61, 39, 48, 57, 43, 66, 41, 50, 38, 59, 46, 54, 42, 63, 47, 36, 55, 49 };
        private LotInfo _lot = new LotInfo("LOT-261119-03", "NCM-21700", "김민수");

        public LotInfo GetLot() { return _lot; }
        public void SaveLot(LotInfo lot) { _lot = lot; }

        public IList<InspectionRecord> GetHistory()
        {
            var list = new List<InspectionRecord>();
            var at = new DateTime(2026, 11, 19, 10, 15, 0);
            for (int i = 0; i < Leaks.Length; i++)
            {
                list.Add(new InspectionRecord(at, "LK26111" + (90412 - i * 3 - i % 2), Leaks[i]));
                if (i < GapSeconds.Length) at = at.AddSeconds(-GapSeconds[i]);
            }
            return list;
        }

        // 오늘 생산 412 중 NG 9(이력의 NG 3건 포함). 수율 = OK / 생산.
        public int GetProducedCount() { return 412; }
        public int GetNgCount() { return 9; }
        public bool VerifyAdminPassword(string password) { return !string.IsNullOrEmpty(password); }   // 샘플: 실제는 저장된 해시와 비교
    }
}
