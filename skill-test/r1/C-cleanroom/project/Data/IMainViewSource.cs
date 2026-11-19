using System.Collections.Generic;

namespace CleanroomMonitor.Data
{
    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(센서 게이트웨이, DB)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        IList<ZoneReading> GetZones();
        IList<ZoneLimit> GetLimits();
        /// <summary>구역의 최근 60분 추이(1분 간격).</summary>
        ZoneTrend GetTrend(string zone);
        /// <summary>데이터 저장 주기(초).</summary>
        int GetSaveIntervalSec();
        void SaveSettings(IList<ZoneLimit> limits, int saveIntervalSec);
    }
}
