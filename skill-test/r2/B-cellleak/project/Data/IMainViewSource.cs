using System.Collections.Generic;

namespace CellLeakTester.Data
{
    /// <summary>검사 한 건.</summary>
    internal sealed class LeakResult
    {
        public readonly string Barcode;
        public readonly string Time;
        public readonly double Leak;

        public LeakResult(string barcode, string time, double leak) { Barcode = barcode; Time = time; Leak = leak; }

        public bool IsOk { get { return LeakJudge.IsOk(Leak); } }
    }

    /// <summary>누설량 판정 하나. 상한과 같은 값은 OK, 초과부터 NG.</summary>
    internal static class LeakJudge
    {
        public const double Limit = 5.0;
        public static bool IsOk(double leak) { return leak <= Limit; }
    }

    /// <summary>메인 화면 데이터 자리. 실제 연결(PLC, DB)은 이 인터페이스를 구현해 넣는다.</summary>
    internal interface IMainViewSource
    {
        string CurrentLot { get; }
        string CurrentModel { get; }
        string CurrentOperator { get; }
        int TodayTotal { get; }
        int TodayNg { get; }
        /// <summary>최신 검사가 맨 앞. [0]이 현재 셀.</summary>
        IList<LeakResult> History { get; }
        bool CheckAdminPassword(string password);
    }
}
