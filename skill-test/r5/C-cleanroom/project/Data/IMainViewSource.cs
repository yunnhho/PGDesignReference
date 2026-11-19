namespace CleanroomMonitor.Data
{
    /// <summary>메인 화면 데이터 자리. 실제 연결(센서·DB)은 이 인터페이스를 구현해 넣는다.</summary>
    internal interface IMainViewSource
    {
        ZoneReading[] GetZones();
        ZoneSettings GetSettings();
        ZoneTrend GetTrend(string zoneId);
        void SaveSettings(ZoneSettings settings);
    }
}
