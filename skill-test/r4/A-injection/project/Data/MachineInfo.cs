using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    /// <summary>사출기 한 대의 현재 읽은 값. 값이 없으면(대기·통신 전) null.</summary>
    internal sealed class MachineInfo
    {
        public MachineInfo(string id, string process, double? temp, double? pressure, double? cycle, double[] trend)
        {
            Id = id; Process = process; Temp = temp; Pressure = pressure; Cycle = cycle; Trend = trend;
        }

        public string Id { get; private set; }
        /// <summary>공정 상태: 가동 / 대기 / 정지</summary>
        public string Process { get; private set; }
        public double? Temp { get; private set; }       // ℃
        public double? Pressure { get; private set; }   // bar
        public double? Cycle { get; private set; }      // s
        /// <summary>금형 온도 추이(5분 간격, 마지막 점 = 현재값)</summary>
        public double[] Trend { get; private set; }
    }

    internal sealed class AlarmRecord
    {
        public AlarmRecord(string time, StatusKind status, string machine, string text)
        {
            Time = time; Status = status; Machine = machine; Text = text;
        }

        public string Time { get; private set; }
        public StatusKind Status { get; private set; }
        public string Machine { get; private set; }
        public string Text { get; private set; }
    }
}
