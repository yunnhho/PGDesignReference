using System.Windows.Forms;
using APC.UI.Controls;
using InjectionMonitor.Data;

namespace InjectionMonitor.Views
{
    /// <summary>레시피 편집: 입력 확정 → 검증 → 저장 확인 → 결과 속성 → OK.</summary>
    internal sealed class RecipeDialog : ApcForm
    {
        private readonly ApcNumericBox _temp = Number(1, 0.1m, 400m);
        private readonly ApcNumericBox _pressure = Number(1, 10m, 3000m);
        private readonly ApcNumericBox _cycle = Number(0, 1m, 600m);

        public RecipeDialog()
            : this(new Recipe((decimal)Judgement.TempSetpoint, (decimal)Judgement.PressureUpper, Judgement.CycleTarget)) { }

        public RecipeDialog(Recipe recipe)
        {
            SetupDialog("레시피 편집", 460);

            _temp.Value = recipe.MoldTempSetpoint;
            _pressure.Value = recipe.PressureUpper;
            _cycle.Value = recipe.CycleTarget;

            AddFieldRow("금형 온도 설정값 (℃)", _temp);
            AddFieldRow("사출 압력 상한 (bar)", _pressure);
            AddFieldRow("사이클 타임 목표 (s)", _cycle);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
        }

        public Recipe Result { get; private set; }

        private void Save()
        {
            _temp.Commit(); _pressure.Commit(); _cycle.Commit();
            string error = null;
            if (_temp.Value < (decimal)Judgement.TempLower || _temp.Value > (decimal)Judgement.TempUpper)
                error = "금형 온도 설정값은 " + Judgement.F(Judgement.TempLower) + " ~ " + Judgement.F(Judgement.TempUpper) + " ℃ 범위여야 합니다.";
            else if (_pressure.Value <= 0) error = "사출 압력 상한은 0보다 커야 합니다.";
            else if (_cycle.Value <= 0) error = "사이클 타임 목표는 0보다 커야 합니다.";
            if (error != null) { ApcDialog.Alert(this, error); return; }
            if (!ApcDialog.Confirm(this, "레시피를 저장할까요?", "저장", ApcButtonVariant.Primary)) return;

            Result = new Recipe(_temp.Value, _pressure.Value, _cycle.Value);
            DialogResult = DialogResult.OK;
        }

        private static ApcNumericBox Number(int decimals, decimal increment, decimal max)
        {
            return new ApcNumericBox { Minimum = 0, Maximum = max, DecimalPlaces = decimals, Increment = increment };
        }
    }
}
