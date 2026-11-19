using System.Collections.Generic;

namespace MoldingLineMonitor.Data
{
    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(PLC, DB)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        /// <summary>사출기 IM-01~IM-08 현재 상태.</summary>
        IList<MachineRow> GetMachines();

        /// <summary>선택 설비의 사출 압력 추이(시간순, 왼쪽=과거, 마지막 점 = 현재값). 값이 없으면 빈 배열.</summary>
        double[] GetPressureTrend(string machineName);

        /// <summary>금일 생산 수량.</summary>
        int GetProducedToday();

        /// <summary>금일 불량 수량.</summary>
        int GetDefectsToday();

        /// <summary>최근 알람 이력(최신순).</summary>
        IList<AlarmRow> GetAlarmHistory();
    }
}
