using System;
using System.Collections.Generic;

namespace CellLeakTester.Data
{
    /// <summary>LOT 정보(값 객체).</summary>
    internal sealed class LotInfo
    {
        public LotInfo(string lotNumber, string model, string worker) { LotNumber = lotNumber; Model = model; Worker = worker; }
        public readonly string LotNumber;
        public readonly string Model;
        public readonly string Worker;
    }

    /// <summary>검사 한 건(값 객체).</summary>
    internal sealed class InspectionRecord
    {
        public InspectionRecord(DateTime time, string barcode, double leak) { Time = time; Barcode = barcode; Leak = leak; }
        public readonly DateTime Time;
        public readonly string Barcode;
        public readonly double Leak;
    }

    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(PLC, DB, 시리얼)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        LotInfo GetLot();
        void SaveLot(LotInfo lot);
        /// <summary>오늘 검사 이력, 최신이 앞.</summary>
        IList<InspectionRecord> GetHistory();
        /// <summary>오늘 생산 수량(이력보다 많을 수 있다).</summary>
        int GetProducedCount();
        int GetNgCount();
        bool VerifyAdminPassword(string password);
    }
}
