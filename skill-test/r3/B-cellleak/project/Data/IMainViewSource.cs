using System;
using System.Collections.Generic;

namespace CellLeakTester.Data
{
    /// <summary>판정 기준. 요구서의 상한 5.0 Pa/s 하나뿐이다(경고 구간 없음). 판정은 이 클래스 하나만 쓴다.</summary>
    internal static class LeakRules
    {
        /// <summary>누설량 상한(Pa/s). 같으면 OK, 초과부터 NG.</summary>
        public const double UpperLimit = 5.0;

        public static bool IsOk(double leak) { return Math.Round(leak, 1) <= UpperLimit; }

        public static string Format(double leak) { return leak.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture); }

        public static string LimitText { get { return Format(UpperLimit) + " 이하 OK"; } }

        public static string NgReason(double leak) { return "상한 초과 " + Format(leak) + " Pa/s (상한 " + Format(UpperLimit) + ")"; }
    }

    internal sealed class LotInfo
    {
        public LotInfo(string lot, string model, string worker) { Lot = lot; Model = model; Worker = worker; }
        public string Lot { get; private set; }
        public string Model { get; private set; }
        public string Worker { get; private set; }
    }

    internal sealed class InspectionRecord
    {
        public InspectionRecord(DateTime time, string barcode, double leak)
        {
            Time = time; Barcode = barcode; Leak = Math.Round(leak, 1);
        }
        public DateTime Time { get; private set; }
        public string Barcode { get; private set; }
        public double Leak { get; private set; }
        public bool Ok { get { return LeakRules.IsOk(Leak); } }
    }

    internal sealed class DailyCount
    {
        public DailyCount(int total, int ng) { Total = total; Ng = ng; }
        public int Total { get; private set; }
        public int Ng { get; private set; }
    }

    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(누설 시험기, DB)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        LotInfo GetLot();
        void SaveLot(LotInfo lot);
        DailyCount GetDaily();
        /// <summary>최근 검사 이력, 최신이 먼저.</summary>
        IList<InspectionRecord> GetHistory();
        /// <summary>바코드를 받아 검사한 결과(실제는 시험기에서 측정).</summary>
        InspectionRecord Inspect(string barcode);
        bool CheckAdminPassword(string password);
    }
}
