using System;

namespace FATester.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 값은 메인 화면·상세 dialog·불량 표가 서로 맞는다.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        // 챔버 기준(요구서에 없어 정함): 설정값, 경고 시작, 상한
        private static readonly Limit ShockTemp = new Limit("챔버 온도", "℃", "0.0", 128.0, 130.0, 125.0);
        private static readonly Limit ShockHumi = new Limit("챔버 습도", "%RH", "0.0", 35.0, 40.0, 20.0);
        private static readonly Limit HotHumidTemp = new Limit("챔버 온도", "℃", "0.0", 87.0, 88.0, 85.0);
        private static readonly Limit HotHumidHumi = new Limit("챔버 습도", "%RH", "0.0", 88.0, 90.0, 85.0);
        private static readonly Limit ElecTemp = new Limit("챔버 온도", "℃", "0.0", 28.0, 30.0, 25.0);
        private static readonly Limit ElecHumi = new Limit("챔버 습도", "%RH", "0.0", 55.0, 60.0, 45.0);

        public ChannelInfo[] GetChannels()
        {
            return new[]
            {
                new ChannelInfo("CH1", ProcessState.Running, "FA-2611-0012", "열충격", 62.0, 13.5, 18.2,
                    ShockTemp, 125.4, ShockHumi, 21.5, 312, "E-104", "챔버 도어 열림", "2026-11-18 16:20:41", Trend(125.0, 125.4)),
                new ChannelInfo("CH2", ProcessState.Done, "FA-2611-0009", "고온고습", 100.0, 11.8, 50.0,
                    HotHumidTemp, 85.2, HotHumidHumi, 85.4, 1000, null, null, null, Trend(85.0, 85.2)),
                new ChannelInfo("CH3", ProcessState.Error, "FA-2611-0021", "전기특성", 35.0, 16.2, 38.4,
                    ElecTemp, 25.3, ElecHumi, 45.0, 148, "E-203", "인가 실패", "2026-11-19 09:42:15", Trend(25.0, 25.3)),
                new ChannelInfo("CH4", ProcessState.Idle, null, null, null, null, null,
                    ElecTemp, 24.8, ElecHumi, 44.0, 1240, "E-117", "시료 감지 안 됨", "2026-11-17 18:03:09", Trend(25.0, 24.8)),
            };
        }

        public DefectRow[] GetDefects()
        {
            return new[]
            {
                new DefectRow("FA-2611-0021", "인가 실패 후 저항 초과", "패드 B3", new DateTime(2026, 11, 19, 9, 42, 15)),
                new DefectRow("FA-2611-0009", "누설전류 한계", "리드 4번", new DateTime(2026, 11, 19, 8, 55, 40)),
                new DefectRow("FA-2611-0007", "와이어 본드 단선", "Die 좌상단", new DateTime(2026, 11, 18, 17, 31, 2)),
                new DefectRow("FA-2611-0005", "층간 박리", "몰드 가장자리", new DateTime(2026, 11, 18, 15, 12, 48)),
                new DefectRow("FA-2611-0004", "솔더 크랙", "볼 C5", new DateTime(2026, 11, 18, 13, 47, 19)),
                new DefectRow("FA-2611-0003", "보이드", "Die Attach 중앙", new DateTime(2026, 11, 18, 10, 6, 33)),
                new DefectRow("FA-2611-0002", "저항 상승", "패드 A1", new DateTime(2026, 11, 17, 16, 58, 7)),
                new DefectRow("FA-2611-0001", "패키지 균열", "우측 모서리", new DateTime(2026, 11, 17, 14, 21, 54)),
                new DefectRow("FA-2610-0098", "와이어 본드 단선", "Die 우하단", new DateTime(2026, 11, 17, 11, 40, 26)),
                new DefectRow("FA-2610-0095", "누설전류 초과", "리드 2번", new DateTime(2026, 11, 16, 17, 9, 51)),
                new DefectRow("FA-2610-0091", "층간 박리", "Die 하단 경계", new DateTime(2026, 11, 16, 13, 52, 38)),
                new DefectRow("FA-2610-0089", "솔더 크랙", "볼 A2", new DateTime(2026, 11, 16, 9, 15, 4)),
                new DefectRow("FA-2610-0086", "보이드", "리드 프레임 중앙", new DateTime(2026, 11, 15, 16, 44, 29)),
                new DefectRow("FA-2610-0083", "저항 상승", "패드 D4", new DateTime(2026, 11, 15, 11, 3, 57)),
            };
        }

        /// <summary>설정값에서 출발해 마지막 점이 현재값이 되는 추이(31점).</summary>
        private static double[] Trend(double setPoint, double current)
        {
            const int n = 31;
            var v = new double[n];
            for (int i = 0; i < n; i++)
            {
                double t = i / (double)(n - 1);
                v[i] = setPoint + (current - setPoint) * t + Math.Sin(i * 0.9) * 0.4 * (1 - t);
            }
            return v;
        }
    }
}
