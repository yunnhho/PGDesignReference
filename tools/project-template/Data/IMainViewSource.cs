using System.Collections.Generic;

namespace __NAME__.Data
{
    /// <summary>메인 화면 데이터 자리. 실제 연결(PLC, DB, 시리얼)은 이 인터페이스를 구현해 넣는다.</summary>
    internal interface IMainViewSource
    {
        IList<string[]> GetRows();
    }
}
