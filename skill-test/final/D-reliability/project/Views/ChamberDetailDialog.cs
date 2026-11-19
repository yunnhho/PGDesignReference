using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using ReliabilityTester.Data;

namespace ReliabilityTester.Views
{
    /// <summary>챔버 상세: 상태 줄 + 온도·습도 현재값 + 추이 + 최근 오류 코드. 값은 메인 화면에서 넘겨받는다.</summary>
    internal sealed class ChamberDetailDialog : ApcForm
    {
        public ChamberDetailDialog() : this(new SampleMainViewSource().GetChamberDetail("CH3")) { }

        public ChamberDetailDialog(ChamberDetail d)
        {
            SetupDialog(d.Channel + " 챔버 상세", 900);

            var head = new ApcHeaderBar { Title = d.Channel, Status = d.Status, StatusText = d.StatusText };

            var tempCard = Card("챔버 온도", d.Temp[d.Temp.Length - 1], "℃", d.TempStatus, d.TempSet, d.TempUpper);
            var humiCard = Card("챔버 습도", d.Humi[d.Humi.Length - 1], "%RH", d.HumiStatus, d.HumiSet, d.HumiUpper);

            var tempChart = Chart(d.Temp, d.TempSet, d.TempUpper);
            var humiChart = Chart(d.Humi, d.HumiSet, d.HumiUpper);

            var errors = new ApcGrid { Height = ApcGrid.HeightForRows(d.Errors.Length), ReadOnly = true };
            errors.Columns.Add(new ApcLampColumn());
            errors.Columns.Add("time", "시각");
            errors.Columns.Add("code", "오류 코드");
            errors.Columns.Add("text", "내용");
            errors.Columns["text"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            errors.Columns["text"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            foreach (var row in d.Errors) errors.Rows.Add(row);

            AddContent(head);
            AddContentRow(tempCard, humiCard);
            AddContentRow(SectionTitle("온도 추이 (최근 1시간)"), SectionTitle("습도 추이 (최근 1시간)"));
            AddContentRow(tempChart, humiChart);
            AddContent(SectionTitle("최근 오류 코드"));
            AddContent(errors);
            var close = new ApcButton { Text = "닫기", DialogResult = DialogResult.OK };
            SetFooter(null, close);
            AcceptButton = close;
        }

        private static ApcValueCard Card(string title, double value, string unit, StatusKind status, double set, double upper)
        {
            return new ApcValueCard
            {
                Title = title, Value = Criteria.F1(value), Unit = unit, Status = status, ShowLamp = true,
                Footer = "설정 " + Criteria.F1(set) + " · 상한 " + Criteria.F1(upper),
                Height = ApcTheme.Px(110),
            };
        }

        private static ApcTrendChart Chart(double[] values, double set, double upper)
        {
            var chart = new ApcTrendChart { Reference = set, UpperLimit = upper, XLabels = new[] { "-60분", "-30분", "현재" }, Height = ApcTheme.Px(220) };
            chart.SetValues(values);
            return chart;
        }
    }
}
