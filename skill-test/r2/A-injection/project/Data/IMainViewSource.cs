using System.Collections.Generic;

namespace InjectionMonitor.Data
{
    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(PLC, DB)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        Recipe GetRecipe();
        void SaveRecipe(Recipe recipe);
        IList<Machine> GetMachines();
        /// <summary>최근 1시간 금형 온도(왼쪽=과거, 마지막 = 현재값). 값 없으면 빈 배열.</summary>
        double[] GetTempTrend(string machineId);
        IList<AlarmRow> GetAlarms();
    }
}
