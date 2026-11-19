using System;
using System.Collections.Generic;

namespace CleanroomMonitor.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 정상 4·경고 2(온도 상한과 같은 값 24.0, 파티클 경고 시작 80)·알람 1·정지 1.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private const int TrendPoints = 13;                 // 5분 간격 1시간
        private static readonly DateTime Now = new DateTime(2026, 11, 19, 11, 0, 0);
        private static readonly string[] Names = { "A1", "A2", "A3", "A4", "B1", "B2", "B3", "B4" };

        private IList<ZoneLimits> _limits = DefaultLimits();
        private int _savePeriodSec = 60;

        public IList<ZoneLimits> GetLimits() { return _limits; }
        public int GetSavePeriodSec() { return _savePeriodSec; }
        public void SaveSettings(IList<ZoneLimits> limits, int savePeriodSec) { _limits = limits; _savePeriodSec = savePeriodSec; }

        private static IList<ZoneLimits> DefaultLimits()
        {
            var list = new List<ZoneLimits>();
            foreach (var n in Names)
                list.Add(n == "B4" ? new ZoneLimits(n, 19.0, 25.0, 40.0, 60.0, 100) : new ZoneLimits(n, 20.0, 24.0, 40.0, 60.0, 100));
            return list;
        }

        // 온도, 습도, 파티클, 차압 (null = 통신 끊김)
        private static readonly double?[][] Values =
        {
            new double?[] { 22.1, 47.5, 12, 15.2 },   // A1
            new double?[] { 21.8, 49.0, 35, 14.8 },   // A2
            new double?[] { 24.0, 51.2, 48, 13.9 },   // A3 온도 = 상한 → 경고
            new double?[] { 22.6, 50.3, 80, 15.0 },   // A4 파티클 = 경고 시작
            new double?[] { 22.3, 48.6, 20, 14.1 },   // B1
            new double?[] { 23.1, 52.0, 112, 13.2 },  // B2 파티클 상한 초과
            new double?[] { null, null, null, null }, // B3 통신 끊김
            new double?[] { 22.0, 49.5, 28, 15.6 },   // B4
        };

        public IList<ZoneReading> GetZones()
        {
            var list = new List<ZoneReading>();
            for (int i = 0; i < Names.Length; i++)
            {
                var v = Values[i];
                list.Add(new ZoneReading(_limits[i], v[0], v[1], v[2], v[3], v[0].HasValue ? Now : Now.AddMinutes(-18).AddSeconds(10)));
            }
            return list;
        }

        public ZoneTrend GetTrend(string zone)
        {
            int idx = Array.IndexOf(Names, zone);
            bool stopped = !Values[idx][0].HasValue;
            double tEnd = stopped ? 22.4 : Values[idx][0].Value;     // 정지 구역은 마지막 수신 값에서 끝난다
            double hEnd = stopped ? 48.8 : Values[idx][1].Value;
            DateTime end = stopped ? Now.AddMinutes(-18) : Now;
            var t = new double[TrendPoints];
            var h = new double[TrendPoints];
            for (int i = 0; i < TrendPoints; i++)
            {
                double fade = (TrendPoints - 1 - i) / (double)(TrendPoints - 1);   // 마지막 점에서 0 → 마지막 = 현재값
                t[i] = Math.Round(tEnd + 0.6 * Math.Sin(i * 0.9 + idx) * fade, 1);
                h[i] = Math.Round(hEnd + 2.5 * Math.Cos(i * 0.7 + idx) * fade, 1);
            }
            var labels = new[] { end.AddMinutes(-60).ToString("HH:mm"), end.AddMinutes(-30).ToString("HH:mm"), end.ToString("HH:mm") };
            return new ZoneTrend(t, h, labels);
        }
    }
}
