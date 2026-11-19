using System.Windows.Forms;
using APC.UI.Controls;

namespace CellLeakTester.Views
{
    /// <summary>LOT 입력: LOT 번호·모델·작업자. 확정 순서: 검증(Alert) → Confirm("저장") → 결과 속성 → OK.</summary>
    internal sealed class LotEntryDialog : ApcForm
    {
        private readonly ApcTextBox _lot = new ApcTextBox { MaxLength = 24 };
        private readonly ApcComboBox _model = new ApcComboBox();
        private readonly ApcTextBox _worker = new ApcTextBox { MaxLength = 16 };

        public LotEntryDialog() : this("LOT-261119-03", "NCM-21700", "김민수") { }

        public LotEntryDialog(string lotNumber, string model, string worker)
        {
            SetupDialog("LOT 입력", 460);
            _model.Items.AddRange(new object[] { "NCM-21700", "NCM-18650", "LFP-32140" });
            _lot.Text = lotNumber;
            _model.SelectedItem = model;
            if (_model.SelectedIndex < 0) _model.SelectedIndex = 0;
            _worker.Text = worker;

            AddFieldRow("LOT 번호", _lot);
            AddFieldRow("모델", _model);
            AddFieldRow("작업자", _worker);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
        }

        public string LotNumber { get; private set; }
        public string Model { get; private set; }
        public string Worker { get; private set; }

        private void Save()
        {
            string error = _lot.Text.Trim().Length == 0 ? "LOT 번호를 입력하세요."
                         : _worker.Text.Trim().Length == 0 ? "작업자를 입력하세요." : null;
            if (error != null) { ApcDialog.Alert(this, error); return; }
            if (!ApcDialog.Confirm(this, "LOT '" + _lot.Text.Trim() + "'를 저장할까요?", "저장", ApcButtonVariant.Primary)) return;
            LotNumber = _lot.Text.Trim(); Model = (string)_model.SelectedItem; Worker = _worker.Text.Trim();
            DialogResult = DialogResult.OK;
        }
    }
}
