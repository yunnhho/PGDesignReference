using System;
using System.Windows.Forms;
using APC.UI.Controls;

namespace APC.UI.Demo.Templates
{
    /// <summary>이력 조회: 기간 필터 + 그리드 + 엑셀 내보내기(자리만, 비활성).</summary>
    internal sealed class HistoryView : ApcTemplateView
    {
        private readonly DateTimePicker _from = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _to = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly ApcGrid _grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };

        public HistoryView() : base("이력 조회")
        {
            _from.Value = DateTime.Today.AddDays(-7);
            _to.Value = DateTime.Today;
            _from.Width = _to.Width = S(130);

            var search = Button("조회", ApcButtonVariant.Primary, 90);
            search.Click += (s, e) => Search();
            var export = Button("엑셀 내보내기", ApcButtonVariant.Normal, 130);
            export.Enabled = false;

            _grid.Columns.Add("date", "일자");
            _grid.Columns.Add("line", "설비");
            _grid.Columns.Add("item", "품목");
            _grid.Columns.Add("good", "양품");
            _grid.Columns.Add("bad", "불량");
            _grid.Columns["item"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            Body.Controls.Add(_grid);
            Body.Controls.Add(Row(Caption("기간"), _from, Caption("~"), _to, search, export));
            Body.Controls.SetChildIndex(_grid, 0);
            Search();
        }

        private void Search()
        {
            if (_from.Value.Date > _to.Value.Date)
            {
                Alarm.Push("시작일이 종료일보다 늦습니다");
                return;
            }
            _grid.Rows.Clear();
            int seed = 0;
            for (var d = _to.Value.Date; d >= _from.Value.Date; d = d.AddDays(-1))
            {
                seed++;
                _grid.Rows.Add(d.ToString("yyyy-MM-dd"), "사출기 0" + (seed % 3 + 1), "하우징 A-120", 1100 + seed * 13 % 200, seed * 7 % 23);
            }
        }
    }
}
