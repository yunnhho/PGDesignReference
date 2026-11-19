namespace InjectionMonitor.Data
{
    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(PLC, DB, 시리얼)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        MachineInfo[] GetMachines();
        /// <summary>그 설비의 최근 금형 온도 추이(시간순, 왼쪽=과거). 읽은 값이 없으면 빈 배열.</summary>
        double[] GetMoldTempTrend(string machineId);
        /// <summary>추이 가로축 글자(양끝·가운데 등).</summary>
        string[] GetTrendLabels();
        AlarmRow[] GetRecentAlarms();
        RecipeValues GetRecipe();
        void SaveRecipe(RecipeValues recipe);
    }
}
