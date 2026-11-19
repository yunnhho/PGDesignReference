using System;
using System.Collections.Generic;
using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 추이 마지막 점 = 현재 금형 온도.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private const int TrendPoints = 61;   // 최근 60분, 1분 간격

        public IList<Machine> GetMachines()
        {
            return new List<Machine>
            {
                new Machine("IM-01", ProcessState.Running, "", 180.2m, 118.6m, 24.9m, Trend(180.2, 1.6, 0.0)),
                new Machine("IM-02", ProcessState.Running, "", 179.5m, 140.0m, 25.6m, Trend(179.5, 1.2, 1.0)),
                new Machine("IM-03", ProcessState.Running, "", 185.0m, 126.4m, 25.3m, Trend(185.0, 2.5, 2.0)),
                new Machine("IM-04", ProcessState.Running, "", 181.4m, 141.5m, 27.2m, Trend(181.4, 1.4, 3.0)),
                new Machine("IM-05", ProcessState.Standby, "", null, null, null, new double[0]),
                new Machine("IM-06", ProcessState.Stopped, "계획 정지", 96.5m, 0.0m, null, Trend(96.5, 0.4, 4.0)),
            };
        }

        public IList<AlarmRow> GetAlarmHistory()
        {
            return new List<AlarmRow>
            {
                new AlarmRow(StatusKind.Alarm, "14:52:10", "IM-04", "사출 압력 상한 초과 141.5 bar (상한 140.0)"),
                new AlarmRow(StatusKind.Warn, "14:41:37", "IM-03", "금형 온도 상한 근접 185.0 ℃ (상한 190.0)"),
                new AlarmRow(StatusKind.Warn, "14:18:52", "IM-04", "사이클 타임 상한 근접 27.2 s (상한 28.0)"),
                new AlarmRow(StatusKind.Stop, "13:58:12", "IM-06", "정지 · 계획 정지"),
                new AlarmRow(StatusKind.Info, "13:35:40", "IM-01", "레시피 변경 금형 온도 설정 178.0 → 180.0 ℃"),
                new AlarmRow(StatusKind.Warn, "13:07:25", "IM-02", "사이클 타임 상한 근접 28.0 s (상한 28.0)"),
                new AlarmRow(StatusKind.Stop, "12:46:03", "IM-05", "대기 · 금형 교체"),
                new AlarmRow(StatusKind.Run, "12:26:03", "IM-02", "사출 압력 정상 복귀 138.4 bar"),
                new AlarmRow(StatusKind.Alarm, "12:20:44", "IM-02", "사출 압력 상한 초과 143.2 bar (상한 140.0)"),
                new AlarmRow(StatusKind.Run, "11:41:55", "IM-03", "금형 온도 정상 복귀 181.0 ℃"),
                new AlarmRow(StatusKind.Alarm, "11:32:10", "IM-03", "금형 온도 상한 초과 191.3 ℃ (상한 190.0)"),
            };
        }

        public void SaveRecipe(string machine, decimal tempSet, decimal pressureUpper, int cycleTarget)
        {
            // 샘플: 저장하지 않는다. 실제 구현은 PLC·DB에 쓴다.
        }

        // 마지막 점이 정확히 last가 되는 추이(결정적 샘플).
        private static double[] Trend(double last, double amplitude, double phase)
        {
            var v = new double[TrendPoints];
            for (int i = 0; i < TrendPoints; i++)
                v[i] = Math.Round(last + amplitude * Math.Sin(i / 7.0 + phase) * (TrendPoints - 1 - i) / (TrendPoints - 1), 1);
            v[TrendPoints - 1] = last;
            return v;
        }
    }
}
