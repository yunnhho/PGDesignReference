using System;
using APC.UI.Theming;

namespace CleanroomMonitor.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 값은 기준값·추이 마지막 점과 서로 맞춘다.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private const int TrendPoints = 13;   // 5분 간격, 10:20 ~ 11:20

        private static readonly string[] Zones = { "A1", "A2", "A3", "A4", "B1", "B2", "B3", "B4" };

        // 구역, 온도, 습도, 파티클, 차압 — A3 온도 23.5 = 경고 시작값, B2 습도 60.0 = 상한과 같은 값(정상 범위 끝, 경고 구간)
        private static readonly double?[][] Values =
        {
            new double?[] { 22.1, 48.5, 1240, 15.2 },
            new double?[] { 22.4, 50.2, 3860, 14.8 },
            new double?[] { 23.5, 52.0, 5120, 16.0 },
            new double?[] { 21.8, 47.3, 8450, 15.5 },
            new double?[] { 22.9, 61.8, 4300, 12.5 },
            new double?[] { 22.6, 60.0, 2700, 17.3 },
            new double?[] { 22.3, 49.6, 2950, 14.1 },
            new double?[] { null, null, null, null },
        };

        private static readonly double[] Calm = { 0.1, -0.2, 0.3, 0.0, -0.3, 0.2, 0.4, -0.1, 0.0, -0.4, 0.2, -0.2, 0.1 };
        private static readonly double[] Rising = { -1.0, -0.9, -0.85, -0.7, -0.65, -0.6, -0.45, -0.4, -0.3, -0.2, -0.15, -0.05, 0.0 };
        private static readonly string[] Labels = { "10:20", "10:35", "10:50", "11:05", "11:20" };

        private ZoneLimits[] _limits;
        private int _intervalSec = ZoneJudge.DefaultStorageIntervalSec;

        public SampleMainViewSource()
        {
            _limits = DefaultLimits();
        }

        public static ZoneLimits[] DefaultLimits()
        {
            var l = new ZoneLimits[Zones.Length];
            for (int i = 0; i < l.Length; i++)
                l[i] = new ZoneLimits(Zones[i], ZoneJudge.DefaultTempLow, ZoneJudge.DefaultTempHigh,
                    ZoneJudge.DefaultHumiLow, ZoneJudge.DefaultHumiHigh, ZoneJudge.DefaultParticleHigh);
            return l;
        }

        public ZoneReading[] GetReadings()
        {
            var r = new ZoneReading[Zones.Length];
            for (int i = 0; i < r.Length; i++)
                r[i] = new ZoneReading(Zones[i], Values[i][0], Values[i][1], Values[i][2], Values[i][3], i == 7 ? "10:58:02" : "11:20:10");
            return r;
        }

        public ZoneLimits[] GetLimits() { return (ZoneLimits[])_limits.Clone(); }

        public int GetStorageIntervalSec() { return _intervalSec; }

        public void SaveSettings(ZoneLimits[] limits, int storageIntervalSec)
        {
            _limits = (ZoneLimits[])limits.Clone();
            _intervalSec = storageIntervalSec;
        }

        public ZoneTrend GetTrend(string zone)
        {
            int idx = Array.IndexOf(Zones, zone);
            var reading = GetReadings()[idx];
            if (!reading.Connected) return new ZoneTrend(new string[0], new double[0], new double[0]);

            var v = ZoneJudge.Judge(reading, _limits[idx]);
            return new ZoneTrend(Labels,
                Series(reading.Temp.Value, 0.5, 1.0, v.Temp != StatusKind.Run, idx),
                Series(reading.Humi.Value, 2.0, 4.0, v.Humi != StatusKind.Run, idx));
        }

        // 정상 항목은 구역마다 다르게 흔들리는 값, 기준 이탈 항목은 서서히 올라 현재값에 도달.
        private static double[] Series(double current, double calmAmp, double risingAmp, bool rising, int zoneIndex)
        {
            var s = new double[TrendPoints];
            for (int i = 0; i < TrendPoints; i++)
            {
                if (rising) s[i] = Math.Round(current + Rising[i] * risingAmp, 1);
                else s[i] = Math.Round(current + (Calm[(i + zoneIndex) % TrendPoints] - Calm[(TrendPoints - 1 + zoneIndex) % TrendPoints]) * calmAmp, 1);
            }
            return s;
        }
    }
}
