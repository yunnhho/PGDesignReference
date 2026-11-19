using System;
using System.Collections.Generic;
using System.Linq;
using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 값은 레시피(설정 60.0 ℃ · 압력 상한 120.0 bar · 목표 25 s)와 맞다.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private const int TrendPoints = 30;
        private Recipe _recipe = new Recipe(60.0, 120.0, 25);

        private static readonly Machine[] Machines =
        {
            new Machine("IM-01", "가동", null, 60.2, 98.5, 24.8),
            new Machine("IM-02", "가동", null, 59.6, 112.4, 25.1),
            new Machine("IM-03", "가동", null, 65.4, 108.0, 25.4),
            new Machine("IM-04", "대기", null, null, null, null),
            new Machine("IM-05", "가동", null, 63.0, 101.2, 24.9),
            new Machine("IM-06", "정지", "계획 정지", 41.8, 0.0, null),
        };

        public Recipe GetRecipe() { return _recipe; }
        public void SaveRecipe(Recipe recipe) { _recipe = recipe; }
        public IList<Machine> GetMachines() { return Machines; }

        public double[] GetTempTrend(string machineId)
        {
            var m = Machines.First(x => x.Id == machineId);
            if (!m.Temp.HasValue) return new double[0];
            int seed = machineId[machineId.Length - 1];
            var v = new double[TrendPoints];
            for (int i = 0; i < TrendPoints; i++)
            {
                double ramp = Math.Pow(i / (double)(TrendPoints - 1), 3);
                double baseline = _recipe.SetTemp + (m.Temp.Value - _recipe.SetTemp) * ramp;
                v[i] = Math.Round(baseline + 0.5 * Math.Sin(i * 1.7 + seed) * (1 - ramp), 1);
            }
            v[TrendPoints - 1] = m.Temp.Value;   // 마지막 점 = 현재값
            return v;
        }

        public IList<AlarmRow> GetAlarms()
        {
            var d = new DateTime(2026, 11, 19);
            Func<int, int, int, DateTime> t = (h, mi, s) => d.AddHours(h).AddMinutes(mi).AddSeconds(s);
            return new List<AlarmRow>
            {
                new AlarmRow(t(14, 52, 10), StatusKind.Alarm, "IM-03", "금형 온도 상한 초과 65.4 ℃ (상한 65.0)"),
                new AlarmRow(t(14, 47, 36), StatusKind.Warn, "IM-05", "금형 온도 상한 근접 63.0 ℃ (상한 65.0)"),
                new AlarmRow(t(14, 31, 2), StatusKind.Stop, "IM-06", "계획 정지 (금형 교체)"),
                new AlarmRow(t(14, 12, 48), StatusKind.Warn, "IM-02", "사출 압력 상한 근접 115.0 bar (상한 120.0)"),
                new AlarmRow(t(13, 58, 20), StatusKind.Info, "IM-01", "사이클 타임 목표 변경 26 → 25 s"),
                new AlarmRow(t(13, 44, 7), StatusKind.Run, "IM-02", "사출 압력 정상 복귀 108.4 bar"),
                new AlarmRow(t(13, 21, 55), StatusKind.Alarm, "IM-03", "사이클 타임 상한 초과 30.4 s (상한 30.0)"),
                new AlarmRow(t(13, 5, 31), StatusKind.Warn, "IM-01", "사이클 타임 상한 근접 28.0 s (상한 30.0)"),
                new AlarmRow(t(12, 47, 14), StatusKind.Stop, "IM-04", "대기 전환 (작업 지시 없음)"),
                new AlarmRow(t(12, 30, 9), StatusKind.Run, "IM-05", "금형 온도 정상 복귀 61.8 ℃"),
                new AlarmRow(t(12, 2, 43), StatusKind.Alarm, "IM-04", "사출 압력 상한 초과 121.6 bar (상한 120.0)"),
            };
        }
    }
}
