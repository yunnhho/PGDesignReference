using APC.UI.Theming;

namespace InjectionMonitor.Data
{
    /// <summary>화면 확인·render용 샘플 데이터. 값·시각은 Judge 기준과 서로 맞춘다.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        public MachineInfo[] GetMachines()
        {
            return new[]
            {
                new MachineInfo("IM-01", "가동", 60.2, 98.5, 24.8, new[] { 59.8, 60.1, 60.3, 59.9, 60.0, 60.4, 60.1, 59.7, 60.0, 60.3, 59.9, 60.2 }),
                new MachineInfo("IM-02", "가동", 59.8, 101.2, 25.1, new[] { 60.2, 60.0, 59.6, 59.9, 60.1, 60.3, 60.0, 59.8, 59.5, 59.9, 60.1, 59.8 }),
                new MachineInfo("IM-03", "가동", 63.0, 105.0, 25.4, new[] { 60.4, 60.6, 60.9, 61.2, 61.0, 61.5, 61.9, 62.2, 62.0, 62.5, 62.8, 63.0 }),
                new MachineInfo("IM-04", "가동", 65.8, 118.0, 25.9, new[] { 61.0, 61.8, 62.4, 63.1, 63.9, 64.2, 64.8, 65.0, 65.3, 65.6, 65.4, 65.8 }),
                new MachineInfo("IM-05", "대기", null, null, null, new double[0]),
                new MachineInfo("IM-06", "정지", 57.5, null, null, new[] { 60.1, 60.0, 59.8, 59.5, 59.1, 58.9, 58.6, 58.4, 58.1, 57.9, 57.7, 57.5 }),
            };
        }

        public AlarmRecord[] GetAlarmHistory()
        {
            return new[]
            {
                new AlarmRecord("14:50:05", StatusKind.Warn, "IM-03", "금형 온도 상한 근접 63.0 ℃ (상한 65.0)"),
                new AlarmRecord("14:35:20", StatusKind.Alarm, "IM-04", "금형 온도 상한 초과 65.3 ℃ (상한 65.0)"),
                new AlarmRecord("14:10:30", StatusKind.Warn, "IM-04", "금형 온도 상한 근접 63.1 ℃ (상한 65.0)"),
                new AlarmRecord("13:58:12", StatusKind.Stop, "IM-06", "계획 정지"),
                new AlarmRecord("13:41:05", StatusKind.Warn, "IM-02", "사출 압력 상한 근접 114.0 bar (상한 120.0)"),
                new AlarmRecord("13:26:48", StatusKind.Run, "IM-02", "사출 압력 정상 복귀 108.4 bar"),
                new AlarmRecord("13:12:30", StatusKind.Info, "IM-02", "레시피 변경 금형 온도 설정 58.0 → 60.0 ℃"),
                new AlarmRecord("13:06:44", StatusKind.Run, "IM-01", "사이클 타임 정상 복귀 25.9 s"),
                new AlarmRecord("12:58:19", StatusKind.Alarm, "IM-01", "사이클 타임 상한 초과 28.4 s (상한 28)"),
                new AlarmRecord("12:49:03", StatusKind.Warn, "IM-01", "사이클 타임 상한 근접 28.0 s (상한 28)"),
                new AlarmRecord("11:47:15", StatusKind.Run, "IM-06", "금형 온도 정상 복귀 59.2 ℃"),
                new AlarmRecord("11:31:08", StatusKind.Alarm, "IM-06", "금형 온도 하한 미만 54.6 ℃ (하한 55.0)"),
                new AlarmRecord("11:20:33", StatusKind.Warn, "IM-06", "금형 온도 하한 근접 56.0 ℃ (하한 55.0)"),
            };
        }

        public void SaveRecipe(decimal tempSetpoint, decimal pressureUpper, decimal cycleTarget)
        {
            // 샘플: 저장하지 않는다. 실제 구현은 PLC/DB에 쓴다.
        }
    }
}
