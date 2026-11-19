using System.Windows.Forms;
using APC.UI.Controls;

namespace ReliabilityTester.Views
{
    /// <summary>시험 조건 설정 입력 dialog. 저장 → OK, 결과는 속성으로.</summary>
    internal sealed class TestConditionDialog : ApcForm
    {
        private const decimal TempMin = -70m, TempMax = 200m, HumiMax = 100m, HoursMax = 5000m, CyclesMax = 10000m;

        private readonly ApcTextBox _name = new ApcTextBox();
        private readonly ApcNumericBox _temp = new ApcNumericBox { Minimum = TempMin, Maximum = TempMax, DecimalPlaces = 1, Increment = 0.5m };
        private readonly ApcNumericBox _humi = new ApcNumericBox { Minimum = 0, Maximum = HumiMax, DecimalPlaces = 1, Increment = 0.5m };
        private readonly ApcNumericBox _hours = new ApcNumericBox { Minimum = 1, Maximum = HoursMax, DecimalPlaces = 0, Increment = 1 };
        private readonly ApcNumericBox _cycles = new ApcNumericBox { Minimum = 1, Maximum = CyclesMax, DecimalPlaces = 0, Increment = 1 };

        public TestConditionDialog() : this("HAST-130/85", 130.0m, 85.0m, 96m, 1m) { }

        public TestConditionDialog(string name, decimal temperature, decimal humidity, decimal hours, decimal cycles)
        {
            SetupDialog("시험 조건 설정", 480);

            _name.Text = name;
            _temp.Value = temperature; _humi.Value = humidity; _hours.Value = hours; _cycles.Value = cycles;

            AddFieldRow("조건 이름", _name);
            AddFieldRow("온도 (℃)", _temp);
            AddFieldRow("습도 (%RH)", _humi);
            AddFieldRow("시간 (h)", _hours);
            AddFieldRow("사이클 수", _cycles);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
        }

        public string ConditionName { get; private set; }
        public decimal Temperature { get; private set; }
        public decimal Humidity { get; private set; }
        public decimal Hours { get; private set; }
        public decimal Cycles { get; private set; }

        private void Save()
        {
            foreach (var n in new[] { _temp, _humi, _hours, _cycles }) n.Commit();
            if (_name.Text.Trim().Length == 0) { ApcDialog.Alert(this, "조건 이름을 입력하세요."); return; }
            if (!ApcDialog.Confirm(this, "시험 조건 '" + _name.Text.Trim() + "'을 저장할까요?", "저장", ApcButtonVariant.Primary)) return;

            ConditionName = _name.Text.Trim();
            Temperature = _temp.Value; Humidity = _humi.Value; Hours = _hours.Value; Cycles = _cycles.Value;
            DialogResult = DialogResult.OK;
        }
    }
}
