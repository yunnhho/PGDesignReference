using System.Windows.Forms;
using APC.UI.Controls;
using CellLeakTester.Data;

namespace CellLeakTester
{
    /// <summary>LOT 입력 dialog: LOT 번호·모델·작업자. 저장하면 Result에 담고 OK로 닫는다.</summary>
    internal sealed class LotDialog : ApcForm
    {
        private readonly ApcTextBox _lot = new ApcTextBox { MaxLength = 20 };
        private readonly ApcComboBox _model = new ApcComboBox();
        private readonly ApcTextBox _worker = new ApcTextBox { MaxLength = 20 };

        public LotDialog() : this(new LotInfo("LOT-261119-A03", "PE-60Ah-A", "김민수")) { }

        public LotDialog(LotInfo current)
        {
            SetupDialog("LOT 입력", 460);
            _model.Items.AddRange(new object[] { "PE-60Ah-A", "PE-60Ah-B", "PE-40Ah-C" });
            _lot.Text = current.Lot;
            _model.SelectedItem = current.Model;
            if (_model.SelectedIndex < 0) _model.SelectedIndex = 0;
            _worker.Text = current.Worker;

            AddFieldRow("LOT 번호", _lot);
            AddFieldRow("모델", _model);
            AddFieldRow("작업자", _worker);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
        }

        public LotInfo Result { get; private set; }

        private void Save()
        {
            string lot = _lot.Text.Trim(), worker = _worker.Text.Trim();
            string error = lot.Length == 0 ? "LOT 번호를 입력하세요."
                         : worker.Length == 0 ? "작업자를 입력하세요." : null;
            if (error != null) { ApcDialog.Alert(this, error); return; }
            if (!ApcDialog.Confirm(this, "LOT '" + lot + "'를 저장할까요?", "저장", ApcButtonVariant.Primary)) return;
            Result = new LotInfo(lot, (string)_model.SelectedItem, worker);
            DialogResult = DialogResult.OK;
        }
    }
}
