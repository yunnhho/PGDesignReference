using System.Windows.Forms;
using APC.UI.Controls;
using CellLeakTester.Data;

namespace CellLeakTester.Views
{
    /// <summary>LOT 입력 dialog: LOT 번호·모델·작업자. 저장 시 검증 → 확인 → Result.</summary>
    internal sealed class LotInputDialog : ApcForm
    {
        private readonly ApcTextBox _lot = new ApcTextBox { MaxLength = 24 };
        private readonly ApcComboBox _model = new ApcComboBox();
        private readonly ApcTextBox _worker = new ApcTextBox { MaxLength = 16 };

        public LotInputDialog() : this(new LotInfo("LOT-261119-A", "CL-50A", "김민수")) { }

        public LotInputDialog(LotInfo current)
        {
            SetupDialog("LOT 입력", 460);
            _model.Items.AddRange(new object[] { "CL-50A", "CL-50B", "CL-75A" });
            _model.SelectedItem = current.Model;
            if (_model.SelectedIndex < 0) _model.SelectedIndex = 0;
            _lot.Text = current.Lot;
            _worker.Text = current.Worker;

            AddFieldRow("LOT 번호", _lot);
            AddFieldRow("모델", _model);
            AddFieldRow("작업자", _worker);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);   // Enter 기본 버튼 없음(스캔 Enter로 저장되지 않게)
        }

        public LotInfo Result { get; private set; }

        private void Save()
        {
            string error = _lot.Text.Trim().Length == 0 ? "LOT 번호를 입력하세요."
                         : _worker.Text.Trim().Length == 0 ? "작업자를 입력하세요." : null;
            if (error != null) { ApcDialog.Alert(this, error); return; }
            if (!ApcDialog.Confirm(this, "LOT '" + _lot.Text.Trim() + "'를 저장할까요?", "저장", ApcButtonVariant.Primary)) return;
            Result = new LotInfo(_lot.Text.Trim(), (string)_model.SelectedItem, _worker.Text.Trim());
            DialogResult = DialogResult.OK;
        }
    }
}
