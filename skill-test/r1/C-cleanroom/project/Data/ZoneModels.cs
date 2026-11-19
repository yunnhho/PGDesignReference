using APC.UI.Theming;

namespace CleanroomMonitor.Data
{
    /// <summary>구역 한 곳의 현재 측정값. 값이 null이면 수신 전·통신 끊김.</summary>
    internal sealed class ZoneReading
    {
        public ZoneReading(string name, double? temp, double? humi, double? particle, double? pressure, string time)
        {
            Name = name; Temp = temp; Humi = humi; Particle = particle; Pressure = pressure; Time = time;
        }
        public string Name { get; private set; }
        public double? Temp { get; private set; }
        public double? Humi { get; private set; }
        public double? Particle { get; private set; }
        public double? Pressure { get; private set; }
        public string Time { get; private set; }
    }

    /// <summary>구역별 기준(설정 dialog에서 바꾸는 값).</summary>
    internal sealed class ZoneLimit
    {
        public ZoneLimit(string name, decimal tempLow, decimal tempHigh, decimal humiLow, decimal humiHigh, decimal particleMax)
        {
            Name = name; TempLow = tempLow; TempHigh = tempHigh; HumiLow = humiLow; HumiHigh = humiHigh; ParticleMax = particleMax;
        }
        public string Name { get; private set; }
        public decimal TempLow { get; private set; }
        public decimal TempHigh { get; private set; }
        public decimal HumiLow { get; private set; }
        public decimal HumiHigh { get; private set; }
        public decimal ParticleMax { get; private set; }
    }

    /// <summary>선택 구역 추이(과거 → 현재). 마지막 점 = 현재값.</summary>
    internal sealed class ZoneTrend
    {
        public ZoneTrend(double[] temps, double[] humis) { Temps = temps; Humis = humis; }
        public double[] Temps { get; private set; }
        public double[] Humis { get; private set; }
    }

    /// <summary>
    /// 구역 판정 함수 하나(표 램프·상태 문구·헤더 요약·알람이 모두 이것을 쓴다).
    /// 기준 초과 = Alarm, 기준과 같거나 근접(여유 안) = Warn, 수신 없음 = Stop.
    /// 차압은 최소 PressureMin 이상이어야 한다(구역별 설정 대상 아님).
    /// </summary>
    internal static class ZoneJudge
    {
        public const double PressureMin = 5.0, PressureWarnMargin = 1.0;   // Pa
        public const double TempMargin = 0.5, HumiMargin = 2.0;            // ℃, %RH
        public const double ParticleWarnRatio = 0.9;                       // 상한의 90% 이상

        /// <summary>판정과 사유(정상이면 "정상", 정지면 "수신 없음").</summary>
        public static StatusKind Judge(ZoneReading r, ZoneLimit l, out string reason)
        {
            reason = "수신 없음";
            if (!r.Temp.HasValue || !r.Humi.HasValue || !r.Particle.HasValue || !r.Pressure.HasValue) return StatusKind.Stop;

            double tl = (double)l.TempLow, th = (double)l.TempHigh, hl = (double)l.HumiLow, hh = (double)l.HumiHigh, pm = (double)l.ParticleMax;
            if (r.Temp.Value > th) return Alarm(out reason, "온도 상한 초과 " + r.Temp.Value.ToString("0.0") + " > " + th.ToString("0.0") + " ℃");
            if (r.Temp.Value < tl) return Alarm(out reason, "온도 하한 미달 " + r.Temp.Value.ToString("0.0") + " < " + tl.ToString("0.0") + " ℃");
            if (r.Humi.Value > hh) return Alarm(out reason, "습도 상한 초과 " + r.Humi.Value.ToString("0.0") + " > " + hh.ToString("0.0") + " %RH");
            if (r.Humi.Value < hl) return Alarm(out reason, "습도 하한 미달 " + r.Humi.Value.ToString("0.0") + " < " + hl.ToString("0.0") + " %RH");
            if (r.Particle.Value > pm) return Alarm(out reason, "파티클 상한 초과 " + r.Particle.Value.ToString("#,0") + " > " + pm.ToString("#,0") + " 개/ft³");
            if (r.Pressure.Value < PressureMin) return Alarm(out reason, "차압 미달 " + r.Pressure.Value.ToString("0.0") + " < " + PressureMin.ToString("0.0") + " Pa");

            if (r.Temp.Value >= th - TempMargin) return Warn(out reason, "온도 상한 근접 " + r.Temp.Value.ToString("0.0") + " ℃ (상한 " + th.ToString("0.0") + ")");
            if (r.Temp.Value <= tl + TempMargin) return Warn(out reason, "온도 하한 근접 " + r.Temp.Value.ToString("0.0") + " ℃ (하한 " + tl.ToString("0.0") + ")");
            if (r.Humi.Value >= hh - HumiMargin) return Warn(out reason, "습도 상한 근접 " + r.Humi.Value.ToString("0.0") + " %RH (상한 " + hh.ToString("0.0") + ")");
            if (r.Humi.Value <= hl + HumiMargin) return Warn(out reason, "습도 하한 근접 " + r.Humi.Value.ToString("0.0") + " %RH (하한 " + hl.ToString("0.0") + ")");
            if (r.Particle.Value >= pm * ParticleWarnRatio) return Warn(out reason, "파티클 상한 근접 " + r.Particle.Value.ToString("#,0") + " 개/ft³ (상한 " + pm.ToString("#,0") + ")");
            if (r.Pressure.Value <= PressureMin + PressureWarnMargin) return Warn(out reason, "차압 하한 근접 " + r.Pressure.Value.ToString("0.0") + " Pa (하한 " + PressureMin.ToString("0.0") + ")");

            reason = "정상";
            return StatusKind.Run;
        }

        private static StatusKind Alarm(out string reason, string text) { reason = text; return StatusKind.Alarm; }
        private static StatusKind Warn(out string reason, string text) { reason = text; return StatusKind.Warn; }
    }
}
