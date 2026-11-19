using System;
using System.Collections.Generic;

namespace CellLeakTester.Data
{
    /// <summary>검사 한 건(값 객체).</summary>
    internal sealed class InspectionRecord
    {
        public InspectionRecord(DateTime time, string barcode, double leak, bool ok)
        {
            Time = time; Barcode = barcode; Leak = leak; Ok = ok;
        }

        public DateTime Time { get; private set; }
        public string Barcode { get; private set; }
        /// <summary>누설량(Pa/s)</summary>
        public double Leak { get; private set; }
        public bool Ok { get; private set; }

        /// <summary>판정 함수 하나: 상한과 같은 값은 OK.</summary>
        public static bool Judge(double leak, double limit) { return leak <= limit; }
    }

    /// <summary>현재 작업 LOT(LOT 입력 dialog 결과).</summary>
    internal sealed class LotInfo
    {
        public LotInfo(string lot, string model, string worker)
        {
            Lot = lot; Model = model; Worker = worker;
        }

        public string Lot { get; private set; }
        public string Model { get; private set; }
        public string Worker { get; private set; }
    }

    /// <summary>금일 집계.</summary>
    internal sealed class DailyCount
    {
        public DailyCount(int total, int ng) { Total = total; Ng = ng; }
        public int Total { get; private set; }
        public int Ng { get; private set; }
        /// <summary>수율 % = OK / 생산</summary>
        public double YieldPercent { get { return Total == 0 ? 0 : 100.0 * (Total - Ng) / Total; } }
    }

    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(PLC, DB, 시리얼)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        /// <summary>누설량 상한(Pa/s). 이 값 이하 OK.</summary>
        double LeakLimit { get; }
        LotInfo GetLot();
        void SetLot(LotInfo lot);
        DailyCount GetToday();
        /// <summary>최근 검사 이력, 최신이 앞.</summary>
        IList<InspectionRecord> GetHistory();
        /// <summary>스캔된 셀을 검사하고 결과를 이력·집계에 반영한다.</summary>
        InspectionRecord Inspect(string barcode);
    }
}
