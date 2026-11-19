using System;
using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 값은 Judging 기준과 서로 맞춘다.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private const int TrendPoints = 25;
        private static readonly DateTime Now = new DateTime(2026, 11, 19, 14, 53, 0);

        private RecipeValues _recipe = new RecipeValues((decimal)Judging.MoldTempSet, (decimal)Judging.PressureAlarm, Judging.CycleTarget);

        public MachineInfo[] GetMachines()
        {
            return new[]
            {
                new MachineInfo("IM-01", "가동", 60.2, 128.0, 29.6),
                new MachineInfo("IM-02", "가동", 59.8, 140.0, 32.0),
                new MachineInfo("IM-03", "가동", 63.0, 135.0, 30.8),
                new MachineInfo("IM-04", "가동", 65.4, 132.0, 35.0),
                new MachineInfo("IM-05", "대기", null, null, null),
                new MachineInfo("IM-06", "정지", 38.6, 0.0, null),
            };
        }

        public double[] GetMoldTempTrend(string machineId)
        {
            foreach (var m in GetMachines())
            {
                if (m.Id != machineId) continue;
                if (!m.MoldTemp.HasValue) return new double[0];
                double slope = machineId == "IM-03" ? 0.12 : machineId == "IM-04" ? 0.2 : machineId == "IM-06" ? 0.0 : 0.0;
                double[] noise = { 0.2, -0.1, 0.1, -0.2, 0.0, 0.1, -0.1, 0.2, -0.2, 0.1, 0.0, -0.1, 0.2 };
                var v = new double[TrendPoints];
                for (int i = 0; i < TrendPoints; i++)
                {
                    double n = i == TrendPoints - 1 ? 0.0 : noise[i % noise.Length] * (slope == 0.0 ? 1.0 : 0.5);
                    v[i] = Math.Round(m.MoldTemp.Value - (TrendPoints - 1 - i) * slope + n, 1);
                }
                return v;
            }
            return new double[0];
        }

        public string[] GetTrendLabels()
        {
            var first = Now.AddMinutes(-(TrendPoints - 1));
            return new[] { first.ToString("HH:mm"), first.AddMinutes(6).ToString("HH:mm"), first.AddMinutes(12).ToString("HH:mm"),
                           first.AddMinutes(18).ToString("HH:mm"), Now.ToString("HH:mm") };
        }

        public AlarmRow[] GetRecentAlarms()
        {
            return new[]
            {
                new AlarmRow(StatusKind.Alarm, At(14, 52, 10), "IM-04", "금형 온도 상한 초과 65.4 ℃ (상한 65.0)"),
                new AlarmRow(StatusKind.Alarm, At(14, 47, 32), "IM-04", "사이클 타임 한계 초과 35.0 s (목표 30)"),
                new AlarmRow(StatusKind.Warn, At(14, 41, 37), "IM-03", "금형 온도 상한 근접 63.0 ℃ (상한 65.0)"),
                new AlarmRow(StatusKind.Warn, At(14, 22, 5), "IM-02", "사출 압력 상한 근접 140.0 bar (상한 150.0)"),
                new AlarmRow(StatusKind.Stop, At(13, 58, 12), "IM-06", "금형 교체 정지"),
                new AlarmRow(StatusKind.Warn, At(13, 44, 51), "IM-02", "사이클 타임 지연 32.0 s (목표 30)"),
                new AlarmRow(StatusKind.Info, At(13, 20, 0), "IM-01", "레시피 변경 사출 압력 상한 145.0 → 150.0 bar"),
                new AlarmRow(StatusKind.Stop, At(12, 36, 18), "IM-05", "생산 계획 없음 대기 전환"),
                new AlarmRow(StatusKind.Run, At(12, 9, 44), "IM-03", "금형 온도 정상 복귀 60.8 ℃"),
                new AlarmRow(StatusKind.Alarm, At(11, 52, 3), "IM-03", "금형 온도 상한 초과 65.0 ℃ (상한 65.0)"),
                new AlarmRow(StatusKind.Warn, At(11, 15, 27), "IM-01", "사이클 타임 지연 33.4 s (목표 30)"),
                new AlarmRow(StatusKind.Info, At(10, 30, 0), "IM-04", "교대 인계 확인"),
            };
        }

        public RecipeValues GetRecipe() { return _recipe; }

        public void SaveRecipe(RecipeValues recipe) { _recipe = recipe; }

        private static DateTime At(int h, int m, int s) { return new DateTime(2026, 11, 19, h, m, s); }
    }
}
