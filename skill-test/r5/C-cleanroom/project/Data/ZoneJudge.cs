using System.Globalization;
using APC.UI.Theming;

namespace CleanroomMonitor.Data
{
    /// <summary>한 값의 판정(상태 + 사유 문구, 정상이면 사유 null).</summary>
    internal sealed class Verdict
    {
        public Verdict(StatusKind status, string reason) { Status = status; Reason = reason; }

        public readonly StatusKind Status;
        public readonly string Reason;
    }

    /// <summary>
    /// 판정 함수 한 곳. 상·하한과 같은 값은 알람이 아니고, 경고 구간(시작값 포함)이 있으므로 경계값은 경고.
    /// ※ 요구서에 없어 정한 기준값(아래 상수)은 사용자가 고칠 수 있게 여기에 모은다.
    /// </summary>
    internal static class ZoneJudge
    {
        public const double TempWarnMargin = 0.5;         // ℃: 상·하한에서 이만큼 안쪽부터 경고
        public const double HumiWarnMargin = 2.0;         // %RH
        public const int ParticleWarnPercent = 80;        // 파티클 상한의 80 %부터 경고
        public const double PressureLow = 10.0;           // Pa: 차압 하한(설정 dialog 항목 아님)
        public const double PressureWarnMargin = 2.0;     // Pa: 하한 + 2.0 이하 경고

        public static Verdict Temperature(double? v, ZoneLimits l)
        {
            return Range(v, l.TempLow, l.TempHigh, TempWarnMargin, "온도", "℃", "0.0");
        }

        public static Verdict Humidity(double? v, ZoneLimits l)
        {
            return Range(v, l.HumiLow, l.HumiHigh, HumiWarnMargin, "습도", "%RH", "0.0");
        }

        public static Verdict Pressure(double? v)
        {
            return Range(v, PressureLow, double.PositiveInfinity, PressureWarnMargin, "차압", "Pa", "0.0");
        }

        public static Verdict Particles(int? v, ZoneLimits l)
        {
            if (!v.HasValue) return new Verdict(StatusKind.Stop, null);
            int warnStart = l.ParticleHigh * ParticleWarnPercent / 100;
            string unit = " 개/ft³ (상한 " + l.ParticleHigh.ToString("#,0", CultureInfo.InvariantCulture) + ")";
            string value = v.Value.ToString("#,0", CultureInfo.InvariantCulture);
            if (v.Value > l.ParticleHigh) return new Verdict(StatusKind.Alarm, "파티클 상한 초과 " + value + unit);
            if (v.Value >= warnStart) return new Verdict(StatusKind.Warn, "파티클 상한 근접 " + value + unit);
            return new Verdict(StatusKind.Run, null);
        }

        private static Verdict Range(double? v, double low, double high, double margin, string name, string unit, string fmt)
        {
            if (!v.HasValue) return new Verdict(StatusKind.Stop, null);   // 값 없음 = 판정 안 함
            string text = v.Value.ToString(fmt, CultureInfo.InvariantCulture) + " " + unit;
            if (v.Value > high) return new Verdict(StatusKind.Alarm, name + " 상한 초과 " + text + " (상한 " + high.ToString(fmt, CultureInfo.InvariantCulture) + ")");
            if (v.Value < low) return new Verdict(StatusKind.Alarm, name + " 하한 미만 " + text + " (하한 " + low.ToString(fmt, CultureInfo.InvariantCulture) + ")");
            if (v.Value >= high - margin) return new Verdict(StatusKind.Warn, name + " 상한 근접 " + text + " (상한 " + high.ToString(fmt, CultureInfo.InvariantCulture) + ")");
            if (v.Value <= low + margin) return new Verdict(StatusKind.Warn, name + " 하한 근접 " + text + " (하한 " + low.ToString(fmt, CultureInfo.InvariantCulture) + ")");
            return new Verdict(StatusKind.Run, null);
        }

        /// <summary>구역 상태·문구("공정 상태 · 사유"). 정지·통신 끊김은 값 판정과 무관하게 Stop.</summary>
        public static Verdict Zone(ZoneReading r, ZoneLimits l)
        {
            if (r.Process == ZoneProcess.Disconnected) return new Verdict(StatusKind.Stop, "통신 끊김");
            if (r.Process == ZoneProcess.Stopped) return new Verdict(StatusKind.Stop, "정지 · 점검 중");
            Verdict worst = new Verdict(StatusKind.Run, "가동 · 정상");
            Verdict[] items = { Temperature(r.Temperature, l), Humidity(r.Humidity, l), Particles(r.Particles, l), Pressure(r.Pressure) };
            foreach (var it in items)
                if (Rank(it.Status) > Rank(worst.Status)) worst = new Verdict(it.Status, "가동 · " + it.Reason);   // 같으면 앞쪽 항목 유지
            return worst;
        }

        private static int Rank(StatusKind k) { return k == StatusKind.Alarm ? 2 : k == StatusKind.Warn ? 1 : 0; }
    }
}
