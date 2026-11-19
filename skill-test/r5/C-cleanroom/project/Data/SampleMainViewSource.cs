using System;

namespace CleanroomMonitor.Data
{
    /// <summary>화면 확인·render용 샘플. 현재값·기준·추이 마지막 점이 서로 맞는다.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private const int TrendPoints = 24;   // 1시간 간격 24점

        private static readonly ZoneReading[] Zones =
        {
            new ZoneReading("A1", ZoneProcess.Running, 22.1, 48.0, 42, 15.0),
            new ZoneReading("A2", ZoneProcess.Running, 24.0, 52.0, 35, 14.0),       // 온도 = 상한 → 경고
            new ZoneReading("A3", ZoneProcess.Running, 22.8, 61.5, 54, 13.0),       // 습도 상한 초과
            new ZoneReading("A4", ZoneProcess.Running, 21.9, 45.0, 78, 14.5),
            new ZoneReading("B1", ZoneProcess.Running, 22.5, 50.0, 420, 12.0),      // 차압 = 하한 + 2.0 → 경고
            new ZoneReading("B2", ZoneProcess.Running, 23.2, 47.0, 1240, 11.0),     // 파티클 상한 초과
            new ZoneReading("B3", ZoneProcess.Disconnected, null, null, null, null),
            new ZoneReading("B4", ZoneProcess.Stopped, 19.6, 44.0, 310, 8.0),
        };

        private ZoneSettings _settings = DefaultSettings();

        public ZoneReading[] GetZones() { return Zones; }

        public ZoneSettings GetSettings() { return _settings; }

        public void SaveSettings(ZoneSettings settings) { _settings = settings; }

        public ZoneTrend GetTrend(string zoneId)
        {
            ZoneReading z = Array.Find(Zones, r => r.Id == zoneId);
            if (z == null || !z.Temperature.HasValue) return new ZoneTrend(new double[0], new double[0]);
            int seed = zoneId[0] * 7 + zoneId[1];
            return new ZoneTrend(Series(z.Temperature.Value, 0.35, seed), Series(z.Humidity.Value, 1.2, seed + 3));
        }

        // 마지막 점 = 현재값. 앞은 현재값 주변의 완만한 변동.
        private static double[] Series(double current, double swing, int seed)
        {
            var v = new double[TrendPoints];
            for (int i = 0; i < TrendPoints - 1; i++)
                v[i] = Math.Round(current - swing * 0.6 * (TrendPoints - 1 - i) / TrendPoints * 2 + swing * Math.Sin(i * 0.9 + seed), 1);
            v[TrendPoints - 1] = current;
            return v;
        }

        private static ZoneSettings DefaultSettings()
        {
            var limits = new ZoneLimits[Zones.Length];
            for (int i = 0; i < Zones.Length; i++)
                limits[i] = new ZoneLimits(Zones[i].Id, 20.0, 24.0, 40.0, 60.0, Zones[i].Id[0] == 'A' ? 100 : 1000);
            return new ZoneSettings(limits, 10);
        }
    }
}
