using System;
using System.Collections.Generic;

namespace CellLeakTester.Data
{
    /// <summary>화면 확인·render용 샘플 데이터.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        // 값·간격을 되풀이하지 않게 섞었다. 5.0 = 상한과 같은 값(OK), 5.3·6.8·5.1 = NG.
        private static readonly decimal[] Leaks =
        {
            3.2m, 2.8m, 4.1m, 3.6m, 5.0m, 2.2m, 3.9m, 5.3m, 1.8m, 3.4m, 4.4m, 2.9m,
            6.8m, 3.1m, 4.7m, 2.5m, 3.8m, 5.1m, 2.7m, 3.3m, 4.2m, 1.9m, 3.5m, 4.0m
        };
        private static readonly int[] GapSeconds =
        {
            38, 52, 45, 41, 63, 36, 48, 57, 44, 39, 61, 47, 35, 54, 42, 50, 46, 58, 37, 53, 43, 49, 40, 55
        };
        private const int HistoryRows = 24;
        private const int TodayTotal = 412;

        private readonly List<InspectionRecord> _history = new List<InspectionRecord>();
        private string _lot = "LOT-261119-A";
        private string _model = "NCM-60Ah";
        private string _worker = "김현장";

        public SampleMainViewSource()
        {
            DateTime at = new DateTime(2026, 11, 19, 10, 15, 0);
            for (int i = 0; i < HistoryRows; i++)
            {
                _history.Add(new InspectionRecord(at, "LC26111904" + (TodayTotal - i).ToString("00#"), Leaks[i]));
                at = at.AddSeconds(-GapSeconds[i]);
            }
        }

        public InspectionRecord CurrentCell { get { return _history[0]; } }
        public string CurrentLot { get { return _lot; } }
        public string CurrentModel { get { return _model; } }
        public string CurrentWorker { get { return _worker; } }
        public int TodayNg { get { return 7; } }
        public int TodayOk { get { return TodayTotal - 7; } }
        public IList<InspectionRecord> RecentHistory { get { return _history; } }

        public void SaveLot(string lot, string model, string worker)
        {
            _lot = lot; _model = model; _worker = worker;
        }

        // 샘플: 비어 있지 않으면 통과. 실제 구현은 저장된 해시와 비교한다.
        public bool VerifyAdminPassword(string password)
        {
            return !string.IsNullOrEmpty(password);
        }
    }
}
