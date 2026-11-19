using APC.UI.Theming;

namespace ReliabilityTester.Data
{
    /// <summary>채널(챔버) 한 대의 현재 상태. 값이 없으면 null(대기·통신 전).</summary>
    internal sealed class ChannelRow
    {
        public ChannelRow(string name, string state, string sampleId, string condition,
            double? progress, int? remainingMinutes, double? insulationMOhm, double? leakageUA, string errorText)
        {
            Name = name; State = state; SampleId = sampleId; Condition = condition;
            Progress = progress; RemainingMinutes = remainingMinutes;
            InsulationMOhm = insulationMOhm; LeakageUA = leakageUA; ErrorText = errorText;
        }

        public readonly string Name;
        /// <summary>공정 상태: 시험중 / 완료 / 오류 / 대기</summary>
        public readonly string State;
        public readonly string SampleId;
        /// <summary>HAST / TC / HTOL</summary>
        public readonly string Condition;
        public readonly double? Progress;
        public readonly int? RemainingMinutes;
        public readonly double? InsulationMOhm;
        public readonly double? LeakageUA;
        /// <summary>State가 오류일 때 문구(코드 포함)</summary>
        public readonly string ErrorText;
    }

    /// <summary>금일 불량 분류 결과 한 건.</summary>
    internal sealed class DefectRow
    {
        public DefectRow(string channel, string sampleId, string mode, string judgedAt)
        {
            Channel = channel; SampleId = sampleId; Mode = mode; JudgedAt = judgedAt;
        }

        public readonly string Channel;
        public readonly string SampleId;
        public readonly string Mode;
        /// <summary>HH:mm:ss (당일)</summary>
        public readonly string JudgedAt;
    }

    /// <summary>챔버 상세 dialog에 넘기는 값. 추이 마지막 점 = 현재값.</summary>
    internal sealed class ChamberDetail
    {
        public ChamberDetail(string channel, StatusKind status, string statusText, double[] temp, double[] humi,
            double tempSet, double tempUpper, double humiSet, double humiUpper,
            StatusKind tempStatus, StatusKind humiStatus, object[][] errors)
        {
            Channel = channel; Status = status; StatusText = statusText; Temp = temp; Humi = humi;
            TempSet = tempSet; TempUpper = tempUpper; HumiSet = humiSet; HumiUpper = humiUpper;
            TempStatus = tempStatus; HumiStatus = humiStatus; Errors = errors;
        }

        public readonly string Channel;
        public readonly StatusKind Status;
        public readonly string StatusText;
        public readonly double[] Temp;
        public readonly double[] Humi;
        public readonly double TempSet, TempUpper, HumiSet, HumiUpper;
        public readonly StatusKind TempStatus, HumiStatus;
        /// <summary>행마다 { StatusKind, 시각 HH:mm:ss, 코드, 내용 }, 최근 것이 위</summary>
        public readonly object[][] Errors;
    }
}
