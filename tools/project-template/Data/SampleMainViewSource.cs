using System.Collections.Generic;

namespace __NAME__.Data
{
    /// <summary>화면 확인용 샘플 데이터.</summary>
    internal sealed class SampleMainViewSource : IMainViewSource
    {
        public IList<string[]> GetRows()
        {
            return new List<string[]>
            {
                new[] { "설비 01", "가동", "92%" },
                new[] { "설비 02", "경고", "67%" },
                new[] { "설비 03", "정지", "0%" },
            };
        }
    }
}
