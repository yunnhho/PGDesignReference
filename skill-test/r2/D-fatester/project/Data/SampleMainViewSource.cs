using System;
using APC.UI.Theming;

namespace FATester.Data
{
    /// <summary>화면 확인·render용 샘플 데이터.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        public ChannelInfo[] GetChannels()
        {
            return new[]
            {
                // CH1: 열충격 시험중, 저항이 경고 시작값(45.0)과 같음 → 경고
                new ChannelInfo("CH1", "시험중", StatusKind.Run, 62.0, "FA-1119-017", "열충격", 45.0, 35.0, "",
                    new ChannelDetail(125.0, 125.0, 130.0, 40.0, 40.0, 60.0, 412, "E-117", "복구됨 · 09:12:44", StatusKind.Info, Trend(125.0))),
                // CH2: 고온고습 완료, 최종 측정값 유지
                new ChannelInfo("CH2", "완료", StatusKind.Run, 100.0, "FA-1119-016", "고온고습", 38.6, 21.3, "",
                    new ChannelDetail(25.0, 25.0, 30.0, 45.0, 45.0, 60.0, 1000, "", "", StatusKind.Stop, Trend(25.0))),
                // CH3: 전기특성 오류(인가 실패) → 읽은 값 없음
                new ChannelInfo("CH3", "오류", StatusKind.Alarm, 12.0, "FA-1119-018", "전기특성", null, null, "인가 실패 (E-304)",
                    new ChannelDetail(25.0, 25.0, 30.0, 45.0, 45.0, 60.0, 0, "E-304", "인가 실패 · 14:52:10", StatusKind.Alarm, Trend(25.0))),
                // CH4: 대기
                new ChannelInfo("CH4", "대기", StatusKind.Stop, null, "", "", null, null, "",
                    new ChannelDetail(24.6, 25.0, 30.0, 47.0, 45.0, 60.0, 1248, "", "", StatusKind.Stop, Trend(24.6))),
            };
        }

        public DefectRow[] GetDefects()
        {
            return new[]
            {
                new DefectRow("FA-1119-015", "박리", "Die 모서리", "14:48:21"),
                new DefectRow("FA-1119-014", "와이어 본드 단선", "Pad B12", "14:21:07"),
                new DefectRow("FA-1119-012", "크랙", "패키지 상단 좌측", "13:55:40"),
                new DefectRow("FA-1119-011", "보이드", "Die Attach 중앙", "13:12:09"),
                new DefectRow("FA-1119-010", "와이어 본드 단선", "Pad A07", "12:58:47"),
                new DefectRow("FA-1119-009", "솔더 볼 균열", "Ball A3", "12:40:52"),
                new DefectRow("FA-1119-008", "누설전류 과다", "Pin 7", "11:58:33"),
                new DefectRow("FA-1119-006", "박리", "리드프레임 하단", "11:03:16"),
                new DefectRow("FA-1119-005", "와이어 본드 단선", "Pad C04", "10:27:45"),
                new DefectRow("FA-1119-004", "몰드 보이드", "게이트 부근", "09:41:02"),
                new DefectRow("FA-1119-003", "박리", "Die 모서리 하단", "09:22:14"),
                new DefectRow("FA-1119-002", "솔더 볼 균열", "Ball D9", "09:05:30"),
                new DefectRow("FA-1119-001", "크랙", "Die 중앙", "08:33:18"),
            };
        }

        // 최근 1시간 챔버 온도: 마지막 점 = 현재값
        private static double[] Trend(double current)
        {
            var v = new double[61];
            for (int i = 0; i < 60; i++) v[i] = current + 0.8 * Math.Sin(i / 4.0);
            v[60] = current;
            return v;
        }
    }
}
