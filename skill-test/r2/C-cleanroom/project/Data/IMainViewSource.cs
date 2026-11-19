using System.Collections.Generic;

namespace CleanroomMonitor.Data
{
    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(센서 수집, DB, 설정 저장)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        IList<ZoneReading> GetReadings();
        IList<ZoneLimit> GetLimits();
        int GetSaveIntervalSec();

        /// <summary>선택 구역의 최근 추이(과거 → 현재, 마지막 점 = 현재값). 값이 없으면 빈 목록.</summary>
        IList<double> GetTemperatureTrend(string zone);
        IList<double> GetHumidityTrend(string zone);
        string[] GetTrendTimeLabels();

        void SaveSettings(IList<ZoneLimit> limits, int saveIntervalSec);
    }
}
