using System.Collections.Generic;
using System.Globalization;
using APC.UI.Theming;

namespace ReliabilityTester.Data
{
    /// <summary>
    /// 판정 기준과 판정 함수 한 곳. 기준값은 요구서에 없어 정한 값(고객 확인 필요).
    /// 상·하한은 허용 한계: 하한과 같은 값은 알람이 아니다(미만부터 알람). 경고 시작값과 같은 값부터 경고.
    /// </summary>
    internal static class Criteria
    {
        public const double InsulationLower = 100.0;       // MΩ, 미만이면 알람
        public const double InsulationWarnStart = 120.0;   // MΩ, 이하면 경고
        public const double LeakageUpper = 5.0;            // µA, 초과하면 알람
        public const double LeakageWarnStart = 4.0;        // µA, 이상이면 경고

        public const string Testing = "시험중", Done = "완료", Error = "오류", Standby = "대기";

        public static string F1(double? v)
        {
            return v.HasValue ? v.Value.ToString("0.0", CultureInfo.InvariantCulture) : "";
        }

        /// <summary>남은 시간 h:mm. 값 없으면 빈칸.</summary>
        public static string Hm(int? minutes)
        {
            return minutes.HasValue ? (minutes.Value / 60) + ":" + (minutes.Value % 60).ToString("00") : "";
        }

        public static StatusKind JudgeInsulation(double v)
        {
            if (v < InsulationLower) return StatusKind.Alarm;
            return v <= InsulationWarnStart ? StatusKind.Warn : StatusKind.Run;
        }

        public static StatusKind JudgeLeakage(double v)
        {
            if (v > LeakageUpper) return StatusKind.Alarm;
            return v >= LeakageWarnStart ? StatusKind.Warn : StatusKind.Run;
        }

        /// <summary>시험중일 때만 값을 판정한다. 완료·오류·대기가 읽은 값은 판정하지 않는다(Stop).</summary>
        public static StatusKind InsulationStatus(ChannelRow r)
        {
            return r.State == Testing && r.InsulationMOhm.HasValue ? JudgeInsulation(r.InsulationMOhm.Value) : StatusKind.Stop;
        }

        public static StatusKind LeakageStatus(ChannelRow r)
        {
            return r.State == Testing && r.LeakageUA.HasValue ? JudgeLeakage(r.LeakageUA.Value) : StatusKind.Stop;
        }

        private static int Rank(StatusKind k)
        {
            switch (k)
            {
                case StatusKind.Alarm: return 4;
                case StatusKind.Warn: return 3;
                case StatusKind.Stop: return 2;
                case StatusKind.Info: return 1;
                default: return 0;
            }
        }

        public static StatusKind Worse(StatusKind a, StatusKind b)
        {
            return Rank(b) > Rank(a) ? b : a;
        }

        /// <summary>공정 상태와 값 판정 중 나쁜 쪽(타일·표 행·요약이 모두 이것을 쓴다).</summary>
        public static StatusKind ChannelStatus(ChannelRow r)
        {
            switch (r.State)
            {
                case Error: return StatusKind.Alarm;
                case Standby: return StatusKind.Stop;
                case Done: return StatusKind.Run;
                default:
                    return Worse(StatusKind.Run, Worse(InsulationStatus(r), LeakageStatus(r)));
            }
        }

        /// <summary>시험중 채널의 이상 사유(없으면 null). 같으면 절연저항이 앞.</summary>
        private static string Reason(ChannelRow r)
        {
            var ir = InsulationStatus(r);
            var lk = LeakageStatus(r);
            if (Rank(lk) > Rank(ir)) return lk == StatusKind.Alarm ? "누설전류 상한 초과" : "누설전류 한계 근접";
            if (ir == StatusKind.Alarm) return "절연저항 하한 미만";
            if (ir == StatusKind.Warn) return "절연저항 한계 근접";
            return null;
        }

        /// <summary>"공정 상태 · 사유" 문구.</summary>
        public static string StatusText(ChannelRow r)
        {
            if (r.State == Error) return Error + " · " + r.ErrorText;
            if (r.State != Testing) return r.State;
            string reason = Reason(r);
            return reason == null ? Testing + " · 정상" : Testing + " · " + reason;
        }

        /// <summary>알람바 문구: 값과 기준 포함.</summary>
        public static string AlarmText(ChannelRow r)
        {
            if (r.State == Error) return r.Name + ": " + Error + " · " + r.ErrorText;
            if (LeakageStatus(r) == StatusKind.Alarm && Rank(LeakageStatus(r)) > Rank(InsulationStatus(r)))
                return r.Name + ": 누설전류 상한 초과 " + F1(r.LeakageUA) + " µA (상한 " + F1(LeakageUpper) + ")";
            return r.Name + ": 절연저항 하한 미만 " + F1(r.InsulationMOhm) + " MΩ (하한 " + F1(InsulationLower) + ")";
        }

        /// <summary>헤더 요약 "전체 · 알람 2 · 경고 1 · 대기 1" (정상 외 모두, 0은 생략, 순서 알람→경고→정지→대기).</summary>
        public static string Summary(IList<ChannelRow> rows)
        {
            int alarm = 0, warn = 0, stop = 0, standby = 0;
            foreach (var r in rows)
            {
                var s = ChannelStatus(r);
                if (s == StatusKind.Alarm) alarm++;
                else if (s == StatusKind.Warn) warn++;
                else if (r.State == Standby) standby++;
                else if (s == StatusKind.Stop) stop++;
            }
            string t = "전체";
            if (alarm > 0) t += " · 알람 " + alarm;
            if (warn > 0) t += " · 경고 " + warn;
            if (stop > 0) t += " · 정지 " + stop;
            if (standby > 0) t += " · 대기 " + standby;
            return alarm + warn + stop + standby == 0 ? "전체 · 정상" : t;
        }

        public static StatusKind WorstOf(IList<ChannelRow> rows)
        {
            var w = StatusKind.Run;
            foreach (var r in rows) w = Worse(w, ChannelStatus(r));
            return w;
        }
    }
}
