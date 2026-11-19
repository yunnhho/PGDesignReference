using System.Globalization;
using APC.UI.Theming;

namespace CleanroomMonitor.Data
{
    /// <summary>구역 판정 결과: 구역 전체 상태·문구와 항목별 상태.</summary>
    internal sealed class ZoneVerdict
    {
        public ZoneVerdict(StatusKind status, string text, StatusKind temp, StatusKind humi, StatusKind particle, StatusKind diff)
        {
            Status = status;
            Text = text;
            Temp = temp;
            Humi = humi;
            Particle = particle;
            Diff = diff;
        }

        public StatusKind Status { get; private set; }
        public string Text { get; private set; }
        public StatusKind Temp { get; private set; }
        public StatusKind Humi { get; private set; }
        public StatusKind Particle { get; private set; }
        public StatusKind Diff { get; private set; }
    }

    /// <summary>
    /// 판정 함수 하나(메인 표·카드·알람·헤더 요약이 모두 이것을 쓴다) + 요구서에 없어 정한 기준값.
    /// 상·하한은 허용 한계: 같은 값은 OK, 초과/미만부터 알람. 경고는 시작값과 같은 값부터.
    /// </summary>
    internal static class ZoneJudge
    {
        // ---- 정한 기준값(요구서에 없음) ----
        public const double TempWarnMargin = 0.5;        // ℃: 상한-0.5 이상 / 하한+0.5 이하 경고
        public const double HumiWarnMargin = 2.0;        // %RH
        public const double ParticleWarnRatio = 0.8;     // 파티클: 상한의 80% 이상 경고
        public const double DiffLow = 10.0;              // Pa 차압 하한(설정 dialog 항목 아님)
        public const double DiffHigh = 25.0;             // Pa 차압 상한
        public const double DiffWarnMargin = 2.0;        // Pa
        public const int DefaultStorageIntervalSec = 60; // 데이터 저장 주기 s
        public const double DefaultTempLow = 20.0, DefaultTempHigh = 24.0;
        public const double DefaultHumiLow = 40.0, DefaultHumiHigh = 60.0;
        public const double DefaultParticleHigh = 10000;
        private const double Eps = 1e-9;

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static string Fmt1(double v) { return v.ToString("0.0", Inv); }
        public static string FmtCount(double v) { return v.ToString("#,0", Inv); }

        public static StatusKind Range(double v, double low, double high, double margin)
        {
            if (v > high + Eps || v < low - Eps) return StatusKind.Alarm;
            if (v >= high - margin - Eps || v <= low + margin + Eps) return StatusKind.Warn;
            return StatusKind.Run;
        }

        public static StatusKind Upper(double v, double high)
        {
            if (v > high + Eps) return StatusKind.Alarm;
            return v >= high * ParticleWarnRatio - Eps ? StatusKind.Warn : StatusKind.Run;
        }

        public static int Severity(StatusKind k)
        {
            switch (k)
            {
                case StatusKind.Alarm: return 3;
                case StatusKind.Warn: return 2;
                case StatusKind.Stop: return 1;
                default: return 0;
            }
        }

        public static ZoneVerdict Judge(ZoneReading r, ZoneLimits l)
        {
            if (!r.Connected)
                return new ZoneVerdict(StatusKind.Stop, "정지 · 통신 끊김 · " + r.LastReceived + " 마지막 수신",
                    StatusKind.Stop, StatusKind.Stop, StatusKind.Stop, StatusKind.Stop);

            var t = Range(r.Temp.Value, l.TempLow, l.TempHigh, TempWarnMargin);
            var h = Range(r.Humi.Value, l.HumiLow, l.HumiHigh, HumiWarnMargin);
            var p = Upper(r.Particle.Value, l.ParticleHigh);
            var d = Range(r.DiffPressure.Value, DiffLow, DiffHigh, DiffWarnMargin);

            // 가장 나쁜 항목 하나(같으면 온도 → 습도 → 파티클 → 차압 순)
            StatusKind worst = StatusKind.Run;
            string reason = "";
            Consider(t, "온도", r.Temp.Value, "℃", l.TempLow, l.TempHigh, TempWarnMargin, false, ref worst, ref reason);
            Consider(h, "습도", r.Humi.Value, "%RH", l.HumiLow, l.HumiHigh, HumiWarnMargin, false, ref worst, ref reason);
            Consider(p, "파티클", r.Particle.Value, "개/ft³", 0, l.ParticleHigh, 0, true, ref worst, ref reason);
            Consider(d, "차압", r.DiffPressure.Value, "Pa", DiffLow, DiffHigh, DiffWarnMargin, false, ref worst, ref reason);

            string text = worst == StatusKind.Run ? "정상" : (worst == StatusKind.Alarm ? "알람 · " : "경고 · ") + reason;
            return new ZoneVerdict(worst, text, t, h, p, d);
        }

        private static void Consider(StatusKind k, string name, double v, string unit, double low, double high,
            double margin, bool count, ref StatusKind worst, ref string reason)
        {
            if (k == StatusKind.Run || Severity(k) <= Severity(worst)) return;
            worst = k;
            Describe(name, v, unit, low, high, margin, count, k, out reason);
        }

        private static void Describe(string name, double v, string unit, double low, double high, double margin,
            bool count, StatusKind k, out string reason)
        {
            Fmt f = count ? (Fmt)FmtCount : Fmt1;
            bool upperSide = v > high + Eps || (k == StatusKind.Warn && (count || v >= high - margin - Eps));
            string state = k == StatusKind.Alarm ? (upperSide ? "상한 초과" : "하한 미만") : (upperSide ? "상한 근접" : "하한 근접");
            reason = name + " " + state + " " + f(v) + " " + unit + " (" + (upperSide ? "상한 " + f(high) : "하한 " + f(low)) + ")";
        }

        private delegate string Fmt(double v);
    }
}
