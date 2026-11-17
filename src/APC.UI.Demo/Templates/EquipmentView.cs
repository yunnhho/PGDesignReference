using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;

namespace APC.UI.Demo.Templates
{
    /// <summary>설비 상태: 상태별 요약 램프 + 설비 목록(램프 컬럼). 알람 설비는 알람 띠로.</summary>
    internal sealed class EquipmentView : TemplateView
    {
        private static readonly object[][] Machines =
        {
            new object[] { StatusKind.Run, "사출기 01", "WO-24117-05", "92%", "-" },
            new object[] { StatusKind.Run, "사출기 02", "WO-24116-09", "88%", "-" },
            new object[] { StatusKind.Alarm, "사출기 03", "WO-24117-01", "41%", "금형 온도 상한 초과" },
            new object[] { StatusKind.Warn, "사출기 04", "WO-24117-02", "67%", "원자재 잔량 10% 미만" },
            new object[] { StatusKind.Stop, "프레스 01", "-", "0%", "계획 정지" },
            new object[] { StatusKind.Info, "프레스 02", "-", "-", "금형 교체 중" },
        };

        public EquipmentView() : base("설비 상태 · 1공장")
        {
            var counts = new Dictionary<StatusKind, int>();
            foreach (var m in Machines)
            {
                var k = (StatusKind)m[0];
                counts[k] = (counts.ContainsKey(k) ? counts[k] : 0) + 1;
                if (k == StatusKind.Alarm) Alarm.Push(m[1] + ": " + m[4]);
            }

            var summary = Row();
            AddSummary(summary, StatusKind.Run, "가동", counts);
            AddSummary(summary, StatusKind.Warn, "경고", counts);
            AddSummary(summary, StatusKind.Alarm, "알람", counts);
            AddSummary(summary, StatusKind.Stop, "정지", counts);
            AddSummary(summary, StatusKind.Info, "점검", counts);

            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn());
            grid.Columns.Add("name", "설비");
            grid.Columns.Add("order", "작업 지시");
            grid.Columns.Add("rate", "가동률");
            grid.Columns.Add("note", "비고");
            grid.Columns["note"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (var m in Machines) grid.Rows.Add(m);

            Header.Status = StatusKind.Alarm;
            Header.StatusText = "알람 " + (counts.ContainsKey(StatusKind.Alarm) ? counts[StatusKind.Alarm] : 0) + "대";

            Body.Controls.Add(grid);
            Body.Controls.Add(summary);
        }

        private static void AddSummary(FlowLayoutPanel row, StatusKind kind, string label, Dictionary<StatusKind, int> counts)
        {
            var lamp = new ApcLamp { Status = kind, Blink = kind == StatusKind.Alarm, Size = new Size(S(28), S(28)), Margin = new Padding(0, S(2), S(6), 0) };
            var text = Caption(label + " " + (counts.ContainsKey(kind) ? counts[kind] : 0));
            text.Margin = new Padding(0, S(6), S(24), 0);
            row.Controls.Add(lamp);
            row.Controls.Add(text);
        }
    }
}
