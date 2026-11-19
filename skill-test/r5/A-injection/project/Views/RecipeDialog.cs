using System.Windows.Forms;
using APC.UI.Controls;
using InjectionMonitor.Data;

namespace InjectionMonitor.Views
{
    /// <summary>
    /// 레시피 편집 dialog: 금형 온도 설정값, 사출 압력 상한, 사이클 타임 목표.
    /// 저장 순서: 입력 확정 → 검증 → Confirm("저장") → 결과 속성 → OK.
    /// </summary>
    internal sealed class RecipeDialog : ApcForm
    {
        private readonly ApcNumericBox _tempSet = new ApcNumericBox { Minimum = 0, Maximum = 400, DecimalPlaces = 1, Increment = 0.5m };
        private readonly ApcNumericBox _pressureUpper = new ApcNumericBox { Minimum = 0, Maximum = 400, DecimalPlaces = 1, Increment = 0.5m };
        private readonly ApcNumericBox _cycleTarget = new ApcNumericBox { Minimum = 1, Maximum = 600, DecimalPlaces = 0, Increment = 1 };
        private readonly ApcTextBox _machine = new ApcTextBox { ReadOnly = true };

        public RecipeDialog() : this("IM-03", Criteria.TempSet, Criteria.PressureUpper, Criteria.CycleTarget) { }

        public RecipeDialog(string machine, decimal tempSet, decimal pressureUpper, int cycleTarget)
        {
            SetupDialog("레시피 편집", 460);

            _machine.Text = machine;
            _tempSet.Value = tempSet;
            _pressureUpper.Value = pressureUpper;
            _cycleTarget.Value = cycleTarget;

            AddFieldRow("설비", _machine);
            AddFieldRow("금형 온도 설정값 (℃)", _tempSet);
            AddFieldRow("사출 압력 상한 (bar)", _pressureUpper);
            AddFieldRow("사이클 타임 목표 (s)", _cycleTarget);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
        }

        public decimal TempSet { get; private set; }
        public decimal PressureUpper { get; private set; }
        public int CycleTarget { get; private set; }

        private void Save()
        {
            _tempSet.Commit(); _pressureUpper.Commit(); _cycleTarget.Commit();
            string error = _tempSet.Value >= Criteria.TempWarn ? "금형 온도 설정값은 경고 시작값 " + Criteria.TempWarn.ToString("0.0") + " ℃보다 작아야 합니다." : null;
            if (error != null) { ApcDialog.Alert(this, error); return; }
            if (!ApcDialog.Confirm(this, _machine.Text + " 레시피를 저장할까요?", "저장", ApcButtonVariant.Primary)) return;

            TempSet = _tempSet.Value;
            PressureUpper = _pressureUpper.Value;
            CycleTarget = (int)_cycleTarget.Value;
            DialogResult = DialogResult.OK;
        }
    }
}
