namespace ModuleLeakTester.Data
{
    /// <summary>검사 모델 한 건: 시험 압력 기준(목표 ± 허용)과 누설량 상한. 값 객체.</summary>
    internal sealed class LeakModel
    {
        public LeakModel(string name, double pressure, double tolerance, double leakLimit)
        {
            Name = name;
            Pressure = pressure;
            Tolerance = tolerance;
            LeakLimit = leakLimit;
        }

        public string Name { get; }
        /// <summary>시험 압력 기준(kPa)</summary>
        public double Pressure { get; }
        /// <summary>시험 압력 허용(± kPa)</summary>
        public double Tolerance { get; }
        /// <summary>누설량 상한(Pa/s)</summary>
        public double LeakLimit { get; }

        public double PressureMin { get { return Pressure - Tolerance; } }
        public double PressureMax { get { return Pressure + Tolerance; } }
    }
}
