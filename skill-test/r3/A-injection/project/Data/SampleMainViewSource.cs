using System;
using System.Collections.Generic;
using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 값·기준은 Judgement 한 곳을 따른다.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private const int TrendPoints = 24;
        private Recipe _recipe = new Recipe((decimal)Judgement.TempSetpoint, (decimal)Judgement.PressureUpper, Judgement.CycleTarget);

        public IList<MachineInfo> GetMachines()
        {
            return new[]
            {
                new MachineInfo("IM-01", ProcessState.Running, 180.0, 950.0, 29.8, ""),
                new MachineInfo("IM-02", ProcessState.Running, 185.0, 1010.0, 30.5, ""),
                new MachineInfo("IM-03", ProcessState.Running, 181.2, 1040.0, 32.0, ""),
                new MachineInfo("IM-04", ProcessState.Running, 191.4, 1100.0, 33.1, ""),
                new MachineInfo("IM-05", ProcessState.Standby, null, null, null, ""),
                new MachineInfo("IM-06", ProcessState.Stopped, null, null, null, "통신 끊김"),
            };
        }

        public IList<double> GetTempTrend(string machineId)
        {
            var values = new List<double>();
            foreach (var m in GetMachines())
            {
                if (m.Id != machineId || !m.Temp.HasValue) continue;
                double last = m.Temp.Value, seed = machineId[machineId.Length - 1];
                for (int i = 0; i < TrendPoints - 1; i++)
                {
                    double t = (double)i / (TrendPoints - 1);
                    double v = Judgement.TempSetpoint + (last - Judgement.TempSetpoint) * t * t + 1.5 * Math.Sin(i * 0.9 + seed);
                    values.Add(Math.Round(v, 1));
                }
                values.Add(last);
            }
            return values;
        }

        public string[] GetTrendLabels() { return new[] { "13:05:00", "14:00:00", "15:00:00" }; }

        public IList<AlarmEntry> GetRecentAlarms()
        {
            return new[]
            {
                Meas("14:58:41", "IM-04", Judgement.Temp(191.4)),
                Meas("14:52:10", "IM-03", Judgement.Cycle(32.0)),
                Meas("14:46:18", "IM-04", Judgement.Temp(190.0)),
                Meas("14:41:37", "IM-02", Judgement.Temp(185.0)),
                Ev("14:30:22", "IM-06", StatusKind.Stop, "통신 끊김"),
                Meas("14:12:45", "IM-04", Judgement.Pressure(1100.0)),
                Ev("14:05:10", "IM-01", StatusKind.Info, "금형 온도 설정 변경 178.0 → 180.0 ℃"),
                Ev("13:48:33", "IM-05", StatusKind.Stop, "대기 전환 (금형 교체)"),
                Meas("13:31:09", "IM-02", Judgement.Cycle(35.0)),
                Ev("13:22:40", "IM-03", StatusKind.Run, "금형 온도 정상 복귀 178.4 ℃"),
                Meas("13:15:52", "IM-03", Judgement.Temp(169.8)),
                Meas("13:08:07", "IM-03", Judgement.Temp(170.0)),
                Meas("12:20:36", "IM-02", Judgement.Pressure(1215.6)),
            };
        }

        public Recipe GetRecipe() { return _recipe; }

        public void SaveRecipe(Recipe recipe) { _recipe = recipe; }

        private static AlarmEntry Meas(string time, string machine, Verdict v)
        {
            return new AlarmEntry(v.Status, time, machine, v.Detail);
        }

        private static AlarmEntry Ev(string time, string machine, StatusKind kind, string text)
        {
            return new AlarmEntry(kind, time, machine, text);
        }
    }
}
