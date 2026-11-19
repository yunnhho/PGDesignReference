using System.Windows.Forms;
using APC.UI.Controls;

namespace CellLeakTester
{
    /// <summary>LOT 입력 dialog: LOT 번호·모델·작업자. 저장 → 검증 → Confirm → 결과 속성 → OK.</summary>
    internal sealed class LotInputDialog : ApcForm
    {
        private readonly ApcTextBox _lot = new ApcTextBox { MaxLength = 32 };
        private readonly ApcComboBox _model = new ApcComboBox();
        private readonly ApcTextBox _worker = new ApcTextBox { MaxLength = 20 };

        public LotInputDialog() : this("LOT-261119-A", "NCM-60Ah", "김현장") { }

        public LotInputDialog(string lot, string model, string worker)
        {
            SetupDialog("LOT 입력", 460);

            _model.Items.AddRange(new object[] { "NCM-60Ah", "NCM-75Ah", "LFP-100Ah" });
            _lot.Text = lot;
            _worker.Text = worker;
            int idx = _model.Items.IndexOf(model);
            _model.SelectedIndex = idx < 0 ? 0 : idx;

            AddFieldRow("LOT 번호", _lot);
            AddFieldRow("모델", _model);
            AddFieldRow("작업자", _worker);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
        }

        public string Lot { get; private set; }
        public string Model { get; private set; }
        public string Worker { get; private set; }

        private void Save()
        {
            string error = _lot.Text.Trim().Length == 0 ? "LOT 번호를 입력하세요."
                         : _worker.Text.Trim().Length == 0 ? "작업자를 입력하세요." : null;
            if (error != null) { ApcDialog.Alert(this, error); return; }
            if (!ApcDialog.Confirm(this, "LOT '" + _lot.Text.Trim() + "'를 저장할까요?", "저장", ApcButtonVariant.Primary)) return;

            Lot = _lot.Text.Trim();
            Model = (string)_model.SelectedItem;
            Worker = _worker.Text.Trim();
            DialogResult = DialogResult.OK;
        }
    }
}
