using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    internal enum ProcessState { Running, Standby, Stopped }

    /// <summary>사출기 한 대의 현재값(읽기 전용). 값이 없으면 null.</summary>
    internal sealed class Machine
    {
        public readonly string Name;
        public readonly ProcessState Process;
        public readonly string Note;
        public readonly decimal? Temp;
        public readonly decimal? Pressure;
        public readonly decimal? Cycle;
        public readonly double[] TempTrend;

        public Machine(string name, ProcessState process, string note, decimal? temp, decimal? pressure, decimal? cycle, double[] tempTrend)
        {
            Name = name; Process = process; Note = note; Temp = temp; Pressure = pressure; Cycle = cycle; TempTrend = tempTrend;
        }
    }

    /// <summary>알람 이력 한 줄.</summary>
    internal sealed class AlarmRow
    {
        public readonly StatusKind Status;
        public readonly string Time;
        public readonly string Machine;
        public readonly string Text;

        public AlarmRow(StatusKind status, string time, string machine, string text)
        {
            Status = status; Time = time; Machine = machine; Text = text;
        }
    }
}
