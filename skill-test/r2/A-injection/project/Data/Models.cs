using System;
using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    /// <summary>레시피(설정값). 상·하한 판정은 여기 한 곳. 상한과 같은 값은 정상 범위, 초과부터 알람, 경고 시작값과 같은 값부터 경고.</summary>
    internal sealed class Recipe
    {
        public const double TempMargin = 5.0;      // 금형 온도 상한 = 설정값 + 5.0
        public const double TempWarnBand = 2.0;    // 상한 - 2.0 이상부터 경고
        public const double PressureWarnBand = 5.0;
        public const double CycleWarnBand = 3.0;   // 목표 + 3 이상부터 경고
        public const double CycleMargin = 5.0;     // 사이클 타임 상한 = 목표 + 5

        public readonly double SetTemp;
        public readonly double MaxPressure;
        public readonly int CycleTarget;

        public Recipe(double setTemp, double maxPressure, int cycleTarget)
        {
            SetTemp = setTemp; MaxPressure = maxPressure; CycleTarget = cycleTarget;
        }

        public double TempUpper { get { return SetTemp + TempMargin; } }
        public double TempWarn { get { return TempUpper - TempWarnBand; } }
        public double PressureWarn { get { return MaxPressure - PressureWarnBand; } }
        public double CycleUpper { get { return CycleTarget + CycleMargin; } }
        public double CycleWarn { get { return CycleTarget + CycleWarnBand; } }

        public StatusKind TempStatus(double v) { return Grade(Math.Round(v, 1), TempWarn, TempUpper); }
        public StatusKind PressureStatus(double v) { return Grade(Math.Round(v, 1), PressureWarn, MaxPressure); }
        public StatusKind CycleStatus(double v) { return Grade(Math.Round(v, 1), CycleWarn, CycleUpper); }

        private static StatusKind Grade(double v, double warnFrom, double upper)
        {
            if (v > upper) return StatusKind.Alarm;
            if (v >= warnFrom) return StatusKind.Warn;
            return StatusKind.Run;
        }
    }

    /// <summary>사출기 한 대의 현재 읽은 값. 값이 없으면 null(빈칸으로 표시).</summary>
    internal sealed class Machine
    {
        public readonly string Id;
        public readonly string Process;   // 가동 / 대기 / 정지
        public readonly string Note;      // 정지 사유
        public readonly double? Temp, Pressure, Cycle;

        public Machine(string id, string process, string note, double? temp, double? pressure, double? cycle)
        {
            Id = id; Process = process; Note = note; Temp = temp; Pressure = pressure; Cycle = cycle;
        }
    }

    internal sealed class AlarmRow
    {
        public readonly DateTime Time;
        public readonly StatusKind Status;
        public readonly string Unit, Text;

        public AlarmRow(DateTime time, StatusKind status, string unit, string text)
        {
            Time = time; Status = status; Unit = unit; Text = text;
        }
    }
}
