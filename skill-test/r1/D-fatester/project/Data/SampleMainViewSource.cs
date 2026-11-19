using System.Collections.Generic;
using APC.UI.Theming;

namespace FATester.Data
{
    /// <summary>화면 확인·render용 샘플 데이터.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        public IList<ChannelInfo> GetChannels()
        {
            return new[]
            {
                // 저항 45.0 = 상한 50.0의 90% → 경고 경계값
                new ChannelInfo("CH1", "시험중", StatusKind.Run, "FA2611-0031", "열충격", 62, 45.0, 38.5, 125.0, 12.0, 1240, "없음", ""),
                new ChannelInfo("CH2", "완료", StatusKind.Run, "FA2611-0028", "고온고습", 100, 38.2, 52.7, 85.0, 85.0, 1000, "없음", ""),
                // 누설전류 100.0 = 상한 → 알람 경계값
                new ChannelInfo("CH3", "오류", StatusKind.Alarm, "FA2611-0033", "전기특성", 37, 41.6, 100.0, 25.0, 45.0, 312, "E-304", "누설전류 상한 100.0 nA 도달"),
                new ChannelInfo("CH4", "대기", StatusKind.Stop, "", "", null, null, null, 25.0, 45.0, 1875, "없음", ""),
            };
        }

        public IList<DefectRow> GetDefects()
        {
            return new[]
            {
                new DefectRow(StatusKind.Alarm, "FA2611-0027", "솔더볼 크랙", "Ball B12", "14:52:10"),
                new DefectRow(StatusKind.Alarm, "FA2611-0025", "와이어 본딩 단선", "Wire #7", "14:31:48"),
                new DefectRow(StatusKind.Warn, "FA2611-0024", "보이드", "Die 좌상단", "14:12:03"),
                new DefectRow(StatusKind.Alarm, "FA2611-0022", "박리", "Mold 가장자리", "13:40:27"),
                new DefectRow(StatusKind.Alarm, "FA2611-0019", "오픈", "Ball A03", "13:22:55"),
                new DefectRow(StatusKind.Warn, "FA2611-0018", "미세 크랙", "Die 중앙", "12:47:31"),
                new DefectRow(StatusKind.Alarm, "FA2611-0016", "단락", "Ball C08", "11:58:09"),
                new DefectRow(StatusKind.Alarm, "FA2611-0014", "솔더볼 크랙", "Ball D21", "11:05:44"),
                new DefectRow(StatusKind.Warn, "FA2611-0012", "보이드", "Underfill 우측", "10:36:18"),
                new DefectRow(StatusKind.Alarm, "FA2611-0009", "와이어 본딩 단선", "Wire #2", "09:51:02"),
                new DefectRow(StatusKind.Alarm, "FA2611-0007", "박리", "Die Attach", "09:14:36"),
                new DefectRow(StatusKind.Warn, "FA2611-0005", "미세 크랙", "Substrate 하단", "08:42:50"),
                new DefectRow(StatusKind.Alarm, "FA2611-0004", "오픈", "Ball E15", "08:07:19"),
                new DefectRow(StatusKind.Alarm, "FA2611-0002", "솔더볼 크랙", "Ball A09", "07:33:41"),
                new DefectRow(StatusKind.Warn, "FA2611-0001", "보이드", "Die Attach", "07:02:58"),
                new DefectRow(StatusKind.Alarm, "FA2610-0098", "단락", "Wire #11", "06:21:05"),
                new DefectRow(StatusKind.Alarm, "FA2610-0095", "박리", "Mold 모서리", "05:48:33"),
                new DefectRow(StatusKind.Warn, "FA2610-0093", "미세 크랙", "Die 우하단", "05:09:47"),
            };
        }
    }
}
