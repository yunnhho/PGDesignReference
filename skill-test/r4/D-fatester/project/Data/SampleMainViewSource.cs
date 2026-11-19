using System.Collections.Generic;
using APC.UI.Theming;

namespace FATester.Data
{
    /// <summary>화면 확인·render용 샘플 데이터.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        public IList<ChannelRow> GetChannels()
        {
            return new List<ChannelRow>
            {
                // 저항 90.0 = 경고 시작값(경계값 경고)
                new ChannelRow("CH1", "시험중", StatusKind.Run, null, "열충격", "FA-0412-018", 62, 90.0, 12.4,
                    -40.0, 12.0, 342, null, null, null),
                new ChannelRow("CH2", "완료", StatusKind.Run, null, "고온고습", "FA-0412-017", 100, 61.8, 38.2,
                    85.0, 85.0, 500, "E-110", "챔버 도어 열림", "2026-11-18 21:40:12"),
                new ChannelRow("CH3", "오류", StatusKind.Alarm, "전류 인가 실패", "전기특성", "FA-0412-019", 37, 71.2, 18.5,
                    25.0, 45.0, 128, "E-204", "전류 인가 실패", "2026-11-19 14:52:10"),
                new ChannelRow("CH4", "대기", StatusKind.Stop, null, "", "", null, null, null,
                    25.1, 45.3, 1240, null, null, null),
            };
        }

        public IList<DefectRow> GetDefects()
        {
            return new List<DefectRow>
            {
                new DefectRow("FA-0412-016", "와이어 본드 박리", "Wire #7 (리드 측)", "14:52:10"),
                new DefectRow("FA-0412-014", "솔더 볼 크랙", "Ball B12", "14:31:48"),
                new DefectRow("FA-0412-011", "델라미네이션", "다이 어태치 계면", "14:05:22"),
                new DefectRow("FA-0412-009", "보이드", "몰드 컴파운드 내부", "13:47:09"),
                new DefectRow("FA-0411-088", "접촉 불량", "Pin 24", "13:12:55"),
                new DefectRow("FA-0411-085", "누설전류 과다", "Pad C3", "12:38:31"),
                new DefectRow("FA-0411-081", "기판 균열", "패키지 모서리 (우하)", "12:02:17"),
                new DefectRow("FA-0411-077", "솔더 브리지", "Ball D05~D06", "11:26:40"),
                new DefectRow("FA-0411-072", "델라미네이션", "몰드/리드프레임 계면", "10:49:03"),
                new DefectRow("FA-0411-068", "와이어 단선", "Wire #3", "10:15:36"),
                new DefectRow("FA-0411-064", "보이드", "Ball A08", "09:41:12"),
                new DefectRow("FA-0411-059", "접촉 불량", "Pin 11", "09:08:54"),
                new DefectRow("FA-0411-054", "솔더 볼 크랙", "Ball F02", "08:36:27"),
                new DefectRow("FA-0411-049", "몰드 크랙", "상단 중앙부", "08:11:05"),
                new DefectRow("FA-0411-047", "누설전류 과다", "Pad A1", "07:44:50"),
                new DefectRow("FA-0411-041", "와이어 본드 박리", "Wire #12 (다이 측)", "07:19:33"),
                new DefectRow("FA-0411-038", "다이 크랙", "다이 좌상단 모서리", "06:52:18"),
                new DefectRow("FA-0411-033", "델라미네이션", "다이 패들 하부", "06:27:41"),
                new DefectRow("FA-0411-030", "보이드", "언더필 중앙", "06:03:09"),
            };
        }
    }
}
