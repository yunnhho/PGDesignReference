using System;
using System.Collections.Generic;
using APC.UI.Theming;

namespace FabEnvMonitor.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 판정은 EnvJudge 하나로 해서 카드·이력·상세가 서로 맞는다.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private const int TrendPoints = 25;
        private static readonly DateTime Today = new DateTime(2026, 11, 19);
        private IList<BaySetting> _settings = DefaultSettings();
        private readonly IList<BayReading> _bays = Bays();

        public IList<BayReading> GetBays() { return _bays; }

        public IList<BaySetting> GetSettings() { return _settings; }

        public void SaveSettings(IList<BaySetting> settings) { _settings = new List<BaySetting>(settings); }

        public IList<ExcursionRecord> GetRecentExcursions() { return Excursions(DefaultSettings()); }

        public double[] GetTrend24h(string bay, EnvMetric metric)
        {
            int idx = IndexOf(bay);
            BayReading r = _bays[idx];
            double nominal = metric == EnvMetric.Temp ? 22.0 : 45.0;
            double amp = metric == EnvMetric.Temp ? 0.25 : 1.2;
            decimal? cur = r.Value(metric);
            double last = cur.HasValue ? (double)cur.Value : nominal;   // 통신 끊김: 마지막 수신 값 대신 기준 근처로
            double phase = idx * 0.8 + (metric == EnvMetric.Humi ? 1.3 : 0);
            double endWave = amp * Math.Sin((TrendPoints - 1) * 0.6 + phase);
            var v = new double[TrendPoints];
            for (int i = 0; i < TrendPoints; i++)
            {
                double w = (double)i / (TrendPoints - 1);
                v[i] = Math.Round(nominal + amp * Math.Sin(i * 0.6 + phase) + (last - nominal - endWave) * w * w, 1);
            }
            v[TrendPoints - 1] = last;   // 추이 마지막 점 = 현재값
            return v;
        }

        private int IndexOf(string bay)
        {
            for (int i = 0; i < _bays.Count; i++) if (_bays[i].Name == bay) return i;
            throw new ArgumentException("알 수 없는 베이: " + bay);
        }

        private static IList<BaySetting> DefaultSettings()
        {
            var list = new List<BaySetting>();
            for (int i = 1; i <= 12; i++)
            {
                string name = "BAY-" + i.ToString("00");
                if (i == 3)   // 노광 베이: 온도 범위가 더 좁다
                    list.Add(new BaySetting(name, 21.5m, 22.5m, EnvJudge.DefaultHumiLow, EnvJudge.DefaultHumiHigh, EnvJudge.DefaultParticleHigh));
                else
                    list.Add(new BaySetting(name, EnvJudge.DefaultTempLow, EnvJudge.DefaultTempHigh, EnvJudge.DefaultHumiLow, EnvJudge.DefaultHumiHigh, EnvJudge.DefaultParticleHigh));
            }
            return list;
        }

        private static IList<BayReading> Bays()
        {
            return new List<BayReading>
            {
                new BayReading("BAY-01", BayState.Running, 22.1m, 45.2m, 820m, ""),
                new BayReading("BAY-02", BayState.Running, 22.4m, 46.0m, 1150m, ""),
                new BayReading("BAY-03", BayState.Running, 22.6m, 44.8m, 1480m, ""),   // 온도 상한(22.5) 초과
                new BayReading("BAY-04", BayState.Running, 21.8m, 44.3m, 640m, ""),
                new BayReading("BAY-05", BayState.Running, 22.9m, 47.0m, 1420m, ""),   // 온도 상한 근접
                new BayReading("BAY-06", BayState.Running, 22.0m, 45.5m, 2300m, ""),
                new BayReading("BAY-07", BayState.Running, 22.3m, 50.0m, 1980m, ""),   // 습도 = 상한(경계값) → 경고
                new BayReading("BAY-08", BayState.Running, 22.2m, 43.8m, 4210m, ""),   // 파티클 상한 초과
                new BayReading("BAY-09", BayState.Running, 21.6m, 46.4m, 900m, ""),
                new BayReading("BAY-10", BayState.Stopped, 20.4m, 38.5m, 310m, "점검"),   // 정지: 값은 판정 안 함
                new BayReading("BAY-11", BayState.Running, 22.0m, 41.5m, 760m, ""),   // 습도 하한 근접
                new BayReading("BAY-12", BayState.Disconnected, null, null, null, "마지막 수신 14:47:50"),
            };
        }

        private static ExcursionRecord Ev(string time, string bay, EnvMetric m, decimal value, IList<BaySetting> settings)
        {
            BaySetting s = settings[int.Parse(bay.Substring(4)) - 1];
            Verdict v = EnvJudge.Judge(m, value, s);
            if (v.Status == StatusKind.Run) throw new InvalidOperationException("샘플 이탈 값이 정상 범위: " + bay + " " + value);
            return new ExcursionRecord(At(time), bay, v.Status, v.Detail);
        }

        private static ExcursionRecord Back(string time, string bay, EnvMetric m, decimal value)
        {
            string text = EnvJudge.ItemName(m) + " 정상 복귀 " + EnvJudge.Fmt(m, value) + " " + EnvJudge.Unit(m);
            return new ExcursionRecord(At(time), bay, StatusKind.Run, text);
        }

        private static DateTime At(string hhmmss)
        {
            return Today.Add(TimeSpan.Parse(hhmmss));
        }

        private static IList<ExcursionRecord> Excursions(IList<BaySetting> s)
        {
            return new List<ExcursionRecord>
            {
                new ExcursionRecord(At("14:48:20"), "BAY-12", StatusKind.Stop, "통신 끊김 (마지막 수신 14:47:50)"),
                Ev("14:41:05", "BAY-08", EnvMetric.Particle, 4210m, s),
                Ev("14:36:40", "BAY-03", EnvMetric.Temp, 22.6m, s),
                Ev("14:20:12", "BAY-11", EnvMetric.Humi, 41.5m, s),
                Ev("13:58:30", "BAY-07", EnvMetric.Humi, 50.0m, s),
                Ev("13:42:18", "BAY-05", EnvMetric.Temp, 22.9m, s),
                new ExcursionRecord(At("13:10:00"), "BAY-10", StatusKind.Stop, "정지 · 점검"),
                Back("13:05:09", "BAY-02", EnvMetric.Particle, 1150m),
                Ev("12:47:33", "BAY-02", EnvMetric.Particle, 3310m, s),
                Back("12:15:40", "BAY-06", EnvMetric.Humi, 45.5m),
                Ev("11:30:55", "BAY-06", EnvMetric.Humi, 41.8m, s),
                Back("10:51:02", "BAY-09", EnvMetric.Temp, 21.6m),
                Ev("10:22:14", "BAY-09", EnvMetric.Temp, 20.8m, s),
                new ExcursionRecord(At("07:30:00"), "BAY-03", StatusKind.Info, "설정 변경 · 온도 범위 21.5 ~ 22.5 ℃"),
            };
        }
    }
}
