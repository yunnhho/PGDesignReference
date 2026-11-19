using System;
using System.Globalization;
using APC.UI.Theming;

namespace MoldingLineMonitor.Data
{
    /// <summary>한 측정 항목의 판정 결과: 상태와 사유("상한 근접" 등).</summary>
    internal sealed class Verdict
    {
        public Verdict(StatusKind status, string reason, double limit, string limitLabel)
        {
            Status = status; Reason = reason; Limit = limit; LimitLabel = limitLabel;
        }
        public StatusKind Status { get; private set; }
        public string Reason { get; private set; }
        public double Limit { get; private set; }
        public string LimitLabel { get; private set; }
    }

    /// <summary>
    /// 측정 항목 기준값(요구서에 없어 정한 값, 한 곳). 판정은 Judge 하나만 쓴다 — 타일·카드·알람 이력·dialog 공통.
    /// 상한과 같은 값은 알람이 아니다(초과부터). 경고는 시작값과 같은 값부터(>=, &lt;=). 없는 한계는 NaN.
    /// </summary>
    internal sealed class Criterion
    {
        // 정한 기준값. 사출기 8대 공통.
        public static readonly Criterion Mold = new Criterion("금형 온도", "℃", "0.0", 55.0, 57.0, 63.0, 65.0, "설정 60.0 · 상한 65.0 · 하한 55.0");
        public static readonly Criterion Pressure = new Criterion("사출 압력", "bar", "0.0", 110.0, 115.0, 140.0, 150.0, "설정 130.0 · 상한 150.0");
        public static readonly Criterion Clamp = new Criterion("형체력", "kN", "0.0", 400.0, 420.0, 480.0, 500.0, "상한 500.0 · 하한 400.0");
        public static readonly Criterion Cycle = new Criterion("사이클 타임", "s", "0.0", double.NaN, double.NaN, 26.0, 28.0, "목표 24 · 상한 28.0");

        public const double PressureSet = 130.0;   // 추이 그래프 기준선(회색 점선)
        public const decimal MoldSet = 60.0m, PressureUpper = 150.0m, ClampUpper = 500.0m, CycleTarget = 24m;   // 레시피 기본값

        private readonly double _lower, _lowerWarn, _upperWarn, _upper;

        private Criterion(string name, string unit, string format, double lower, double lowerWarn, double upperWarn, double upper, string footer)
        {
            Name = name; Unit = unit; Format = format; Footer = footer;
            _lower = lower; _lowerWarn = lowerWarn; _upperWarn = upperWarn; _upper = upper;
        }

        public string Name { get; private set; }
        public string Unit { get; private set; }
        public string Format { get; private set; }
        public string Footer { get; private set; }

        public string Text(double v) { return v.ToString(Format, CultureInfo.InvariantCulture); }

        public Verdict Judge(double v)
        {
            if (v > _upper) return new Verdict(StatusKind.Alarm, "상한 초과", _upper, "상한");
            if (v < _lower) return new Verdict(StatusKind.Alarm, "하한 미만", _lower, "하한");
            if (v >= _upperWarn) return new Verdict(StatusKind.Warn, "상한 근접", _upper, "상한");
            if (v <= _lowerWarn) return new Verdict(StatusKind.Warn, "하한 근접", _lower, "하한");
            return new Verdict(StatusKind.Run, null, double.NaN, null);
        }

        /// <summary>알람 이력 문구: "상한 초과 508.4 kN (상한 500.0)". 정상이면 null.</summary>
        public string Message(double v)
        {
            var d = Judge(v);
            if (d.Reason == null) return null;
            return Name + " " + d.Reason + " " + Text(v) + " " + Unit + " (" + d.LimitLabel + " " + Text(d.Limit) + ")";
        }
    }
}
