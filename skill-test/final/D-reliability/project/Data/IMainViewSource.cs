using System.Collections.Generic;

namespace ReliabilityTester.Data
{
    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(챔버 PLC, 절연저항 측정기, DB)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        IList<ChannelRow> GetChannels();
        /// <summary>금일 불량 분류 결과, 최근 것이 위.</summary>
        IList<DefectRow> GetTodayDefects();
        ChamberDetail GetChamberDetail(string channel);
        /// <summary>시험 조건 설정 dialog에서 저장한 값을 넘긴다.</summary>
        void SaveCondition(string name, decimal temperature, decimal humidity, decimal hours, decimal cycles);
    }
}
