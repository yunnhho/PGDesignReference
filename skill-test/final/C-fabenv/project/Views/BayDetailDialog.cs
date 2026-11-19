using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using FabEnvMonitor.Data;
using System.Drawing;

namespace FabEnvMonitor.Views
{
    /// <summary>베이 상세(보기용): 상태 줄 + 센서 표 + 온도·습도 24시간 추이 + 닫기. 값은 메인 화면에서 받는다.</summary>
    internal sealed class BayDetailDialog : ApcForm
    {
        public BayDetailDialog() : this(new SampleMainViewSource(), 2) { }

        private BayDetailDialog(IMainViewSource src, int index)
            : this(src.GetBays()[index], src.GetSettings()[index],
                   src.GetTrend24h(src.GetBays()[index].Name, EnvMetric.Temp), src.GetTrend24h(src.GetBays()[index].Name, EnvMetric.Humi)) { }

        public BayDetailDialog(BayReading r, BaySetting s, double[] tempTrend, double[] humiTrend)
        {
            SetupDialog(r.Name + " 상세", 900, 800);
            BayEvaluation ev = EnvJudge.Evaluate(r, s);

            var head = new ApcHeaderBar { Title = r.Name, Status = ev.Status, StatusText = ev.StateText };

            var grid = new ApcGrid { Height = ApcGrid.HeightForRows(3), ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn());
            grid.Columns.Add("item", "센서");
            grid.Columns.Add("value", "현재값");
            grid.Columns.Add("low", "하한");
            grid.Columns.Add("high", "상한");
            grid.Columns.Add("unit", "단위");
            grid.Columns.Add("text", "판정");
            for (int i = 1; i < grid.Columns.Count; i++) grid.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            AddMetricRow(grid, r, s, ev, EnvMetric.Temp, ev.Temp);
            AddMetricRow(grid, r, s, ev, EnvMetric.Humi, ev.Humi);
            AddMetricRow(grid, r, s, ev, EnvMetric.Particle, ev.Particle);

            AddContent(head);
            AddContent(grid);
            AddContent(SectionTitle("온도 24시간 추이 (빨간 점선 = 상·하한)"));
            AddContent(Chart(tempTrend, (double)s.TempLow, (double)s.TempHigh, 170));
            AddContent(SectionTitle("습도 24시간 추이 (빨간 점선 = 상·하한)"));
            AddContentFill(Chart(humiTrend, (double)s.HumiLow, (double)s.HumiHigh, 0));
            var close = new ApcButton { Text = "닫기", DialogResult = DialogResult.OK };
            SetFooter(null, close);
            AcceptButton = close;
        }

        private static ApcTrendChart Chart(double[] values, double low, double high, int height)
        {
            var c = new ApcTrendChart { LowerLimit = low, UpperLimit = high, XLabels = new[] { "-24시간", "-12시간", "현재" } };
            if (height > 0) c.Height = ApcTheme.Px(height);
            c.SetValues(values);
            return c;
        }

        private static void AddMetricRow(ApcGrid grid, BayReading r, BaySetting s, BayEvaluation ev, EnvMetric m, Verdict v)
        {
            decimal? cur = r.Value(m);
            decimal? low = EnvJudge.Low(m, s);
            StatusKind status = v != null ? v.Status : StatusKind.Stop;
            string text = r.State == BayState.Disconnected ? "통신 끊김"
                : r.State == BayState.Stopped ? "정지"
                : v.Status == StatusKind.Run ? "정상" : (v.Status == StatusKind.Alarm ? "알람 · " : "경고 · ") + v.Reason;
            int row = grid.Rows.Add(status, EnvJudge.ItemName(m), cur.HasValue ? EnvJudge.Fmt(m, cur.Value) : "",
                low.HasValue ? EnvJudge.Fmt(m, low.Value) : "", EnvJudge.Fmt(m, EnvJudge.High(m, s)), EnvJudge.Unit(m), text);
            if (v != null && v.Status != StatusKind.Run)
            {
                var cell = grid.Rows[row].Cells["value"];
                cell.Style.ForeColor = ApcTheme.Status(v.Status);
                cell.Style.Font = ApcTheme.GetFont(FontStyle.Bold);
            }
        }
    }
}
