using System;
using APC.UI.Theming;

namespace FabEnvMonitor.Data
{
    internal enum BayState { Running, Stopped, Disconnected }

    internal enum EnvMetric { Temp, Humi, Particle }

    /// <summary>베이 하나의 현재 측정값. 값이 없으면(통신 끊김) null.</summary>
    internal sealed class BayReading
    {
        public BayReading(string name, BayState state, decimal? temp, decimal? humi, decimal? particle, string note)
        {
            Name = name; State = state; Temp = temp; Humi = humi; Particle = particle; Note = note;
        }

        public string Name { get; private set; }
        public BayState State { get; private set; }
        public decimal? Temp { get; private set; }
        public decimal? Humi { get; private set; }
        public decimal? Particle { get; private set; }
        /// <summary>정지 사유 또는 통신 끊김 시 마지막 수신 시각 문구.</summary>
        public string Note { get; private set; }

        public decimal? Value(EnvMetric m)
        {
            return m == EnvMetric.Temp ? Temp : m == EnvMetric.Humi ? Humi : Particle;
        }
    }

    /// <summary>베이별 허용 범위. 파티클은 상한만.</summary>
    internal sealed class BaySetting
    {
        public BaySetting(string name, decimal tempLow, decimal tempHigh, decimal humiLow, decimal humiHigh, decimal particleHigh)
        {
            Name = name; TempLow = tempLow; TempHigh = tempHigh; HumiLow = humiLow; HumiHigh = humiHigh; ParticleHigh = particleHigh;
        }

        public string Name { get; private set; }
        public decimal TempLow { get; private set; }
        public decimal TempHigh { get; private set; }
        public decimal HumiLow { get; private set; }
        public decimal HumiHigh { get; private set; }
        public decimal ParticleHigh { get; private set; }
    }

    /// <summary>최근 이탈 이력 한 줄.</summary>
    internal sealed class ExcursionRecord
    {
        public ExcursionRecord(DateTime time, string bay, StatusKind status, string text)
        {
            Time = time; Bay = bay; Status = status; Text = text;
        }

        public DateTime Time { get; private set; }
        public string Bay { get; private set; }
        public StatusKind Status { get; private set; }
        public string Text { get; private set; }
    }
}
