using System;

namespace ModuleLeakTester.Data
{
    /// <summary>검사 이력 한 건. 판정은 그 검사 때의 모델 기준(LeakJudge)으로 한다. 값 객체.</summary>
    internal sealed class HistoryRow
    {
        public HistoryRow(DateTime time, string barcode, LeakModel model, double pressure, double leak)
        {
            Time = time;
            Barcode = barcode;
            Model = model;
            Pressure = pressure;
            Leak = leak;
        }

        public DateTime Time { get; }
        public string Barcode { get; }
        public LeakModel Model { get; }
        public double Pressure { get; }
        public double Leak { get; }

        /// <summary>NG 사유. OK면 null.</summary>
        public string NgReason { get { return LeakJudge.NgReason(Model, Pressure, Leak); } }
    }
}
