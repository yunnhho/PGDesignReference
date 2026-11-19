namespace FATester.Data
{
    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(시험기 PLC, DB)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        /// <summary>시험 채널 CH1~CH4의 현재 상태.</summary>
        ChannelInfo[] GetChannels();

        /// <summary>불량 분류 결과(최신순).</summary>
        DefectRow[] GetDefects();
    }
}
