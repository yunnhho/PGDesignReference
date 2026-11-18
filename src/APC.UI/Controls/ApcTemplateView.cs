using System;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Theming;

namespace APC.UI.Controls
{
    /// <summary>모든 기능 화면의 공통 틀: 상단 ApcHeaderBar, 하단 ApcAlarmBar, 가운데 Body.</summary>
    public class ApcTemplateView : UserControl
    {
        protected readonly ApcHeaderBar Header = new ApcHeaderBar();
        protected readonly ApcAlarmBar Alarm = new ApcAlarmBar();
        /// <summary>
        /// 내용 영역(Padding 16px). 도킹 순서: 같은 Dock(Top 등)끼리는 나중에 Add한 것이 바깥(위)에 온다.
        /// Fill을 먼저 Add하고, Top 줄들은 화면 아래쪽 것부터 위쪽 것 순서로 Add한다(또는 SetChildIndex(fill, 0)).
        /// </summary>
        protected readonly Panel Body = new Panel { Dock = DockStyle.Fill };

        protected ApcTemplateView(string title)
        {
            Dock = DockStyle.Fill;
            BackColor = ApcTheme.C(ApcTheme.Current.Colors.Background[0]);
            Header.Title = title;
            Header.Status = StatusKind.Run;
            Header.StatusText = "가동중";
            Body.Padding = new Padding(S(16));
            Controls.Add(Body);     // Fill은 마지막에 도킹되도록 먼저 추가
            Controls.Add(Alarm);
            Controls.Add(Header);
        }

        /// <summary>배율 적용 px. ApcForm dialog 등 이 클래스 밖에서는 ApcTheme.Px.</summary>
        protected static int S(int px) { return ApcTheme.Px(px); }

        protected static Label Caption(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = ApcTheme.GetFont(FontStyle.Bold),
                ForeColor = ApcTheme.C(ApcTheme.Current.Colors.Text),
                Margin = new Padding(0, S(8), S(6), 0),
                BackColor = Color.Transparent,
            };
        }

        protected static ApcButton Button(string text, ApcButtonVariant variant, int width = 110)
        {
            return new ApcButton { Text = text, Variant = variant, Width = S(width), Height = S(34), Margin = new Padding(0, 0, S(8), 0) };
        }

        /// <summary>왼쪽→오른쪽으로 쌓는 한 줄(필터·버튼 줄).</summary>
        protected static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, S(10)),
                BackColor = Color.Transparent,
            };
            // 입력칸(기본 Margin 3)과 버튼(Margin 0)이 섞이면 위아래가 어긋난다 → 입력·버튼은 위 0, 오른쪽 간격만 둔다. 이름표는 자기 Margin(글자 높이 맞춤) 유지.
            foreach (var c in controls)
                if (!(c is Label)) c.Margin = new Padding(0, 0, Math.Max(c.Margin.Right, S(8)), 0);
            row.Controls.AddRange(controls);
            return row;
        }
    }
}
