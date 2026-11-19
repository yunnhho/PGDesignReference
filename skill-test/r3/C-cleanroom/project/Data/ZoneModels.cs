namespace CleanroomMonitor.Data
{
    /// <summary>구역별 상·하한 설정(허용 한계). 상한과 같은 값은 정상 범위.</summary>
    internal sealed class ZoneLimits
    {
        public ZoneLimits(string zone, double tempLow, double tempHigh, double humiLow, double humiHigh, double particleHigh)
        {
            Zone = zone;
            TempLow = tempLow;
            TempHigh = tempHigh;
            HumiLow = humiLow;
            HumiHigh = humiHigh;
            ParticleHigh = particleHigh;
        }

        public string Zone { get; private set; }
        public double TempLow { get; private set; }
        public double TempHigh { get; private set; }
        public double HumiLow { get; private set; }
        public double HumiHigh { get; private set; }
        public double ParticleHigh { get; private set; }
    }

    /// <summary>구역 센서 현재값. 통신이 끊기면 값은 모두 null(빈칸), LastReceived에 마지막 수신 시각.</summary>
    internal sealed class ZoneReading
    {
        public ZoneReading(string zone, double? temp, double? humi, double? particle, double? diffPressure, string lastReceived)
        {
            Zone = zone;
            Temp = temp;
            Humi = humi;
            Particle = particle;
            DiffPressure = diffPressure;
            LastReceived = lastReceived;
        }

        public string Zone { get; private set; }
        public double? Temp { get; private set; }
        public double? Humi { get; private set; }
        public double? Particle { get; private set; }
        public double? DiffPressure { get; private set; }
        public string LastReceived { get; private set; }
        public bool Connected { get { return Temp.HasValue; } }
    }

    /// <summary>선택 구역 온습도 추이(시간순, 마지막 = 현재값).</summary>
    internal sealed class ZoneTrend
    {
        public ZoneTrend(string[] xLabels, double[] temps, double[] humis)
        {
            XLabels = xLabels;
            Temps = temps;
            Humis = humis;
        }

        public string[] XLabels { get; private set; }
        public double[] Temps { get; private set; }
        public double[] Humis { get; private set; }
    }
}
