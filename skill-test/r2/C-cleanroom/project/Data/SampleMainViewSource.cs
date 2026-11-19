using System.Collections.Generic;
using System.Linq;

namespace CleanroomMonitor.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 현재값·기준·추이 마지막 점이 서로 맞는다.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        // 과거 → 현재 변화량(마지막 0 = 현재값). 온도는 그대로, 습도는 ×3
        private static readonly double[] TempOffsets = { -0.6, -0.4, -0.5, -0.2, -0.3, 0.1, -0.1, 0.2, 0.0, -0.2, 0.1, -0.1, 0.0 };

        private readonly List<ZoneReading> _readings = new List<ZoneReading>
        {
            new ZoneReading("A1", 22.1, 48.5, 420, 12.5),
            new ZoneReading("A2", 23.5, 44.0, 650, 12.3),   // 온도 23.5 = 상한 24.0 - 0.5 → 경고 시작값
            new ZoneReading("A3", 21.8, 50.2, 1000, 12.0),  // 파티클 1,000 = 상한 → 초과 아님, 경고
            new ZoneReading("A4", 22.4, 38.0, 380, 12.8),   // 습도 하한 40.0 미만 → 알람
            new ZoneReading("B1", 22.9, 52.0, 6200, 10.1),
            new ZoneReading("B2", 21.4, 47.0, 3100, 9.8),
            new ZoneReading("B3", 22.0, 52.0, 12400, 10.4), // 파티클 상한 10,000 초과 → 알람
            new ZoneReading("B4", null, null, null, null),  // 통신 끊김
        };

        private List<ZoneLimit> _limits = new List<ZoneLimit>
        {
            new ZoneLimit("A1", 20.0, 24.0, 40.0, 60.0, 1000),
            new ZoneLimit("A2", 20.0, 24.0, 40.0, 60.0, 1000),
            new ZoneLimit("A3", 20.0, 24.0, 40.0, 60.0, 1000),
            new ZoneLimit("A4", 20.0, 24.0, 40.0, 60.0, 1000),
            new ZoneLimit("B1", 20.0, 26.0, 40.0, 60.0, 10000),
            new ZoneLimit("B2", 20.0, 26.0, 40.0, 60.0, 10000),
            new ZoneLimit("B3", 20.0, 26.0, 40.0, 60.0, 10000),
            new ZoneLimit("B4", 20.0, 26.0, 40.0, 60.0, 10000),
        };

        private int _saveIntervalSec = 10;

        public IList<ZoneReading> GetReadings() { return _readings; }
        public IList<ZoneLimit> GetLimits() { return _limits; }
        public int GetSaveIntervalSec() { return _saveIntervalSec; }

        public IList<double> GetTemperatureTrend(string zone)
        {
            var r = _readings.First(x => x.Zone == zone);
            return r.Temp.HasValue ? TempOffsets.Select(o => r.Temp.Value + o).ToList() : new List<double>();
        }

        public IList<double> GetHumidityTrend(string zone)
        {
            var r = _readings.First(x => x.Zone == zone);
            return r.Humi.HasValue ? TempOffsets.Select(o => r.Humi.Value + o * 3).ToList() : new List<double>();
        }

        public string[] GetTrendTimeLabels() { return new[] { "14:00", "14:15", "14:30", "14:45", "15:00" }; }

        public void SaveSettings(IList<ZoneLimit> limits, int saveIntervalSec)
        {
            _limits = limits.ToList();
            _saveIntervalSec = saveIntervalSec;
        }
    }
}
