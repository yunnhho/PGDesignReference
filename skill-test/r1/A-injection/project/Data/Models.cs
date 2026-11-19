using System;
using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    /// <summary>사출기 한 대의 현재 상태. 값이 null이면 읽은 값 없음(대기·통신 전).</summary>
    internal sealed class MachineInfo
    {
        public readonly string Id;
        public readonly string RunState;     // 가동 / 대기 / 정지
        public readonly double? MoldTemp;    // ℃
        public readonly double? Pressure;    // bar
        public readonly double? CycleTime;   // s

        public MachineInfo(string id, string runState, double? moldTemp, double? pressure, double? cycleTime)
        {
            Id = id; RunState = runState; MoldTemp = moldTemp; Pressure = pressure; CycleTime = cycleTime;
        }

        /// <summary>대기·정지 = Stop, 가동 중이면 세 값 중 나쁜 판정.</summary>
        public StatusKind Status
        {
            get
            {
                if (RunState != "가동") return StatusKind.Stop;
                var k = StatusKind.Run;
                if (MoldTemp.HasValue) k = Judging.Worst(k, Judging.MoldTemp(MoldTemp.Value));
                if (Pressure.HasValue) k = Judging.Worst(k, Judging.Pressure(Pressure.Value));
                if (CycleTime.HasValue) k = Judging.Worst(k, Judging.Cycle(CycleTime.Value));
                return k;
            }
        }

        public string StatusText
        {
            get
            {
                var k = Status;
                if (RunState != "가동") return RunState;
                if (k == StatusKind.Run) return "가동 · 정상";
                string head = k == StatusKind.Alarm ? "알람 · " : "경고 · ";
                string tail = k == StatusKind.Alarm ? " 상한 초과" : " 상한 근접";
                if (MoldTemp.HasValue && Judging.MoldTemp(MoldTemp.Value) == k) return head + "금형 온도" + tail;
                if (Pressure.HasValue && Judging.Pressure(Pressure.Value) == k) return head + "사출 압력" + tail;
                return head + "사이클 타임" + (k == StatusKind.Alarm ? " 한계 초과" : " 지연");
            }
        }
    }

    internal sealed class AlarmRow
    {
        public readonly StatusKind Kind;
        public readonly DateTime Time;
        public readonly string MachineId;
        public readonly string Text;

        public AlarmRow(StatusKind kind, DateTime time, string machineId, string text)
        {
            Kind = kind; Time = time; MachineId = machineId; Text = text;
        }
    }

    /// <summary>레시피 편집 결과·초기값.</summary>
    internal sealed class RecipeValues
    {
        public readonly decimal MoldTempSet;      // ℃
        public readonly decimal PressureLimit;    // bar
        public readonly decimal CycleTarget;      // s

        public RecipeValues(decimal moldTempSet, decimal pressureLimit, decimal cycleTarget)
        {
            MoldTempSet = moldTempSet; PressureLimit = pressureLimit; CycleTarget = cycleTarget;
        }
    }
}
