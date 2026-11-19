using System.Globalization;
using APC.UI.Theming;

namespace CleanroomMonitor.Data
{
    /// <summary>구역 판정 결과. Reason은 정상이면 빈 문자열. 항목별 상태는 표 셀 글자색에 쓴다.</summary>
    internal sealed class ZoneJudgement
    {
        public ZoneJudgement(StatusKind status, string stateText, StatusKind temp, StatusKind humi, StatusKind particle)
        {
            Status = status;
            StateText = stateText;
            Temp = temp;
            Humi = humi;
            Particle = particle;
        }

        public readonly StatusKind Status;
        public readonly string StateText;
        public readonly StatusKind Temp, Humi, Particle;
    }

    /// <summary>판정 함수 하나. 표·헤더 요약·알람바가 모두 이것을 쓴다.</summary>
    internal static class ZoneJudge
    {
        public const double TempWarnMargin = 0.5;      // ℃: 상·하한에서 이만큼 안쪽부터 경고
        public const double HumiWarnMargin = 2.0;      // %RH
        public const double ParticleWarnRatio = 0.9;   // 상한의 90 % 이상부터 경고

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static ZoneJudgement Evaluate(ZoneReading r, ZoneLimit l)
        {
            if (!r.HasValue) return new ZoneJudgement(StatusKind.Stop, "정지 · 통신 끊김", StatusKind.Stop, StatusKind.Stop, StatusKind.Stop);

            StatusKind tk, hk, pk;
            string tr, hr, pr;
            Range(r.Temp.Value, l.TempLow, l.TempHigh, TempWarnMargin, "온도", "℃", "0.0", out tk, out tr);
            Range(r.Humi.Value, l.HumiLow, l.HumiHigh, HumiWarnMargin, "습도", "%RH", "0.0", out hk, out hr);
            Range(r.Particle.Value, double.NegativeInfinity, l.ParticleHigh, l.ParticleHigh * (1 - ParticleWarnRatio), "파티클", "개/ft³", "#,0", out pk, out pr);

            // 가장 나쁜 것 하나, 같으면 항목 순서 앞쪽
            StatusKind worst = tk; string reason = tr;
            if (Rank(hk) > Rank(worst)) { worst = hk; reason = hr; }
            if (Rank(pk) > Rank(worst)) { worst = pk; reason = pr; }

            string text = worst == StatusKind.Run ? "정상" : (worst == StatusKind.Warn ? "경고" : "알람") + " · " + reason;
            return new ZoneJudgement(worst, text, tk, hk, pk);
        }

        private static int Rank(StatusKind k) { return k == StatusKind.Alarm ? 2 : (k == StatusKind.Warn ? 1 : 0); }

        // 상한과 같으면 정상 범위(초과부터 알람), 경고는 근접 시작값과 같은 값부터
        private static void Range(double v, double lo, double hi, double margin, string name, string unit, string fmt, out StatusKind kind, out string reason)
        {
            string vs = v.ToString(fmt, Inv) + " " + unit;
            if (v > hi) { kind = StatusKind.Alarm; reason = name + " 상한 초과 " + vs + " (상한 " + hi.ToString(fmt, Inv) + ")"; }
            else if (v < lo) { kind = StatusKind.Alarm; reason = name + " 하한 미만 " + vs + " (하한 " + lo.ToString(fmt, Inv) + ")"; }
            else if (v >= hi - margin) { kind = StatusKind.Warn; reason = name + " 상한 근접 " + vs + " (상한 " + hi.ToString(fmt, Inv) + ")"; }
            else if (v <= lo + margin) { kind = StatusKind.Warn; reason = name + " 하한 근접 " + vs + " (하한 " + lo.ToString(fmt, Inv) + ")"; }
            else { kind = StatusKind.Run; reason = ""; }
        }
    }
}
