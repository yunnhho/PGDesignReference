using System.Collections.Generic;

namespace FabEnvMonitor.Data
{
    /// <summary>
    /// 메인 화면 데이터 자리. 실제 연결(센서 게이트웨이, DB)은 이 인터페이스를 구현해 넣는다.
    /// </summary>
    internal interface IMainViewSource
    {
        /// <summary>베이 12곳의 현재값(BAY-01 ~ BAY-12 순).</summary>
        IList<BayReading> GetBays();

        /// <summary>베이별 허용 범위(GetBays와 같은 순서).</summary>
        IList<BaySetting> GetSettings();

        /// <summary>설정 저장(설정 dialog 확정 시).</summary>
        void SaveSettings(IList<BaySetting> settings);

        /// <summary>최근 이탈 이력, 최신순.</summary>
        IList<ExcursionRecord> GetRecentExcursions();

        /// <summary>최근 24시간 추이(1시간 간격 25점, 왼쪽=과거, 마지막 = 현재값). 온도 또는 습도.</summary>
        double[] GetTrend24h(string bay, EnvMetric metric);
    }
}
