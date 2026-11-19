namespace InjectionMonitor.Data
{
    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(PLC, DB, 시리얼)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        MachineInfo[] GetMachines();
        AlarmRecord[] GetAlarmHistory();
        /// <summary>레시피 저장(금형 온도 설정값 ℃, 사출 압력 상한 bar, 사이클 타임 목표 s).</summary>
        void SaveRecipe(decimal tempSetpoint, decimal pressureUpper, decimal cycleTarget);
    }
}
