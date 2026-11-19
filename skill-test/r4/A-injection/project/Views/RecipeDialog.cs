using System.Windows.Forms;
using APC.UI.Controls;
using InjectionMonitor.Data;

namespace InjectionMonitor.Views
{
    /// <summary>
    /// 레시피 편집: 금형 온도 설정값 / 사출 압력 상한 / 사이클 타임 목표.
    /// 저장 순서: Commit → 검증(Alert) → Confirm → 결과 속성 → OK.
    /// </summary>
    internal sealed class RecipeDialog : ApcForm
    {
        private readonly ApcNumericBox _temp = new ApcNumericBox { Minimum = 0, Maximum = 300, DecimalPlaces = 1, Increment = 0.1m };
        private readonly ApcNumericBox _pressure = new ApcNumericBox { Minimum = 0, Maximum = 300, DecimalPlaces = 1, Increment = 0.1m };
        private readonly ApcNumericBox _cycle = new ApcNumericBox { Minimum = 0, Maximum = 600, DecimalPlaces = 0, Increment = 1 };

        public RecipeDialog() : this((decimal)Judge.TempSetpoint, (decimal)Judge.PressureUpper, (decimal)Judge.CycleTarget) { }

        public RecipeDialog(decimal tempSetpoint, decimal pressureUpper, decimal cycleTarget)
        {
            SetupDialog("레시피 편집", 460);

            _temp.Value = tempSetpoint;
            _pressure.Value = pressureUpper;
            _cycle.Value = cycleTarget;

            AddFieldRow("금형 온도 설정값 (℃)", _temp);
            AddFieldRow("사출 압력 상한 (bar)", _pressure);
            AddFieldRow("사이클 타임 목표 (s)", _cycle);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
        }

        public decimal TempSetpoint { get; private set; }
        public decimal PressureUpper { get; private set; }
        public decimal CycleTarget { get; private set; }

        private void Save()
        {
            _temp.Commit(); _pressure.Commit(); _cycle.Commit();
            string error = _temp.Value < (decimal)Judge.TempLower || _temp.Value > (decimal)Judge.TempUpper
                               ? "금형 온도 설정값은 하한 " + Judge.F1(Judge.TempLower) + " ~ 상한 " + Judge.F1(Judge.TempUpper) + " ℃ 안이어야 합니다."
                         : _pressure.Value <= 0 ? "사출 압력 상한을 입력하세요."
                         : _cycle.Value <= 0 ? "사이클 타임 목표를 입력하세요." : null;
            if (error != null) { ApcDialog.Alert(this, error); return; }
            if (!ApcDialog.Confirm(this, "레시피를 저장할까요?", "저장", ApcButtonVariant.Primary)) return;

            TempSetpoint = _temp.Value; PressureUpper = _pressure.Value; CycleTarget = _cycle.Value;
            DialogResult = DialogResult.OK;
        }
    }
}
