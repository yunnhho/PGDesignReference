using System.Windows.Forms;
using APC.UI.Controls;
using MoldingLineMonitor.Data;

namespace MoldingLineMonitor.Views
{
    /// <summary>레시피 편집: 입력 확정 → 검증(Alert) → Confirm("저장") → 결과 속성 → OK.</summary>
    internal sealed class RecipeDialog : ApcForm
    {
        private readonly ApcNumericBox _moldSet = Number(1, 0.1m), _pressureUpper = Number(1, 0.1m), _clampUpper = Number(1, 0.1m), _cycleTarget = Number(0, 1m);

        public RecipeDialog() : this(Criterion.MoldSet, Criterion.PressureUpper, Criterion.ClampUpper, Criterion.CycleTarget) { }

        public RecipeDialog(decimal moldSet, decimal pressureUpper, decimal clampUpper, decimal cycleTarget)
        {
            SetupDialog("레시피 편집", 460);

            _moldSet.Value = moldSet;
            _pressureUpper.Value = pressureUpper;
            _clampUpper.Value = clampUpper;
            _cycleTarget.Value = cycleTarget;

            AddFieldRow("금형 온도 설정값 (℃)", _moldSet);
            AddFieldRow("사출 압력 상한 (bar)", _pressureUpper);
            AddFieldRow("형체력 상한 (kN)", _clampUpper);
            AddFieldRow("사이클 타임 목표 (s)", _cycleTarget);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
        }

        public decimal MoldSet { get; private set; }
        public decimal PressureUpper { get; private set; }
        public decimal ClampUpper { get; private set; }
        public decimal CycleTarget { get; private set; }

        private void Save()
        {
            foreach (var n in new[] { _moldSet, _pressureUpper, _clampUpper, _cycleTarget }) n.Commit();
            string error = _moldSet.Value < 55.0m || _moldSet.Value > 65.0m ? "금형 온도 설정값은 하한 55.0 ~ 상한 65.0 ℃ 사이여야 합니다."
                         : _pressureUpper.Value > 150.0m ? "사출 압력 상한은 설비 한계 150.0 bar를 넘을 수 없습니다."
                         : _clampUpper.Value > 500.0m ? "형체력 상한은 설비 한계 500.0 kN을 넘을 수 없습니다."
                         : _cycleTarget.Value >= 28m ? "사이클 타임 목표는 상한 28.0 s보다 작아야 합니다." : null;
            if (error != null) { ApcDialog.Alert(this, error); return; }
            if (!ApcDialog.Confirm(this, "레시피를 저장할까요?", "저장", ApcButtonVariant.Primary)) return;

            MoldSet = _moldSet.Value; PressureUpper = _pressureUpper.Value;
            ClampUpper = _clampUpper.Value; CycleTarget = _cycleTarget.Value;
            DialogResult = DialogResult.OK;
        }

        private static ApcNumericBox Number(int decimals, decimal increment)
        {
            return new ApcNumericBox { Minimum = 0, Maximum = 1000, DecimalPlaces = decimals, Increment = increment };
        }
    }
}
