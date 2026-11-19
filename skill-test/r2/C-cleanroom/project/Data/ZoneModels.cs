namespace CleanroomMonitor.Data
{
    /// <summary>구역 하나의 설정 기준(상·하한). 새로 만드는 값 객체라 readonly.</summary>
    internal sealed class ZoneLimit
    {
        public ZoneLimit(string zone, double tempLow, double tempHigh, double humiLow, double humiHigh, double particleHigh)
        {
            Zone = zone;
            TempLow = tempLow;
            TempHigh = tempHigh;
            HumiLow = humiLow;
            HumiHigh = humiHigh;
            ParticleHigh = particleHigh;
        }

        public readonly string Zone;
        public readonly double TempLow, TempHigh, HumiLow, HumiHigh, ParticleHigh;
    }

    /// <summary>구역 하나의 현재 센서값. null = 값 없음(통신 끊김). 차압은 기준이 없어 판정하지 않는다.</summary>
    internal sealed class ZoneReading
    {
        public ZoneReading(string zone, double? temp, double? humi, double? particle, double? pressure)
        {
            Zone = zone;
            Temp = temp;
            Humi = humi;
            Particle = particle;
            Pressure = pressure;
        }

        public readonly string Zone;
        public readonly double? Temp, Humi, Particle, Pressure;
        public bool HasValue { get { return Temp.HasValue && Humi.HasValue && Particle.HasValue; } }
    }
}
