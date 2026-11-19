using System;
using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    /// <summary>판정 결과 한 건: 상태색, 짧은 사유(타일·헤더), 값이 든 사유(알람 이력·알람바). 정상이면 사유 없음.</summary>
    internal sealed class Judgement
    {
        public readonly StatusKind Status;
        public readonly string Reason;
        public readonly string Detail;

        public Judgement(StatusKind status, string reason, string detail)
        {
            Status = status; Reason = reason; Detail = detail;
        }
    }

    /// <summary>
    /// 정한 기준값과 판정 함수 한 곳(요구서에 없어 정함 — 고칠 때는 여기만 바꾼다).
    /// 상한은 허용 한계: 상한과 같으면 알람 아님, 초과부터 알람. 경고는 시작값과 같은 값부터.
    /// </summary>
    internal static class Criteria
    {
        public const decimal TempSet = 180.0m;       // 금형 온도 설정 ℃
        public const decimal TempWarn = 185.0m;      // 금형 온도 경고 시작 ℃
        public const decimal TempUpper = 190.0m;     // 금형 온도 상한 ℃
        public const decimal PressureUpper = 140.0m; // 사출 압력 상한 bar (경고 구간 없음)
        public const int CycleTarget = 25;           // 사이클 타임 목표 s (사람이 넣는 설정값 = 정수)
        public const decimal CycleWarn = 27.0m;      // 사이클 타임 경고 시작 s (측정값 자리수)
        public const decimal CycleUpper = 28.0m;     // 사이클 타임 상한 s

        private static readonly Judgement Normal = new Judgement(StatusKind.Run, "", "");

        public static Judgement JudgeTemp(decimal v)
        {
            string tail = " " + v.ToString("0.0") + " ℃ (상한 " + TempUpper.ToString("0.0") + ")";
            if (v > TempUpper) return new Judgement(StatusKind.Alarm, "금형 온도 상한 초과", "금형 온도 상한 초과" + tail);
            if (v >= TempWarn) return new Judgement(StatusKind.Warn, "금형 온도 상한 근접", "금형 온도 상한 근접" + tail);
            return Normal;
        }

        public static Judgement JudgePressure(decimal v)
        {
            string tail = " " + v.ToString("0.0") + " bar (상한 " + PressureUpper.ToString("0.0") + ")";
            if (v > PressureUpper) return new Judgement(StatusKind.Alarm, "사출 압력 상한 초과", "사출 압력 상한 초과" + tail);
            return Normal;
        }

        public static Judgement JudgeCycle(decimal v)
        {
            string tail = " " + v.ToString("0.0") + " s (상한 " + CycleUpper.ToString("0.0") + ")";
            if (v > CycleUpper) return new Judgement(StatusKind.Alarm, "사이클 타임 상한 초과", "사이클 타임 상한 초과" + tail);
            if (v >= CycleWarn) return new Judgement(StatusKind.Warn, "사이클 타임 상한 근접", "사이클 타임 상한 근접" + tail);
            return Normal;
        }

        /// <summary>카드용: 값이 없으면 판정하지 않고 Stop.</summary>
        public static Judgement Card(decimal? v, Func<decimal, Judgement> judge)
        {
            return v.HasValue ? judge(v.Value) : new Judgement(StatusKind.Stop, "", "");
        }

        /// <summary>설비 상태 = 공정 상태와 값 판정 중 나쁜 쪽. 정지·대기는 공정 상태를 따른다. 같으면 항목 순서(온도→압력→사이클) 앞쪽.</summary>
        public static Judgement JudgeMachine(Machine m)
        {
            if (m.Process == ProcessState.Standby) return new Judgement(StatusKind.Stop, "대기", "");
            if (m.Process == ProcessState.Stopped) return new Judgement(StatusKind.Stop, "정지 · " + m.Note, "");
            Judgement worst = Normal;
            foreach (var j in new[] { Card(m.Temp, JudgeTemp), Card(m.Pressure, JudgePressure), Card(m.Cycle, JudgeCycle) })
            {
                bool worse = j.Status == StatusKind.Alarm ? worst.Status != StatusKind.Alarm
                           : j.Status == StatusKind.Warn && worst.Status == StatusKind.Run;
                if (worse) worst = j;
            }
            return worst;
        }

        /// <summary>타일 문구: "공정 상태 · 사유".</summary>
        public static string TileText(Machine m)
        {
            var j = JudgeMachine(m);
            if (m.Process != ProcessState.Running) return j.Reason;
            return j.Status == StatusKind.Run ? "가동 · 정상" : "가동 · " + j.Reason;
        }
    }
}
