using System.Collections.Generic;
using System.Windows.Forms;
using APC.UI.Controls;
using ModuleLeakTester.Data;

namespace ModuleLeakTester.Views
{
    /// <summary>모델 선택: 모델 목록 표에서 한 행을 골라 [선택]. 결과는 Selected.</summary>
    internal sealed class ModelSelectDialog : ApcForm
    {
        private readonly IList<LeakModel> _models;
        private readonly ApcGrid _grid = new ApcGrid { ReadOnly = true, ClearInitialSelection = false, MultiSelect = false };

        public ModelSelectDialog() : this(new SampleMainViewSource().GetModels(), null) { }

        public ModelSelectDialog(IList<LeakModel> models, LeakModel current)
        {
            _models = models;
            SetupDialog("모델 선택", 720);

            _grid.Columns.Add("name", "모델명");
            _grid.Columns.Add("pressure", "시험 압력(kPa)");
            _grid.Columns.Add("tol", "압력 허용(±kPa)");
            _grid.Columns.Add("leak", "누설 상한(Pa/s)");
            foreach (DataGridViewColumn c in _grid.Columns) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (var m in models)
                _grid.Rows.Add(m.Name, LeakJudge.Fmt(m.Pressure), LeakJudge.Fmt(m.Tolerance), LeakJudge.Fmt(m.LeakLimit));
            _grid.Height = ApcGrid.HeightForRows(models.Count);
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Choose(); };
            AddContent(_grid);

            int index = current == null ? 0 : models.IndexOf(current);
            if (index >= 0 && index < _grid.Rows.Count)
            {
                _grid.CurrentCell = _grid.Rows[index].Cells[0];
                _grid.Rows[index].Selected = true;
            }

            var select = new ApcButton { Text = "선택", Variant = ApcButtonVariant.Primary };
            select.Click += (s, e) => Choose();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, select);
            AcceptButton = select;
        }

        public LeakModel Selected { get; private set; }

        private void Choose()
        {
            var row = _grid.CurrentRow;
            if (row == null) { ApcDialog.Alert(this, "모델을 선택하세요."); return; }
            Selected = _models[row.Index];
            DialogResult = DialogResult.OK;
        }
    }
}
