using System.Globalization;
using APC.UI.Theming;

namespace FATester.Data
{
    /// <summary>
    /// 측정 항목 한 개의 판정 기준과 판정 함수. 상태색·문구가 모두 이 클래스 하나로 나온다.
    /// 상한과 같은 값은 알람이 아니다(초과부터 알람). 경고 시작값과 같은 값부터 경고.
    /// </summary>
    internal sealed class Limit
    {
        public Limit(string name, string unit, string format, double warn, double upper, double? setPoint = null)
        {
            Name = name; Unit = unit; Format = format; Warn = warn; Upper = upper; SetPoint = setPoint;
        }

        public string Name { get; private set; }
        public string Unit { get; private set; }
        public string Format { get; private set; }
        public double Warn { get; private set; }
        public double Upper { get; private set; }
        public double? SetPoint { get; private set; }

        public string Text(double v) { return v.ToString(Format, CultureInfo.InvariantCulture); }

        public StatusKind Judge(double v)
        {
            if (v > Upper) return StatusKind.Alarm;
            if (v >= Warn) return StatusKind.Warn;
            return StatusKind.Run;
        }

        /// <summary>타일·알람 문구용 사유. 정상이면 null.</summary>
        public string Reason(double v)
        {
            StatusKind k = Judge(v);
            if (k == StatusKind.Alarm) return Name + " 상한 초과";
            if (k == StatusKind.Warn) return Name + " 한계 근접";
            return null;
        }

        /// <summary>카드 아래 줄: 판정 글 + 기준.</summary>
        public string Footer(double v)
        {
            StatusKind k = Judge(v);
            string verdict = k == StatusKind.Alarm ? "상한 초과" : (k == StatusKind.Warn ? "상한 근접" : "정상");
            return verdict + " · 기준 " + Text(Upper) + " 이하 · 경고 " + Text(Warn) + " 이상";
        }
    }
}
