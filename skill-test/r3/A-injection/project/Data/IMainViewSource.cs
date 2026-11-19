using System.Collections.Generic;

namespace InjectionMonitor.Data
{
    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(PLC, DB)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        IList<MachineInfo> GetMachines();

        /// <summary>설비의 금형 온도 추이(시간순, 마지막 점 = 현재값). 값이 없으면 빈 목록.</summary>
        IList<double> GetTempTrend(string machineId);

        /// <summary>추이 가로축 글자(양끝·가운데).</summary>
        string[] GetTrendLabels();

        IList<AlarmEntry> GetRecentAlarms();

        Recipe GetRecipe();

        void SaveRecipe(Recipe recipe);
    }
}
