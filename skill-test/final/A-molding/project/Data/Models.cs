using System;
using APC.UI.Theming;

namespace MoldingLineMonitor.Data
{
    /// <summary>사출기 한 대의 현재 상태(읽기 전용). 값이 없으면(통신 전·끊김) null.</summary>
    internal sealed class MachineRow
    {
        public const string Running = "가동", Stopped = "정지", Offline = "통신 끊김";

        public MachineRow(string name, string process, string note, double? mold, double? pressure, double? clamp, double? cycle)
        {
            Name = name; Process = process; Note = note;
            Mold = mold; Pressure = pressure; Clamp = clamp; Cycle = cycle;
        }
        public string Name { get; private set; }
        public string Process { get; private set; }   // 가동 / 정지 / 통신 끊김
        public string Note { get; private set; }      // 정지 사유(예: 계획 정지)
        public double? Mold { get; private set; }
        public double? Pressure { get; private set; }
        public double? Clamp { get; private set; }
        public double? Cycle { get; private set; }
    }

    /// <summary>타일·표 행의 상태 = 공정 상태와 값 판정 중 나쁜 쪽.</summary>
    internal sealed class MachineState
    {
        public MachineState(StatusKind status, string text) { Status = status; Text = text; }
        public StatusKind Status { get; private set; }
        public string Text { get; private set; }

        public static MachineState Evaluate(MachineRow m)
        {
            if (m.Process == MachineRow.Offline) return new MachineState(StatusKind.Stop, MachineRow.Offline);
            if (m.Process == MachineRow.Stopped) return new MachineState(StatusKind.Stop, MachineRow.Stopped + (m.Note == null ? "" : " · " + m.Note));
            var worst = new MachineState(StatusKind.Run, "가동 · 정상");
            Consider(ref worst, Criterion.Mold, m.Mold);
            Consider(ref worst, Criterion.Pressure, m.Pressure);
            Consider(ref worst, Criterion.Clamp, m.Clamp);
            Consider(ref worst, Criterion.Cycle, m.Cycle);
            return worst;
        }

        // 더 나쁜 것만 바꾼다(같으면 앞쪽 항목 유지).
        private static void Consider(ref MachineState worst, Criterion c, double? v)
        {
            if (v == null) return;
            var d = c.Judge(v.Value);
            if (Rank(d.Status) > Rank(worst.Status))
                worst = new MachineState(d.Status, "가동 · " + c.Name + " " + d.Reason);
        }

        private static int Rank(StatusKind k) { return k == StatusKind.Alarm ? 2 : k == StatusKind.Warn ? 1 : 0; }
    }

    /// <summary>알람 이력 한 줄(읽기 전용).</summary>
    internal sealed class AlarmRow
    {
        public AlarmRow(StatusKind status, string time, string machine, string text)
        {
            Status = status; Time = time; Machine = machine; Text = text;
        }
        public StatusKind Status { get; private set; }
        public string Time { get; private set; }
        public string Machine { get; private set; }
        public string Text { get; private set; }
    }
}
