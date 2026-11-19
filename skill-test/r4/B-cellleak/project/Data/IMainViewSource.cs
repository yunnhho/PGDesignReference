using System;
using System.Collections.Generic;

namespace CellLeakTester.Data
{
    /// <summary>검사 이력 한 건(읽기 전용).</summary>
    internal sealed class InspectionRecord
    {
        public InspectionRecord(DateTime time, string barcode, decimal leak)
        {
            Time = time;
            Barcode = barcode;
            Leak = leak;
        }

        public DateTime Time { get; private set; }
        public string Barcode { get; private set; }
        /// <summary>누설량 (Pa/s)</summary>
        public decimal Leak { get; private set; }
    }

    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(PLC, 누설 검사기, DB)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        /// <summary>현재 검사 중인 셀(가장 최근 검사). 이력의 첫 행과 같다.</summary>
        InspectionRecord CurrentCell { get; }
        string CurrentLot { get; }
        string CurrentModel { get; }
        string CurrentWorker { get; }
        int TodayOk { get; }
        int TodayNg { get; }
        /// <summary>최근 검사 이력(최신이 먼저).</summary>
        IList<InspectionRecord> RecentHistory { get; }

        void SaveLot(string lot, string model, string worker);
        bool VerifyAdminPassword(string password);
    }
}
