using System;
using System.Collections.Generic;

namespace CellLeakTester.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 집계(412 / NG 9)는 이력 행 수 이상이다.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        // 최신이 먼저. 5.0은 상한과 같은 값(OK). 5.3·6.8·5.1은 NG.
        private static readonly double[] Leaks = { 4.2, 3.1, 5.0, 2.7, 5.3, 3.8, 4.6, 1.9, 6.8, 4.0, 3.3, 4.9, 2.2, 5.1, 3.6, 4.4, 2.9, 3.9 };
        private static readonly int[] GapSeconds = { 0, 38, 45, 41, 52, 36, 47, 43, 58, 40, 44, 49, 37, 55, 42, 46, 39, 51 };
        private const int TotalToday = 412, NgToday = 9;

        private LotInfo _lot = new LotInfo("LOT-261119-A03", "PE-60Ah-A", "김민수");
        private readonly List<InspectionRecord> _history = new List<InspectionRecord>();
        private int _total = TotalToday, _ng = NgToday;

        public SampleMainViewSource()
        {
            var at = new DateTime(2026, 11, 19, 10, 15, 0);
            for (int i = 0; i < Leaks.Length; i++)
            {
                at = at.AddSeconds(-GapSeconds[i]);
                _history.Add(new InspectionRecord(at, Barcode(TotalToday - i), Leaks[i]));
            }
        }

        public LotInfo GetLot() { return _lot; }
        public void SaveLot(LotInfo lot) { _lot = lot; }
        public DailyCount GetDaily() { return new DailyCount(_total, _ng); }
        public IList<InspectionRecord> GetHistory() { return _history; }

        public InspectionRecord Inspect(string barcode)
        {
            // 샘플: 바코드 글자 합으로 0.0~7.0 사이 값을 만든다. 실제는 시험기 측정값.
            int sum = 0;
            foreach (char c in barcode) sum += c;
            var r = new InspectionRecord(DateTime.Now, barcode, (sum % 71) / 10.0);
            _total++;
            if (!r.Ok) _ng++;
            _history.Insert(0, r);
            return r;
        }

        public bool CheckAdminPassword(string password) { return password == "1234"; }   // 샘플 전용

        private static string Barcode(int seq) { return "CL-261119-" + seq.ToString("0000"); }
    }
}
