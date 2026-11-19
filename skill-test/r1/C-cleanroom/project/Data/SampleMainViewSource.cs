using System;
using System.Collections.Generic;

namespace CleanroomMonitor.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 상태: Run 4, Warn 2(경계값 2), Alarm 1, Stop 1.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private IList<ZoneLimit> _limits = new List<ZoneLimit>
        {
            new ZoneLimit("A1", 20.0m, 24.0m, 40.0m, 60.0m, 100m),
            new ZoneLimit("A2", 20.0m, 24.0m, 40.0m, 60.0m, 100m),
            new ZoneLimit("A3", 20.0m, 24.0m, 40.0m, 60.0m, 100m),
            new ZoneLimit("A4", 20.0m, 24.0m, 40.0m, 60.0m, 100m),
            new ZoneLimit("B1", 20.0m, 24.0m, 40.0m, 60.0m, 1000m),
            new ZoneLimit("B2", 20.0m, 24.0m, 40.0m, 60.0m, 1000m),
            new ZoneLimit("B3", 20.0m, 24.0m, 40.0m, 60.0m, 1000m),
            new ZoneLimit("B4", 20.0m, 24.0m, 40.0m, 60.0m, 1000m),
        };
        private int _saveIntervalSec = 10;

        private static readonly ZoneReading[] Zones =
        {
            new ZoneReading("A1", 22.1, 45.2, 12, 15.0, "14:32:10"),
            new ZoneReading("A2", 22.4, 46.0, 18, 14.8, "14:32:10"),
            new ZoneReading("A3", 24.0, 48.5, 35, 15.2, "14:32:10"),   // 온도 = 상한(경계) → 경고
            new ZoneReading("A4", 21.8, 44.1, 9, 15.1, "14:32:10"),
            new ZoneReading("B1", 22.9, 50.3, 210, 10.2, "14:32:09"),
            new ZoneReading("B2", 23.1, 63.4, 640, 9.8, "14:32:09"),   // 습도 상한 초과 → 알람
            new ZoneReading("B3", 22.7, 52.0, 520, 5.0, "14:32:10"),   // 차압 = 하한(경계) → 경고
            new ZoneReading("B4", null, null, null, null, ""),         // 통신 끊김 → 정지
        };

        public IList<ZoneReading> GetZones() { return Zones; }
        public IList<ZoneLimit> GetLimits() { return _limits; }
        public int GetSaveIntervalSec() { return _saveIntervalSec; }

        public void SaveSettings(IList<ZoneLimit> limits, int saveIntervalSec)
        {
            _limits = limits;
            _saveIntervalSec = saveIntervalSec;
        }

        public ZoneTrend GetTrend(string zone)
        {
            ZoneReading r = null;
            foreach (var z in Zones) if (z.Name == zone) r = z;
            if (r == null || !r.Temp.HasValue) return new ZoneTrend(new double[0], new double[0]);
            int seed = zone[0] * 7 + zone[1];
            var t = new double[61];
            var h = new double[61];
            for (int i = 0; i <= 60; i++)
            {
                double k = 60 - i;   // 현재에서 거슬러 간 분
                t[i] = r.Temp.Value + 0.35 * Math.Sin((i + seed) / 6.0) * Math.Min(1.0, k / 4.0);   // 마지막 점 = 현재값
                h[i] = r.Humi.Value + 1.2 * Math.Sin((i + seed) / 9.0 + 1) * Math.Min(1.0, k / 4.0);
            }
            return new ZoneTrend(t, h);
        }
    }
}
