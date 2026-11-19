using System;
using System.Collections.Generic;

namespace CellLeakTester.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 집계(생산 386 · NG 9)는 이력 행 수 이상이다.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private const double Limit = 5.0;
        private readonly List<InspectionRecord> _history = new List<InspectionRecord>();
        private readonly Random _random = new Random(11);
        private LotInfo _lot = new LotInfo("LOT-261119-A", "CL-50A", "김민수");
        private int _total = 386, _ng = 9;

        public SampleMainViewSource()
        {
            // 최신이 앞. 시각 간격·값을 섞고, 5.0 경계값(OK)과 NG 몇 건 포함.
            var t = new DateTime(2026, 11, 19, 10, 42, 18);
            double[] leaks = { 3.2, 4.1, 5.0, 2.7, 6.4, 3.8, 1.9, 4.6, 5.8, 3.3, 2.4, 4.9, 7.1 };
            int[] gaps = { 38, 45, 41, 52, 37, 49, 44, 40, 58, 36, 47, 43, 51, 39, 46, 42 };
            for (int i = 0; i < leaks.Length; i++)
            {
                _history.Add(new InspectionRecord(t, "CELL" + (2611190412L - i * 3 - (i % 4 == 2 ? 1 : 0)),
                    leaks[i], InspectionRecord.Judge(leaks[i], Limit)));
                if (i < gaps.Length) t = t.AddSeconds(-gaps[i]);
            }
        }

        public double LeakLimit { get { return Limit; } }
        public LotInfo GetLot() { return _lot; }
        public void SetLot(LotInfo lot) { _lot = lot; }
        public DailyCount GetToday() { return new DailyCount(_total, _ng); }
        public IList<InspectionRecord> GetHistory() { return _history; }

        public InspectionRecord Inspect(string barcode)
        {
            double leak = Math.Round(1.5 + _random.NextDouble() * 5.5, 1);
            var r = new InspectionRecord(DateTime.Now, barcode, leak, InspectionRecord.Judge(leak, Limit));
            _history.Insert(0, r);
            _total++;
            if (!r.Ok) _ng++;
            return r;
        }
    }
}
