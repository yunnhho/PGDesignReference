using System;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Demo.Templates;
using APC.UI.Theming;

namespace APC.UI.Demo.Catalog
{
    /// <summary>컨트롤 카탈로그: 모든 컨트롤 × 상태를 한 화면에.</summary>
    internal sealed class ControlCatalog : ApcTemplateView
    {
        public ControlCatalog() : base("컨트롤 카탈로그 — " + ApcTheme.Current.Name)
        {
            Body.AutoScroll = true;

            // 아래에서 위로 쌓인다(Top 도킹은 나중 추가가 위) → 보여줄 순서의 역순으로 추가
            var alarmDemo = Button("알람 추가", ApcButtonVariant.Danger);
            int n = 0;
            alarmDemo.Click += (s, e) => Alarm.Push("테스트 알람 #" + (++n));
            var confirm = Button("확인창 열기", ApcButtonVariant.Normal, 130);
            confirm.Click += (s, e) => MessageBox.Show(ApcDialog.Confirm(FindForm(), "정말 삭제할까요? 되돌릴 수 없습니다.") ? "확인" : "취소");
            Body.Controls.Add(Row(Caption("알람 띠 / 확인창"), alarmDemo, confirm));

            var grid = new ApcGrid { Height = S(150), Dock = DockStyle.Top, ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn());
            grid.Columns.Add("a", "LOT");
            grid.Columns.Add("b", "품목");
            grid.Columns["b"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (StatusKind k in Enum.GetValues(typeof(StatusKind))) grid.Rows.Add(k, "LOT-0" + (int)k, k.ToString());
            Body.Controls.Add(grid);
            Body.Controls.Add(Row(Caption("그리드 (ApcGrid + ApcLampColumn)")));

            var scan = new ApcScanBox { Width = S(260) };
            var ok = Button("성공 플래시", ApcButtonVariant.Normal, 120);
            ok.Click += (s, e) => scan.Flash(true);
            var bad = Button("실패 플래시", ApcButtonVariant.Normal, 120);
            bad.Click += (s, e) => scan.Flash(false);
            Body.Controls.Add(Row(Caption("스캔 입력"), scan, ok, bad));

            var lamps = Row(Caption("상태 램프"));
            foreach (StatusKind k in Enum.GetValues(typeof(StatusKind)))
            {
                lamps.Controls.Add(new ApcLamp { Status = k, Blink = k == StatusKind.Alarm, Size = new Size(S(28), S(28)), Margin = new Padding(S(4), S(4), 0, 0) });
                var l = Caption(k.ToString());
                l.Margin = new Padding(S(2), S(8), S(12), 0);
                l.Font = ApcTheme.GetFont();
                lamps.Controls.Add(l);
            }
            Body.Controls.Add(lamps);

            foreach (ApcButtonVariant v in new[] { ApcButtonVariant.Danger, ApcButtonVariant.Primary, ApcButtonVariant.Normal })
            {
                var enabled = Button(v + " 버튼", v, 130);
                var disabled = Button(v + " 비활성", v, 130);
                disabled.Enabled = false;
                var wide = Button(v + " — 넓게 늘려도 모서리 유지", v, 300);
                Body.Controls.Add(Row(Caption(v.ToString().PadRight(8)), enabled, disabled, wide));
            }
            Body.Controls.Add(Row(Caption("버튼: 마우스를 올리면 hover, 누르면 pressed. Tab으로 포커스 표시 확인")));
        }
    }
}
