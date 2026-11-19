using System.Collections.Generic;

namespace CleanroomMonitor.Data
{
    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(PLC, DB, 시리얼)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        /// <summary>구역 8곳 현재값(A1~A4, B1~B4 순서).</summary>
        IList<ZoneReading> GetZones();

        /// <summary>구역의 최근 1시간 온습도 추이(마지막 점 = 현재값).</summary>
        ZoneTrend GetTrend(string zone);

        /// <summary>현재 설정(구역별 상·하한).</summary>
        IList<ZoneLimits> GetLimits();

        /// <summary>데이터 저장 주기(초).</summary>
        int GetSavePeriodSec();

        /// <summary>설정 dialog 확인 결과 저장.</summary>
        void SaveSettings(IList<ZoneLimits> limits, int savePeriodSec);
    }
}
