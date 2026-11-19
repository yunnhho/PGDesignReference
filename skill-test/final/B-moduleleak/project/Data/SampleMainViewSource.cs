using System;
using System.Collections.Generic;

namespace ModuleLeakTester.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 금일 집계 OK 405 + NG 7 = 412 (이력 표는 그 중 최근 건).</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private readonly Random _random = new Random(11);

        // 이력 샘플: 압력·누설량·간격을 서로 다른 주기로. 205.0 / 195.0 / 4.5 는 경계값(OK), 나머지 NG 4건.
        private const int HistoryRows = 17;   // render가 알려 준 표 높이만큼
        private static readonly double[] Pressures = { 200.2, 199.6, 205.0, 200.8, 198.9, 200.1, 206.1, 199.3, 201.4, 195.0, 200.4, 199.8, 200.9, 193.9, 200.0, 201.7, 199.1, 200.6, 203.2, 200.3 };
        private static readonly double[] Leaks = { 2.1, 3.4, 1.8, 4.5, 2.9, 3.8, 2.6, 4.8, 3.1, 2.4, 3.9, 1.5, 3.3, 2.8, 5.3, 2.2, 3.6, 4.1, 2.7, 3.0 };
        private static readonly int[] Gaps = { 47, 63, 38, 71, 52, 44, 66, 58, 41, 75, 49, 61, 36, 69, 54, 43, 64, 57, 46, 72 };

        private readonly List<LeakModel> _models = new List<LeakModel>
        {
            new LeakModel("MD-12S2P-A", LeakJudge.DefaultPressure, LeakJudge.DefaultTolerance, LeakJudge.DefaultLeakLimit),
            new LeakModel("MD-12S2P-B", 200.0, 5.0, 4.0),
            new LeakModel("MD-8S3P-A", 180.0, 4.0, 3.5),
            new LeakModel("MD-14S1P-C", 220.0, 6.0, 5.0),
            new LeakModel("MD-6S4P-A", 150.0, 3.0, 3.0),
        };

        private int _ok = 405, _ng = 7;

        public int TodayOk { get { return _ok; } }
        public int TodayNg { get { return _ng; } }

        public IList<LeakModel> GetModels() { return _models; }

        public IList<HistoryRow> GetHistory(LeakModel model)
        {
            var rows = new List<HistoryRow>();
            var at = new DateTime(2026, 11, 19, 10, 42, 18);
            for (int i = 0; i < HistoryRows; i++)
            {
                rows.Add(new HistoryRow(at, "MD2611190" + (412 - i).ToString("000"), _models[0], Pressures[i], Leaks[i]));
                at = at.AddSeconds(-Gaps[i]);
            }
            return rows;
        }

        public HistoryRow Test(string barcode, LeakModel model)
        {
            double p = Math.Round(model.Pressure - 7 + _random.NextDouble() * 14, 1);
            double l = Math.Round(1.5 + _random.NextDouble() * (model.LeakLimit - 0.5), 1);
            var row = new HistoryRow(DateTime.Now, barcode, model, p, l);
            if (row.NgReason == null) _ok++; else _ng++;
            return row;
        }

        // 샘플 전용. 실제 구현은 설정 저장소의 해시와 비교한다.
        public bool VerifyAdminPassword(string password) { return password == "1234"; }
    }
}
