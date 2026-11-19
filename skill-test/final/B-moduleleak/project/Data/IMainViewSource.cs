using System.Collections.Generic;

namespace ModuleLeakTester.Data
{
    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(PLC, DB, 시리얼)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        IList<LeakModel> GetModels();

        /// <summary>최근 검사 이력, 최신이 앞.</summary>
        IList<HistoryRow> GetHistory(LeakModel model);

        int TodayOk { get; }
        int TodayNg { get; }

        /// <summary>스캔한 모듈을 시험해 측정값을 돌려준다(PLC 연동 자리).</summary>
        HistoryRow Test(string barcode, LeakModel model);

        bool VerifyAdminPassword(string password);
    }
}
