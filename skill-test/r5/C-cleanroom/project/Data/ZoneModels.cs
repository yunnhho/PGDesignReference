namespace CleanroomMonitor.Data
{
    internal enum ZoneProcess { Running, Stopped, Disconnected }

    /// <summary>구역 한 곳의 현재 센서값. 값이 null이면 읽은 값 없음(통신 전·끊김).</summary>
    internal sealed class ZoneReading
    {
        public ZoneReading(string id, ZoneProcess process, double? temperature, double? humidity, int? particles, double? pressure)
        {
            Id = id; Process = process; Temperature = temperature; Humidity = humidity; Particles = particles; Pressure = pressure;
        }

        public readonly string Id;
        public readonly ZoneProcess Process;
        public readonly double? Temperature;   // ℃
        public readonly double? Humidity;      // %RH
        public readonly int? Particles;        // 0.5 µm, 개/ft³
        public readonly double? Pressure;      // Pa
    }

    /// <summary>구역별 기준(설정 dialog에서 바꾼다). 차압 기준은 ZoneJudge 상수.</summary>
    internal sealed class ZoneLimits
    {
        public ZoneLimits(string id, double tempLow, double tempHigh, double humiLow, double humiHigh, int particleHigh)
        {
            Id = id; TempLow = tempLow; TempHigh = tempHigh; HumiLow = humiLow; HumiHigh = humiHigh; ParticleHigh = particleHigh;
        }

        public readonly string Id;
        public readonly double TempLow, TempHigh, HumiLow, HumiHigh;
        public readonly int ParticleHigh;
    }

    internal sealed class ZoneSettings
    {
        public ZoneSettings(ZoneLimits[] limits, int savePeriodSec) { Limits = limits; SavePeriodSec = savePeriodSec; }

        public readonly ZoneLimits[] Limits;
        public readonly int SavePeriodSec;   // 데이터 저장 주기(s)
    }

    /// <summary>선택 구역 추이(시간순, 마지막 점 = 현재값). 값이 없으면 빈 배열.</summary>
    internal sealed class ZoneTrend
    {
        public ZoneTrend(double[] temperatures, double[] humidities) { Temperatures = temperatures; Humidities = humidities; }

        public readonly double[] Temperatures;
        public readonly double[] Humidities;
    }
}
