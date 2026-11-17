using APC.UI.Controls;
using __NAME__.Data;

namespace __NAME__.Views
{
    /// <summary>메인 화면. 데이터는 IMainViewSource에서만 받는다(기본: 샘플 데이터).</summary>
    internal sealed class MainView : ApcTemplateView
    {
        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("__NAME__")
        {
            // TODO: source에서 값을 받아 Body에 Apc 컨트롤로 배치한다(skills/apc-field-ui/SKILL.md 4단계 컨트롤 표, 데모 Templates 참고).
            //       Fill 컨트롤을 먼저 Add하고, Top 줄은 화면 아래쪽 것부터 Add한다. 크기는 S(px).
        }
    }
}
