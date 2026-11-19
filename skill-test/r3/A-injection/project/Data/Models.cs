using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    internal enum ProcessState { Running, Standby, Stopped }

    /// <summary>사출기 한 대. 값이 없으면(대기·통신 전) null.</summary>
    internal sealed class MachineInfo
    {
        public readonly string Id;
        public readonly ProcessState State;
        public readonly double? Temp;
        public readonly double? Pressure;
        public readonly double? Cycle;
        public readonly string Note;

        public MachineInfo(string id, ProcessState state, double? temp, double? pressure, double? cycle, string note)
        {
            Id = id; State = state; Temp = temp; Pressure = pressure; Cycle = cycle; Note = note;
        }

        /// <summary>타일·표 상태 = 공정 상태와 값 판정 중 나쁜 쪽(같으면 온도 → 압력 → 사이클 순).</summary>
        public Verdict Overall()
        {
            var worst = new Verdict(StatusKind.Run, "", "");
            if (Temp.HasValue) worst = Worse(worst, Judgement.Temp(Temp.Value));
            if (Pressure.HasValue) worst = Worse(worst, Judgement.Pressure(Pressure.Value));
            if (Cycle.HasValue) worst = Worse(worst, Judgement.Cycle(Cycle.Value));
            if (worst.Status == StatusKind.Run && State != ProcessState.Running)
                worst = new Verdict(StatusKind.Stop, Note, Note);
            return worst;
        }

        public string StatusText()
        {
            string label = State == ProcessState.Running ? "가동" : State == ProcessState.Standby ? "대기" : "정지";
            var v = Overall();
            if (v.Reason.Length > 0) return label + " · " + v.Reason;
            return State == ProcessState.Running ? label + " · 정상" : label;
        }

        private static Verdict Worse(Verdict a, Verdict b)
        {
            return Rank(b.Status) > Rank(a.Status) ? b : a;
        }

        private static int Rank(StatusKind k)
        {
            return k == StatusKind.Alarm ? 2 : k == StatusKind.Warn ? 1 : 0;
        }
    }

    internal sealed class AlarmEntry
    {
        public readonly StatusKind Kind;
        public readonly string Time;
        public readonly string Machine;
        public readonly string Text;

        public AlarmEntry(StatusKind kind, string time, string machine, string text)
        {
            Kind = kind; Time = time; Machine = machine; Text = text;
        }
    }

    internal sealed class Recipe
    {
        public readonly decimal MoldTempSetpoint;
        public readonly decimal PressureUpper;
        public readonly decimal CycleTarget;

        public Recipe(decimal moldTempSetpoint, decimal pressureUpper, decimal cycleTarget)
        {
            MoldTempSetpoint = moldTempSetpoint; PressureUpper = pressureUpper; CycleTarget = cycleTarget;
        }
    }
}
