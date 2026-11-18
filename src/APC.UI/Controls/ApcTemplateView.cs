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
            Body.Padding = new Padding(S(ApcLayout.Pad));
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
            return new ApcButton { Text = text, Variant = variant, Width = S(width), Height = S(ApcLayout.FieldHeight), Margin = new Padding(0, 0, S(ApcLayout.ItemGap), 0) };
        }

        /// <summary>
        /// 카드·타일·판정판을 한 줄에 나란히(Dock=Top). percents가 없으면 같은 폭. 칸 간격 12px(마지막 칸 0), 줄 아래 간격 12px.
        /// height는 칸 높이(배율 적용한 값, 예: S(150)).
        /// </summary>
        protected static TableLayoutPanel Columns(int height, Control[] cells, float[] percents = null)
        {
            if (cells == null || cells.Length == 0) throw new ArgumentException("cells가 비었습니다.", "cells");
            if (percents != null && percents.Length != cells.Length) throw new ArgumentException("percents 개수가 cells와 다릅니다.", "percents");
            var t = new TableLayoutPanel { Dock = DockStyle.Top, Height = height + S(ApcLayout.RowGap), ColumnCount = cells.Length, RowCount = 1, Padding = new Padding(0, 0, 0, S(ApcLayout.RowGap)), BackColor = Color.Transparent };
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            for (int i = 0; i < cells.Length; i++)
            {
                t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, percents == null ? 100f / cells.Length : percents[i]));
                cells[i].Dock = DockStyle.Fill;
                cells[i].Margin = new Padding(0, 0, i == cells.Length - 1 ? 0 : S(ApcLayout.RowGap), 0);
                t.Controls.Add(cells[i], i, 0);
            }
            return t;
        }

        /// <summary>왼쪽→오른쪽으로 쌓는 한 줄(필터·버튼 줄).</summary>
        protected static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, S(ApcLayout.RowGap)),
                BackColor = Color.Transparent,
            };
            // 입력칸(기본 Margin 3)과 버튼(Margin 0)이 섞이면 위아래가 어긋난다 → 입력·버튼은 위 0, 오른쪽 간격만 둔다. 이름표는 자기 Margin(글자 높이 맞춤) 유지.
            foreach (var c in controls)
                if (!(c is Label)) c.Margin = new Padding(0, 0, Math.Max(c.Margin.Right, S(ApcLayout.ItemGap)), 0);
            row.Controls.AddRange(controls);
            return row;
        }
    }
}
