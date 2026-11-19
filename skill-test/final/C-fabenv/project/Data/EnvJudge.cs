using System.Collections.Generic;
using APC.UI.Theming;

namespace FabEnvMonitor.Data
{
    /// <summary>한 항목의 판정. Reason = "상한 근접", Detail = "온도 상한 근접 22.9 ℃ (상한 23.0)".</summary>
    internal sealed class Verdict
    {
        public Verdict(StatusKind status, string reason, string detail)
        {
            Status = status; Reason = reason; Detail = detail;
        }

        public StatusKind Status { get; private set; }
        public string Reason { get; private set; }
        public string Detail { get; private set; }
    }

    /// <summary>베이 하나의 종합 판정(공정 상태 + 값 판정 중 나쁜 쪽).</summary>
    internal sealed class BayEvaluation
    {
        public BayEvaluation(StatusKind status, string label, string reason, Verdict temp, Verdict humi, Verdict particle)
        {
            Status = status; Label = label; Reason = reason; Temp = temp; Humi = humi; Particle = particle;
        }

        public StatusKind Status { get; private set; }
        /// <summary>정상 / 경고 / 알람 / 정지 / 통신 끊김</summary>
        public string Label { get; private set; }
        public string Reason { get; private set; }
        public Verdict Temp { get; private set; }
        public Verdict Humi { get; private set; }
        public Verdict Particle { get; private set; }
        public string StateText { get { return Reason.Length == 0 ? Label : Label + " · " + Reason; } }
    }

    /// <summary>
    /// 판정 함수 한 곳. 카드·헤더·이력·상세 표가 모두 이것만 쓴다.
    /// 상·하한과 같은 값은 허용(알람 아님). 경고는 상·하한에서 마진 안쪽부터(같은 값 포함).
    /// </summary>
    internal static class EnvJudge
    {
        // ---- 요구서에 없어 정한 기준값(고치려면 여기만) ----
        public const decimal DefaultTempLow = 21.0m;        // ℃
        public const decimal DefaultTempHigh = 23.0m;       // ℃
        public const decimal TempWarnMargin = 0.3m;         // 상·하한에서 0.3 ℃ 안쪽부터 경고
        public const decimal DefaultHumiLow = 40.0m;        // %RH
        public const decimal DefaultHumiHigh = 50.0m;       // %RH
        public const decimal HumiWarnMargin = 2.0m;         // 2.0 %RH
        public const decimal DefaultParticleHigh = 3520m;   // 개/m³ (≥0.5 µm, ISO 5 기준)
        public const decimal ParticleWarnMargin = 520m;     // 상한 3,520 → 3,000부터 경고
        // -----------------------------------------------

        public static string ItemName(EnvMetric m)
        {
            return m == EnvMetric.Temp ? "온도" : m == EnvMetric.Humi ? "습도" : "파티클";
        }

        public static string Unit(EnvMetric m)
        {
            return m == EnvMetric.Temp ? "℃" : m == EnvMetric.Humi ? "%RH" : "개/m³";
        }

        public static string Fmt(EnvMetric m, decimal v)
        {
            return m == EnvMetric.Particle ? v.ToString("#,0") : v.ToString("0.0");
        }

        public static decimal High(EnvMetric m, BaySetting s)
        {
            return m == EnvMetric.Temp ? s.TempHigh : m == EnvMetric.Humi ? s.HumiHigh : s.ParticleHigh;
        }

        /// <summary>하한. 파티클은 없음(null).</summary>
        public static decimal? Low(EnvMetric m, BaySetting s)
        {
            if (m == EnvMetric.Temp) return s.TempLow;
            if (m == EnvMetric.Humi) return s.HumiLow;
            return null;
        }

        private static decimal Margin(EnvMetric m)
        {
            return m == EnvMetric.Temp ? TempWarnMargin : m == EnvMetric.Humi ? HumiWarnMargin : ParticleWarnMargin;
        }

        public static Verdict Judge(EnvMetric m, decimal v, BaySetting s)
        {
            decimal high = High(m, s);
            decimal? low = Low(m, s);
            decimal margin = Margin(m);
            if (v > high) return Make(m, v, StatusKind.Alarm, "상한 초과", "상한", high);
            if (low.HasValue && v < low.Value) return Make(m, v, StatusKind.Alarm, "하한 미만", "하한", low.Value);
            if (v >= high - margin) return Make(m, v, StatusKind.Warn, "상한 근접", "상한", high);
            if (low.HasValue && v <= low.Value + margin) return Make(m, v, StatusKind.Warn, "하한 근접", "하한", low.Value);
            return new Verdict(StatusKind.Run, "정상", "");
        }

        private static Verdict Make(EnvMetric m, decimal v, StatusKind kind, string reason, string limitName, decimal limit)
        {
            string detail = ItemName(m) + " " + reason + " " + Fmt(m, v) + " " + Unit(m) + " (" + limitName + " " + Fmt(m, limit) + ")";
            return new Verdict(kind, reason, detail);
        }

        private static int Rank(StatusKind k)
        {
            return k == StatusKind.Alarm ? 2 : k == StatusKind.Warn ? 1 : 0;
        }

        public static BayEvaluation Evaluate(BayReading r, BaySetting s)
        {
            if (r.State == BayState.Disconnected) return new BayEvaluation(StatusKind.Stop, "통신 끊김", "", null, null, null);
            if (r.State == BayState.Stopped) return new BayEvaluation(StatusKind.Stop, "정지", r.Note, null, null, null);   // 정지 설비 값은 판정하지 않는다

            Verdict t = r.Temp.HasValue ? Judge(EnvMetric.Temp, r.Temp.Value, s) : null;
            Verdict h = r.Humi.HasValue ? Judge(EnvMetric.Humi, r.Humi.Value, s) : null;
            Verdict p = r.Particle.HasValue ? Judge(EnvMetric.Particle, r.Particle.Value, s) : null;

            Verdict worst = null;
            string worstName = "";
            Consider(ref worst, ref worstName, t, EnvMetric.Temp);
            Consider(ref worst, ref worstName, h, EnvMetric.Humi);
            Consider(ref worst, ref worstName, p, EnvMetric.Particle);

            if (worst == null || worst.Status == StatusKind.Run) return new BayEvaluation(StatusKind.Run, "정상", "", t, h, p);
            string label = worst.Status == StatusKind.Alarm ? "알람" : "경고";
            return new BayEvaluation(worst.Status, label, worstName + " " + worst.Reason, t, h, p);
        }

        private static void Consider(ref Verdict worst, ref string worstName, Verdict v, EnvMetric m)
        {
            if (v == null) return;
            if (worst == null || Rank(v.Status) > Rank(worst.Status)) { worst = v; worstName = ItemName(m); }
        }

        /// <summary>헤더 요약: "전체 · 알람 2 · 경고 3 · 정지 1 · 통신 끊김 1" (0인 것 생략, 모두 정상이면 "전체 · 정상").</summary>
        public static string Summarize(IList<BayEvaluation> list, out StatusKind worst)
        {
            int alarm = 0, warn = 0, stop = 0, comm = 0;
            foreach (var e in list)
            {
                if (e.Label == "알람") alarm++;
                else if (e.Label == "경고") warn++;
                else if (e.Label == "정지") stop++;
                else if (e.Label == "통신 끊김") comm++;
            }
            worst = alarm > 0 ? StatusKind.Alarm : warn > 0 ? StatusKind.Warn : (stop + comm) > 0 ? StatusKind.Stop : StatusKind.Run;
            string text = "전체";
            if (alarm + warn + stop + comm == 0) return text + " · 정상";
            if (alarm > 0) text += " · 알람 " + alarm;
            if (warn > 0) text += " · 경고 " + warn;
            if (stop > 0) text += " · 정지 " + stop;
            if (comm > 0) text += " · 통신 끊김 " + comm;
            return text;
        }
    }
}
