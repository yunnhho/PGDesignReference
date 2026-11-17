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

        protected static int S(int px) { return (int)Math.Round(px * ApcTheme.Scale); }

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
            row.Controls.AddRange(controls);
            return row;
        }
    }
}
