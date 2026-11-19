using System.Collections.Generic;

namespace FATester.Data
{
    /// <summary>화면 확인·render용 샘플 데이터.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        public IList<ChannelInfo> GetChannels()
        {
            return new List<ChannelInfo>
            {
                // 저항 45.0 = 경고 시작값(경고), 누설 62.4 정상
                new ChannelInfo("CH1", ChannelProcess.Testing, "FA-261119-031", "열충격", 62, 45.0, 62.4, "E-110", "챔버 도어 열림", "2026-11-18 21:40:12", 125.0, 10.0, 312),
                // 누설전류 100.0 = 상한(초과 아님) → 경고
                new ChannelInfo("CH2", ChannelProcess.Done, "FA-261119-028", "고온고습", 100, 38.2, 100.0, "E-207", "습도 센서 편차", "2026-11-17 14:05:31", 85.0, 85.0, 1000),
                new ChannelInfo("CH3", ChannelProcess.Error, "FA-261119-030", "전기특성", 37, 52.3, 71.8, "E-203", "접촉 불량", "2026-11-19 10:52:08", 25.0, 45.0, 48),
                new ChannelInfo("CH4", ChannelProcess.Waiting, "", "", null, null, null, "E-110", "챔버 도어 열림", "2026-11-18 09:12:40", 25.0, 35.0, 150),
            };
        }

        public IList<DefectRow> GetDefects()
        {
            return new List<DefectRow>
            {
                new DefectRow("FA-261119-027", "와이어 본딩 단선", "Wire #7", "11:02:45"),
                new DefectRow("FA-261119-026", "박리(Delamination)", "Die 모서리", "10:48:10"),
                new DefectRow("FA-261119-024", "크랙", "몰드 상단", "10:21:37"),
                new DefectRow("FA-261119-023", "보이드", "Solder Ball C4", "09:57:02"),
                new DefectRow("FA-261119-021", "패드 부식", "Bond Pad B12", "09:30:55"),
                new DefectRow("FA-261119-019", "와이어 본딩 단선", "Wire #12", "09:05:18"),
                new DefectRow("FA-261119-018", "솔더 볼 균열", "Solder Ball A2", "08:44:40"),
                new DefectRow("FA-261119-016", "박리(Delamination)", "리드프레임 하단", "08:19:26"),
                new DefectRow("FA-261119-015", "크랙", "Die 중앙", "08:02:09"),
                new DefectRow("FA-261119-013", "보이드", "언더필", "07:41:51"),
                new DefectRow("FA-261119-011", "솔더 볼 균열", "Solder Ball D7", "07:12:33"),
                new DefectRow("FA-261119-010", "패드 부식", "Bond Pad A03", "06:58:04"),
                new DefectRow("FA-261119-008", "크랙", "기판 모서리", "06:20:47"),
            };
        }
    }
}
