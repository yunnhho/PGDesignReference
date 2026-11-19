namespace CleanroomMonitor.Data
{
    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(센서 수집, DB 저장)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        /// <summary>구역 8곳(A1~A4, B1~B4) 센서 현재값.</summary>
        ZoneReading[] GetReadings();

        /// <summary>구역별 상·하한 설정(GetReadings와 같은 순서).</summary>
        ZoneLimits[] GetLimits();

        /// <summary>데이터 저장 주기(s).</summary>
        int GetStorageIntervalSec();

        /// <summary>구역 온습도 추이(마지막 점 = 현재값). 통신이 끊긴 구역은 빈 배열.</summary>
        ZoneTrend GetTrend(string zone);

        /// <summary>설정 dialog에서 저장한 값 반영.</summary>
        void SaveSettings(ZoneLimits[] limits, int storageIntervalSec);
    }
}
