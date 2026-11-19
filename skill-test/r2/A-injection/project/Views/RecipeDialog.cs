using System.Windows.Forms;
using APC.UI.Controls;
using InjectionMonitor.Data;

namespace InjectionMonitor.Views
{
    /// <summary>레시피 편집 dialog. 저장: 입력 확정 → 검증 → 확인 → Result 에 담고 OK.</summary>
    internal sealed class RecipeDialog : ApcForm
    {
        private readonly ApcNumericBox _setTemp = new ApcNumericBox { Minimum = 0, Maximum = 300, DecimalPlaces = 1, Increment = 0.1m };
        private readonly ApcNumericBox _maxPressure = new ApcNumericBox { Minimum = 0, Maximum = 300, DecimalPlaces = 1, Increment = 0.1m };
        private readonly ApcNumericBox _cycleTarget = new ApcNumericBox { Minimum = 1, Maximum = 600, DecimalPlaces = 0, Increment = 1 };

        public RecipeDialog() : this(new Recipe(60.0, 120.0, 25)) { }

        public RecipeDialog(Recipe recipe)
        {
            SetupDialog("레시피 편집", 460);

            _setTemp.Value = (decimal)recipe.SetTemp;
            _maxPressure.Value = (decimal)recipe.MaxPressure;
            _cycleTarget.Value = recipe.CycleTarget;

            AddFieldRow("금형 온도 설정값 (℃)", _setTemp);
            AddFieldRow("사출 압력 상한 (bar)", _maxPressure);
            AddFieldRow("사이클 타임 목표 (s)", _cycleTarget);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
        }

        public Recipe Result { get; private set; }

        private void Save()
        {
            _setTemp.Commit(); _maxPressure.Commit(); _cycleTarget.Commit();
            if (_setTemp.Value <= 0) { ApcDialog.Alert(this, "금형 온도 설정값은 0보다 커야 합니다."); return; }
            if (_maxPressure.Value <= 0) { ApcDialog.Alert(this, "사출 압력 상한은 0보다 커야 합니다."); return; }
            if (!ApcDialog.Confirm(this, "레시피를 저장할까요?", "저장", ApcButtonVariant.Primary)) return;
            Result = new Recipe((double)_setTemp.Value, (double)_maxPressure.Value, (int)_cycleTarget.Value);
            DialogResult = DialogResult.OK;
        }
    }
}
