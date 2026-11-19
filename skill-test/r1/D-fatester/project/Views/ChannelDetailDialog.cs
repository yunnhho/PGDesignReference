using System.Collections.Generic;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using FATester.Data;

namespace FATester.Views
{
    /// <summary>설비 상세 dialog: 채널을 고르면 챔버 온도·습도·누적 사이클·최근 오류 코드를 보여 준다(보기용).</summary>
    internal sealed class ChannelDetailDialog : ApcForm
    {

        private readonly IList<ChannelInfo> _channels;
        private readonly ApcComboBox _combo = new ApcComboBox();
        private readonly ApcHeaderBar _head = new ApcHeaderBar();
        private readonly ApcValueCard _temp = NewCard("챔버 온도", "℃");
        private readonly ApcValueCard _humi = NewCard("챔버 습도", "%RH");
        private readonly ApcValueCard _cycles = NewCard("누적 사이클", "회");
        private readonly ApcValueCard _error = NewCard("최근 오류 코드", "");

        public ChannelDetailDialog() : this(new SampleMainViewSource().GetChannels(), 2) { }

        public ChannelDetailDialog(IList<ChannelInfo> channels, int selectedIndex)
        {
            _channels = channels;
            SetupDialog("설비 상세", 720, 440);

            foreach (var ch in channels) _combo.Items.Add(ch.Name + (ch.Item.Length == 0 ? "" : " · " + ch.Item));
            AddFieldRow("채널", _combo);

            var cards = new TableLayoutPanel { ColumnCount = 2, RowCount = 2 };
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            cards.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            cards.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            cards.Controls.Add(_temp, 0, 0);
            cards.Controls.Add(_humi, 1, 0);
            cards.Controls.Add(_cycles, 0, 1);
            cards.Controls.Add(_error, 1, 1);

            int gap = ApcTheme.Px(ApcLayout.ItemGap), rgap = ApcTheme.Px(ApcLayout.RowGap) / 2;   // 위아래 줄 높이를 같게 반씩
            _temp.Margin = new Padding(0, 0, gap, rgap);
            _humi.Margin = new Padding(0, 0, 0, rgap);
            _cycles.Margin = new Padding(0, rgap, gap, 0);
            _error.Margin = new Padding(0, rgap, 0, 0);

            AddContent(_head);
            AddContentFill(cards);
            var close = new ApcButton { Text = "닫기", DialogResult = DialogResult.OK };
            SetFooter(null, close);

            _combo.SelectedIndexChanged += (s, e) => Show(_combo.SelectedIndex);
            _combo.SelectedIndex = selectedIndex;
        }

        private static ApcValueCard NewCard(string title, string unit)
        {
            return new ApcValueCard { Title = title, Unit = unit, ShowLamp = true, Dock = DockStyle.Fill };
        }

        private void Show(int index)
        {
            if (index < 0 || index >= _channels.Count) return;
            var ch = _channels[index];
            _head.Title = ch.Name;
            _head.Status = ch.OverallStatus;
            _head.StatusText = ch.OverallText;
            _temp.Footer = ch.Item.Length == 0 ? "시험 항목 없음" : "시험 항목 " + ch.Item;
            _humi.Footer = ch.SampleId.Length == 0 ? "시료 없음" : "시료 " + ch.SampleId;
            _cycles.Footer = ch.ProgressPercent.HasValue ? "진행률 " + ch.ProgressPercent.Value.ToString("0") + " %" : "진행 중 아님";
            _temp.Value = ch.ChamberTemp.ToString("0.0");
            _humi.Value = ch.Humidity.ToString("0.0");
            _cycles.Value = ch.Cycles.ToString("#,0");
            bool hasError = ch.LastError != "없음";
            _error.Value = ch.LastError;
            _error.Status = hasError ? StatusKind.Alarm : StatusKind.Run;
            _error.Footer = hasError ? ch.LastErrorText : "오류 이력 없음";
        }
    }
}
