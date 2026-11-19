using System.Windows.Forms;
using APC.UI.Controls;

namespace InjectionMonitor
{
    /// <summary>레시피 편집: 금형 온도 설정값, 사출 압력 상한, 사이클 타임 목표. 저장 순서: Commit → 검증 → Confirm → OK.</summary>
    internal sealed class RecipeDialog : ApcForm
    {
        private readonly ApcNumericBox _moldTemp = new ApcNumericBox { Minimum = 0, Maximum = 300, DecimalPlaces = 1, Increment = 0.5m };
        private readonly ApcNumericBox _pressure = new ApcNumericBox { Minimum = 0, Maximum = 500, DecimalPlaces = 1, Increment = 1m };
        private readonly ApcNumericBox _cycle = new ApcNumericBox { Minimum = 1, Maximum = 600, DecimalPlaces = 0, Increment = 1m };

        public RecipeDialog() : this(60.0m, 150.0m, 30m) { }

        public RecipeDialog(decimal moldTempSet, decimal pressureLimit, decimal cycleTarget)
        {
            SetupDialog("레시피 편집", 460);

            _moldTemp.Value = moldTempSet;
            _pressure.Value = pressureLimit;
            _cycle.Value = cycleTarget;

            AddFieldRow("금형 온도 설정값 (℃)", _moldTemp);
            AddFieldRow("사출 압력 상한 (bar)", _pressure);
            AddFieldRow("사이클 타임 목표 (s)", _cycle);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
        }

        public decimal MoldTempSet { get; private set; }
        public decimal PressureLimit { get; private set; }
        public decimal CycleTarget { get; private set; }

        private void Save()
        {
            _moldTemp.Commit(); _pressure.Commit(); _cycle.Commit();
            if (_moldTemp.Value <= 0) { ApcDialog.Alert(this, "금형 온도 설정값은 0보다 커야 합니다."); return; }
            if (!ApcDialog.Confirm(this, "레시피를 저장할까요?", "저장", ApcButtonVariant.Primary)) return;

            MoldTempSet = _moldTemp.Value;
            PressureLimit = _pressure.Value;
            CycleTarget = _cycle.Value;
            DialogResult = DialogResult.OK;
        }
    }
}
