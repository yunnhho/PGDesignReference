using System;
using System.Collections.Generic;
using APC.UI.Theming;

namespace MoldingLineMonitor.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 알람 문구는 Criterion.Message로 만들어 기준값과 같다.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private const int TrendPoints = 40;
        private static readonly MachineRow[] Machines =
        {
            new MachineRow("IM-01", MachineRow.Running, null, 60.2, 131.4, 452.0, 24.2),
            new MachineRow("IM-02", MachineRow.Running, null, 61.5, 150.0, 455.3, 24.8),   // 사출 압력 = 상한(150.0) → 경고
            new MachineRow("IM-03", MachineRow.Running, null, 59.8, 128.9, 448.6, 23.9),
            new MachineRow("IM-04", MachineRow.Running, null, 60.6, 133.0, 508.4, 24.5),   // 형체력 상한 초과
            new MachineRow("IM-05", MachineRow.Stopped, "계획 정지", 38.5, 0.0, 0.0, 24.4),   // 멈춘 값(판정 안 함)
            new MachineRow("IM-06", MachineRow.Running, null, 60.1, 129.7, 450.2, 28.0),   // 사이클 타임 = 상한(28.0) → 경고
            new MachineRow("IM-07", MachineRow.Running, null, 62.9, 131.0, 449.1, 24.0),
            new MachineRow("IM-08", MachineRow.Offline, null, null, null, null, null),
        };

        public IList<MachineRow> GetMachines() { return Machines; }

        public double[] GetPressureTrend(string machineName)
        {
            foreach (var m in Machines)
            {
                if (m.Name != machineName || m.Pressure == null) continue;
                var v = new double[TrendPoints];
                for (int i = 0; i < TrendPoints; i++)
                    v[i] = Math.Round(Criterion.PressureSet + 2.5 * Math.Sin(i / 3.0 + m.Name[4]) + 1.2 * Math.Cos(i * 1.3), 1);
                v[TrendPoints - 1] = m.Pressure.Value;   // 마지막 점 = 현재값
                return v;
            }
            return new double[0];
        }

        public int GetProducedToday() { return 12480; }
        public int GetDefectsToday() { return 187; }

        public IList<AlarmRow> GetAlarmHistory()
        {
            return new List<AlarmRow>
            {
                new AlarmRow(StatusKind.Alarm, "14:52:10", "IM-04", Criterion.Clamp.Message(508.4)),
                new AlarmRow(StatusKind.Warn, "14:47:31", "IM-06", Criterion.Cycle.Message(28.0)),
                new AlarmRow(StatusKind.Warn, "14:40:05", "IM-02", Criterion.Pressure.Message(150.0)),
                new AlarmRow(StatusKind.Stop, "14:12:44", "IM-05", "계획 정지"),
                new AlarmRow(StatusKind.Stop, "13:58:02", "IM-08", "통신 끊김"),
                new AlarmRow(StatusKind.Run, "13:52:09", "IM-01", "금형 온도 정상 복귀 60.4 ℃"),
                new AlarmRow(StatusKind.Alarm, "13:41:20", "IM-01", Criterion.Mold.Message(66.2)),
                new AlarmRow(StatusKind.Info, "13:20:15", "IM-07", "레시피 변경 금형 M-0742 → M-0745"),
                new AlarmRow(StatusKind.Warn, "12:10:22", "IM-04", Criterion.Clamp.Message(420.0)),
                new AlarmRow(StatusKind.Alarm, "11:02:15", "IM-03", Criterion.Pressure.Message(151.3)),
            };
        }
    }
}
