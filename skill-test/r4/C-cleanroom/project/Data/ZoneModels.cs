using System;
using APC.UI.Theming;

namespace CleanroomMonitor.Data
{
    /// <summary>구역별 설정 상·하한(설정 dialog에서 바꾼다).</summary>
    internal sealed class ZoneLimits
    {
        public ZoneLimits(string zone, double tempLow, double tempHigh, double humiLow, double humiHigh, double particleHigh)
        {
            Zone = zone; TempLow = tempLow; TempHigh = tempHigh; HumiLow = humiLow; HumiHigh = humiHigh; ParticleHigh = particleHigh;
        }
        public readonly string Zone;
        public readonly double TempLow, TempHigh, HumiLow, HumiHigh, ParticleHigh;
    }

    /// <summary>구역 현재값. 값이 없으면(통신 끊김) null. LastReceived = 마지막 수신 시각.</summary>
    internal sealed class ZoneReading
    {
        public ZoneReading(ZoneLimits limits, double? temp, double? humi, double? particle, double? diffPressure, DateTime lastReceived)
        {
            Limits = limits; Temp = temp; Humi = humi; Particle = particle; DiffPressure = diffPressure; LastReceived = lastReceived;
        }
        public readonly ZoneLimits Limits;
        public readonly double? Temp, Humi, Particle, DiffPressure;
        public readonly DateTime LastReceived;
        public string Zone { get { return Limits.Zone; } }
        public bool Connected { get { return Temp.HasValue; } }
    }

    /// <summary>선택 구역 온습도 추이(시간순, 왼쪽=과거). 마지막 점 = 현재값.</summary>
    internal sealed class ZoneTrend
    {
        public ZoneTrend(double[] temp, double[] humi, string[] xLabels) { Temp = temp; Humi = humi; XLabels = xLabels; }
        public readonly double[] Temp, Humi;
        public readonly string[] XLabels;
    }

    /// <summary>값 하나의 판정 결과(상태 + 문구). 이상이 없으면 Text는 빈 문자열.</summary>
    internal sealed class ItemJudge
    {
        public ItemJudge(StatusKind status, string text) { Status = status; Text = text; }
        public readonly StatusKind Status;
        public readonly string Text;
    }

    /// <summary>구역 한 곳의 판정: 항목별 + 가장 나쁜 것.</summary>
    internal sealed class ZoneJudgement
    {
        public ZoneJudgement(ItemJudge temp, ItemJudge humi, ItemJudge particle, ItemJudge diff, StatusKind status, string text)
        {
            Temp = temp; Humi = humi; Particle = particle; Diff = diff; Status = status; Text = text;
        }
        public readonly ItemJudge Temp, Humi, Particle, Diff;
        public readonly StatusKind Status;
        public readonly string Text;
    }
}
