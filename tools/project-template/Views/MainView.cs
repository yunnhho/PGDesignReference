using System.Windows.Forms;
using APC.UI.Controls;
using __NAME__.Data;

namespace __NAME__.Views
{
    /// <summary>메인 화면. 데이터는 IMainViewSource에서만 받는다(기본: 샘플 데이터).</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private readonly ApcGrid _grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("__NAME__")
        {
            _grid.Columns.Add("name", "설비");
            _grid.Columns.Add("state", "상태");
            _grid.Columns.Add("value", "값");
            _grid.Columns["value"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (var row in source.GetRows()) _grid.Rows.Add(row);

            Body.Controls.Add(_grid);
        }
    }
}
