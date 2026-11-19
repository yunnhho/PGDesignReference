using APC.UI.Theming;

namespace CleanroomMonitor.Data
{
    /// <summary>
    /// 구역 판정 한 곳. 기준값(요구서에 없어 정함)은 아래 상수에 모았다.
    /// 상·하한은 허용 한계: 초과·미달부터 알람. 경고 시작값과 같은 값부터 경고(상한과 같은 값은 경고).
    /// 메인 표·헤더 요약·알람이 모두 이 함수를 쓴다.
    /// </summary>
    internal static class ZoneJudge
    {
        // ponytail: 기본 상·하한은 SampleMainViewSource 에, 아래는 설정 dialog 에 없는 고정 기준.
        public const double TempWarnMargin = 0.5;        // ℃: 상한 -0.5 이상 / 하한 +0.5 이하 = 경고
        public const double HumiWarnMargin = 2.0;        // %RH
        public const double ParticleWarnRatio = 0.8;     // 상한의 80 % 이상 = 경고
        public const double DiffPressureLow = 10.0;      // Pa 하한(양압 유지). 미만 = 알람
        public const double DiffPressureWarnStart = 12.0; // Pa 이하 = 경고

        public static ZoneJudgement Judge(ZoneReading r)
        {
            if (!r.Connected)
            {
                var none = new ItemJudge(StatusKind.Stop, "");
                return new ZoneJudgement(none, none, none, none, StatusKind.Stop,
                    "정지 · 통신 끊김 · " + r.LastReceived.ToString("HH:mm:ss") + " 마지막 수신");
            }
            var l = r.Limits;
            var t = Range(r.Temp.Value, l.TempLow, l.TempHigh, TempWarnMargin, "온도", "℃", "0.0");
            var h = Range(r.Humi.Value, l.HumiLow, l.HumiHigh, HumiWarnMargin, "습도", "%RH", "0.0");
            var p = Particle(r.Particle.Value, l.ParticleHigh);
            var d = Diff(r.DiffPressure.Value);
            // 가장 나쁜 것 하나, 같으면 화면 항목 순서(온도→습도→파티클→차압) 앞쪽
            var worst = t;
            foreach (var c in new[] { h, p, d }) if (Rank(c.Status) > Rank(worst.Status)) worst = c;
            string text = worst.Status == StatusKind.Run ? "정상" : (worst.Status == StatusKind.Alarm ? "알람 · " : "경고 · ") + worst.Text;
            return new ZoneJudgement(t, h, p, d, worst.Status, text);
        }

        private static int Rank(StatusKind k) { return k == StatusKind.Alarm ? 2 : k == StatusKind.Warn ? 1 : 0; }

        private static ItemJudge Range(double v, double low, double high, double margin, string name, string unit, string fmt)
        {
            string u = " " + unit;
            if (v > high) return new ItemJudge(StatusKind.Alarm, name + " 상한 초과 " + v.ToString(fmt) + u + " (상한 " + high.ToString(fmt) + ")");
            if (v < low) return new ItemJudge(StatusKind.Alarm, name + " 하한 미달 " + v.ToString(fmt) + u + " (하한 " + low.ToString(fmt) + ")");
            if (v >= high - margin) return new ItemJudge(StatusKind.Warn, name + " 상한 근접 " + v.ToString(fmt) + u + " (상한 " + high.ToString(fmt) + ")");
            if (v <= low + margin) return new ItemJudge(StatusKind.Warn, name + " 하한 근접 " + v.ToString(fmt) + u + " (하한 " + low.ToString(fmt) + ")");
            return new ItemJudge(StatusKind.Run, "");
        }

        private static ItemJudge Particle(double v, double high)
        {
            string s = v.ToString("#,0") + " 개/ft³ (상한 " + high.ToString("#,0") + ")";
            if (v > high) return new ItemJudge(StatusKind.Alarm, "파티클 상한 초과 " + s);
            if (v >= high * ParticleWarnRatio) return new ItemJudge(StatusKind.Warn, "파티클 상한 근접 " + s);
            return new ItemJudge(StatusKind.Run, "");
        }

        private static ItemJudge Diff(double v)
        {
            string s = v.ToString("0.0") + " Pa (하한 " + DiffPressureLow.ToString("0.0") + ")";
            if (v < DiffPressureLow) return new ItemJudge(StatusKind.Alarm, "차압 하한 미달 " + s);
            if (v <= DiffPressureWarnStart) return new ItemJudge(StatusKind.Warn, "차압 하한 근접 " + s);
            return new ItemJudge(StatusKind.Run, "");
        }
    }
}
