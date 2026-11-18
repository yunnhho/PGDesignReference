using APC.UI.Controls;

namespace APC.UI.Theming
{
    /// <summary>
    /// 화면·dialog 공통 치수(배율 전 px, 쓸 때 ApcTheme.Px). ApcTemplateView·ApcForm dialog 헬퍼가 쓰고,
    /// APC.UI.Tools의 LayoutLint가 같은 값으로 점검한다(값을 바꾸면 LayoutLint 상수도 함께).
    /// </summary>
    public static class ApcLayout
    {
        /// <summary>dialog·화면 바깥 여백.</summary>
        public const int Pad = 16;
        /// <summary>입력 행 사이, 메인 화면 줄 사이.</summary>
        public const int RowGap = 12;
        /// <summary>묶음(ApcGroup)·블록 사이, 내용과 하단 버튼 줄 사이.</summary>
        public const int GroupGap = 16;
        /// <summary>이름표 열과 입력칸 사이.</summary>
        public const int LabelGap = 12;
        /// <summary>한 줄 안 요소 사이(버튼·입력칸).</summary>
        public const int ItemGap = 8;
        public const int FooterButtonWidth = 110;
        public const int FooterButtonHeight = 40;
        /// <summary>입력칸·이름표 높이.</summary>
        public const int FieldHeight = ApcTextBox.BaseHeight;
    }
}
