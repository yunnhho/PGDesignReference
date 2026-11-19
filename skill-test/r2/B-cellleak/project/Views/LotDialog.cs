using System.Windows.Forms;
using APC.UI.Controls;

namespace CellLeakTester.Views
{
    /// <summary>LOT 입력: LOT 번호, 모델, 작업자. 저장 → 결과 속성 → OK.</summary>
    internal sealed class LotDialog : ApcForm
    {
        private readonly ApcTextBox _lot = new ApcTextBox();
        private readonly ApcComboBox _model = new ApcComboBox();
        private readonly ApcTextBox _operator = new ApcTextBox();

        public LotDialog() : this("LOT-20261119-03", "NCM-21700", "김현장") { }

        public LotDialog(string lot, string model, string worker)
        {
            SetupDialog("LOT 입력", 460);
            _model.Items.AddRange(new object[] { "NCM-21700", "NCM-18650", "LFP-32140" });
            _model.SelectedItem = model;
            _lot.Text = lot;
            _operator.Text = worker;
            AddFieldRow("LOT 번호", _lot);
            AddFieldRow("모델", _model);
            AddFieldRow("작업자", _operator);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
        }

        public string Lot { get; private set; }
        public string Model { get; private set; }
        public string Operator { get; private set; }

        private void Save()
        {
            string error = _lot.Text.Trim().Length == 0 ? "LOT 번호를 입력하세요."
                         : _model.SelectedItem == null ? "모델을 선택하세요."
                         : _operator.Text.Trim().Length == 0 ? "작업자를 입력하세요." : null;
            if (error != null) { ApcDialog.Alert(this, error); return; }
            if (!ApcDialog.Confirm(this, "LOT " + _lot.Text.Trim() + "를 저장할까요?", "저장", ApcButtonVariant.Primary)) return;
            Lot = _lot.Text.Trim();
            Model = (string)_model.SelectedItem;
            Operator = _operator.Text.Trim();
            DialogResult = DialogResult.OK;
        }
    }
}
