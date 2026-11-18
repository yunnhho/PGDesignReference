using System;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;

namespace APC.UI.Demo.Templates
{
    /// <summary>
    /// 보기용 상세 dialog 골격: 상태 줄(ApcHeaderBar) + 센서 표 + 추이 + 오른쪽 아래 닫기.
    /// 값은 메인 화면에서 넘겨받는다(자체 데이터 인터페이스 없음). 매개변수 없는 생성자는 render용 샘플.
    /// </summary>
    internal sealed class EquipmentDetailDialog : ApcForm
    {
        public EquipmentDetailDialog() : this("건조로2", StatusKind.Alarm, "알람 · 온도 상한 초과",
            new[] { new object[] { StatusKind.Alarm, "로 내 온도", "168.0", "160.0", "℃" }, new object[] { StatusKind.Run, "배기 팬", "1450", "1450", "rpm" } },
            SampleTrend(), 160, 165) { }

        /// <param name="sensors">행마다 { StatusKind, 센서, 현재값, 설정값, 단위 }</param>
        public EquipmentDetailDialog(string name, StatusKind status, string statusText, object[][] sensors, double[] trend, double setPoint, double upper)
        {
            TitleText = name + " 상세";
            Resizable = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(ApcTheme.Px(760), ApcTheme.Px(560));

            var head = new ApcHeaderBar { Dock = DockStyle.Top, Title = name, Status = status, StatusText = statusText };

            // 표 높이 = 머리줄 + 센서 수만큼(빈 줄 없이). 남는 높이는 추이 그래프(Fill)가 쓴다.
            int gridH = (int)Math.Round(ApcTheme.MinHeight * 1.15) + ApcTheme.MinHeight * sensors.Length + 2;
            var grid = new ApcGrid { Dock = DockStyle.Top, Height = gridH, ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn());
            grid.Columns.Add("name", "센서");
            grid.Columns.Add("value", "현재값");
            grid.Columns.Add("set", "설정값");
            grid.Columns.Add("unit", "단위");
            grid.Columns["name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (var row in sensors) grid.Rows.Add(row);

            var chart = new ApcTrendChart { Dock = DockStyle.Fill, Reference = setPoint, UpperLimit = upper, XLabels = new[] { "-60분", "-30분", "현재" } };
            chart.SetValues(trend);
            var caption = new Label
            {
                Dock = DockStyle.Top, Height = ApcTheme.Px(32), Text = "최근 1시간 온도 추이 (회색 점선 = 설정값, 빨간 점선 = 상한)",
                TextAlign = ContentAlignment.BottomLeft, BackColor = Color.Transparent,
                Font = ApcTheme.GetFont(FontStyle.Bold), ForeColor = ApcTheme.C(ApcTheme.Current.Colors.Text),
            };

            var close = new ApcButton { Text = "닫기", Width = ApcTheme.Px(110), Dock = DockStyle.Right, DialogResult = DialogResult.OK };
            var buttons = new Panel { Dock = DockStyle.Bottom, Height = ApcTheme.Px(34 + 12), Padding = new Padding(0, ApcTheme.Px(12), 0, 0), BackColor = Color.Transparent };
            buttons.Controls.Add(close);   // 버튼 줄: Dock=Right로 오른쪽 끝, 폭은 고정(전체 폭으로 늘지 않게)

            // 내용 영역: ContentBounds 안에 여백을 두고, 그 안은 도킹(Fill 먼저 → Top은 아래쪽 것부터)
            var content = new Panel { Bounds = ContentBounds, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(ApcTheme.Px(16)), BackColor = Color.Transparent };
            content.Controls.Add(chart);
            content.Controls.Add(caption);
            content.Controls.Add(grid);
            content.Controls.Add(head);
            content.Controls.Add(buttons);
            Controls.Add(content);
            AcceptButton = CancelButton = close;
        }

        private static double[] SampleTrend()
        {
            var v = new double[61];
            for (int i = 0; i <= 60; i++) v[i] = 160 + Math.Sin(Math.Min(i, 45) / 5.0) + (i > 45 ? (i - 45) * (8.0 - Math.Sin(9.0)) / 15 : 0);   // 마지막 점 = 168.0(현재값과 같게)
            return v;
        }
    }
}
