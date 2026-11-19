using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using FATester.Data;

namespace FATester.Views
{
    /// <summary>설비 상세 dialog: 상태 줄 + 챔버 표(온도·습도·누적 사이클·최근 오류 코드) + 온도 추이 + 닫기.</summary>
    internal sealed class ChannelDetailDialog : ApcForm
    {
        public ChannelDetailDialog() : this(new SampleMainViewSource().GetChannels()[0]) { }

        public ChannelDetailDialog(ChannelInfo ch)
        {
            SetupDialog(ch.Name + " 설비 상세", 760, 560);
            ChannelDetail d = ch.Detail;

            var head = new ApcHeaderBar { Title = ch.Name + (ch.TestItem.Length > 0 ? " · " + ch.TestItem : ""), Status = ch.Status, StatusText = ch.StatusText };

            var grid = new ApcGrid { Height = ApcGrid.HeightForRows(4), ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn());
            grid.Columns.Add("name", "항목");
            grid.Columns.Add("value", "현재값");
            grid.Columns.Add("ref", "기준");
            grid.Columns.Add("unit", "단위");
            grid.Columns["ref"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns["ref"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.Rows.Add(Judge.Of(d.Temp, d.TempUpper - Limits.TempMargin, d.TempUpper), "챔버 온도", Judge.F1(d.Temp),
                "설정 " + Judge.F1(d.TempSet) + " · 상한 " + Judge.F1(d.TempUpper), "℃");
            grid.Rows.Add(Judge.Of(d.Humi, d.HumiUpper - Limits.HumiMargin, d.HumiUpper), "챔버 습도", Judge.F1(d.Humi),
                "설정 " + Judge.F1(d.HumiSet) + " · 상한 " + Judge.F1(d.HumiUpper), "%RH");
            grid.Rows.Add(StatusKind.Run, "누적 사이클", d.Cycles.ToString("#,0"), "", "회");
            grid.Rows.Add(d.ErrorCode.Length > 0 ? d.ErrorStatus : StatusKind.Stop, "최근 오류 코드", d.ErrorCode, d.ErrorNote, "");

            var chart = new ApcTrendChart { Reference = d.TempSet, UpperLimit = d.TempUpper, XLabels = new[] { "-60분", "-30분", "현재" } };
            chart.SetValues(d.Trend);

            AddContent(head);
            AddContent(grid);
            AddContent(SectionTitle("최근 1시간 챔버 온도 추이 (회색 점선 = 설정값, 빨간 점선 = 상한)"));
            AddContentFill(chart);
            var close = new ApcButton { Text = "닫기", DialogResult = DialogResult.OK };
            SetFooter(null, close);
            AcceptButton = close;
        }
    }
}
