using System;
using System.Collections.Generic;
using APC.UI.Theming;

namespace ReliabilityTester.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 값은 알람 문구·상세 dialog·불량 표와 서로 맞춘다.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        private const int TrendPoints = 61;

        private static readonly ChannelRow[] Channels =
        {
            new ChannelRow("CH1", Criteria.Testing, "PKG-2611-001", "HAST", 62.0, 2180, 850.0, 0.8, null),
            new ChannelRow("CH2", Criteria.Testing, "PKG-2611-014", "TC", 35.0, 7200, 120.0, 2.1, null),
            new ChannelRow("CH3", Criteria.Testing, "PKG-2611-022", "HTOL", 48.0, 31200, 85.0, 5.0, null),
            new ChannelRow("CH4", Criteria.Error, "PKG-2611-037", "HAST", 12.0, null, 640.0, 1.2, "인가 실패 (E-204)"),
            new ChannelRow("CH5", Criteria.Done, "PKG-2611-009", "HAST", 100.0, 0, 910.0, 0.6, null),
            new ChannelRow("CH6", Criteria.Standby, "", "", null, null, null, null, null),
        };

        // 채널, 시료 ID, 불량 모드, 판정 시각 — 최근 것이 위. CH3 PKG-2611-022는 지금 NG 판정(알람)과 같은 건.
        private static readonly DefectRow[] Defects =
        {
            new DefectRow("CH3", "PKG-2611-022", "절연 열화", "14:32:10"),
            new DefectRow("CH2", "PKG-2611-011", "박리(Delamination)", "13:48:25"),
            new DefectRow("CH5", "PKG-2611-008", "누설전류 초과", "13:05:41"),
            new DefectRow("CH1", "PKG-2611-002", "와이어 오픈", "12:17:08"),
            new DefectRow("CH4", "PKG-2611-036", "수분 침투", "11:52:56"),
            new DefectRow("CH2", "PKG-2611-010", "크랙", "11:09:33"),
            new DefectRow("CH3", "PKG-2611-021", "솔더볼 단락", "10:26:19"),
            new DefectRow("CH6", "PKG-2611-030", "절연 열화", "09:44:02"),
            new DefectRow("CH5", "PKG-2611-007", "박리(Delamination)", "09:03:47"),
            new DefectRow("CH1", "PKG-2611-004", "팝콘 현상", "08:31:15"),
            new DefectRow("CH4", "PKG-2611-035", "누설전류 초과", "08:12:50"),
            new DefectRow("CH2", "PKG-2611-013", "와이어 오픈", "07:58:04"),
            new DefectRow("CH6", "PKG-2611-029", "크랙", "07:40:37"),
            new DefectRow("CH3", "PKG-2611-020", "수분 침투", "07:22:11"),
            new DefectRow("CH5", "PKG-2611-006", "솔더볼 단락", "07:05:29"),
            new DefectRow("CH1", "PKG-2611-003", "절연 열화", "06:51:43"),
            new DefectRow("CH4", "PKG-2611-034", "박리(Delamination)", "06:37:08"),
            new DefectRow("CH2", "PKG-2611-012", "팝콘 현상", "06:15:52"),
        };

        public IList<ChannelRow> GetChannels() { return Channels; }

        public IList<DefectRow> GetTodayDefects() { return Defects; }

        public void SaveCondition(string name, decimal temperature, decimal humidity, decimal hours, decimal cycles)
        {
            // 샘플: 저장하지 않는다.
        }

        public ChamberDetail GetChamberDetail(string channel)
        {
            ChannelRow row = Channels[0];
            foreach (var c in Channels) if (c.Name == channel) row = c;

            double tSet, tUp, hSet, hUp;
            switch (row.Condition)
            {
                case "HAST": tSet = 130.0; tUp = 132.0; hSet = 85.0; hUp = 90.0; break;
                case "TC": tSet = 100.0; tUp = 105.0; hSet = 30.0; hUp = 40.0; break;
                case "HTOL": tSet = 125.0; tUp = 127.0; hSet = 45.0; hUp = 50.0; break;
                default: tSet = 25.0; tUp = 30.0; hSet = 40.0; hUp = 60.0; break;   // 대기: 상온
            }
            var temp = Trend(tSet, 0.6, 0.0);
            var humi = Trend(hSet, 1.2, 1.5);
            double tNow = temp[TrendPoints - 1], hNow = humi[TrendPoints - 1];   // 추이 마지막 점 = 현재값

            bool judged = row.State == Criteria.Testing || row.State == Criteria.Done;
            var tStatus = judged ? (tNow > tUp ? StatusKind.Alarm : StatusKind.Run) : StatusKind.Stop;
            var hStatus = judged ? (hNow > hUp ? StatusKind.Alarm : StatusKind.Run) : StatusKind.Stop;

            return new ChamberDetail(row.Name, Criteria.ChannelStatus(row), Criteria.StatusText(row),
                temp, humi, tSet, tUp, hSet, hUp, tStatus, hStatus, Errors(row.Name));
        }

        private static double[] Trend(double center, double amplitude, double phase)
        {
            var v = new double[TrendPoints];
            for (int i = 0; i < TrendPoints; i++) v[i] = Math.Round(center + amplitude * Math.Sin(i / 6.0 + phase), 1);
            return v;
        }

        // { StatusKind, 시각, 코드, 내용 } — 최근 것이 위, 이상 → 복귀는 복귀가 더 늦게
        private static object[][] Errors(string channel)
        {
            switch (channel)
            {
                case "CH1":
                    return new[] { new object[] { StatusKind.Warn, "10:12:44", "E-052", "누설전류 한계 근접 4.0 µA (경고 4.0 이상)" } };
                case "CH2":
                    return new[]
                    {
                        new object[] { StatusKind.Warn, "13:30:18", "E-041", "절연저항 한계 근접 120.0 MΩ (경고 120.0 이하)" },
                        new object[] { StatusKind.Run, "09:40:02", "E-030", "챔버 온도 정상 복귀 100.2 ℃" },
                        new object[] { StatusKind.Alarm, "09:33:51", "E-030", "챔버 온도 상한 초과 105.4 ℃ (상한 105.0)" },
                    };
                case "CH3":
                    return new[]
                    {
                        new object[] { StatusKind.Alarm, "14:32:10", "E-117", "절연저항 하한 미만 85.0 MΩ (하한 100.0)" },
                        new object[] { StatusKind.Warn, "12:48:05", "E-053", "누설전류 한계 근접 5.0 µA (경고 4.0 이상)" },
                        new object[] { StatusKind.Run, "09:21:07", "E-031", "챔버 온도 정상 복귀 125.2 ℃" },
                        new object[] { StatusKind.Alarm, "09:15:30", "E-031", "챔버 온도 상한 초과 127.4 ℃ (상한 127.0)" },
                    };
                case "CH4":
                    return new[]
                    {
                        new object[] { StatusKind.Alarm, "14:05:36", "E-204", "바이어스 인가 실패 (재시도 3회)" },
                        new object[] { StatusKind.Warn, "13:41:20", "E-060", "바이어스 전류 불안정" },
                    };
                case "CH5":
                    return new[] { new object[] { StatusKind.Run, "13:58:00", "E-000", "시험 정상 완료" } };
                default:
                    return new[] { new object[] { StatusKind.Warn, "08:40:12", "E-044", "챔버 도어 열림 감지" } };
            }
        }
    }
}
