using System;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;

namespace APC.UI.Demo.Templates
{
    /// <summary>
    /// 입력 폼 dialog 골격: 이름표 + 입력칸 행 반복, 오른쪽 아래 [취소][저장].
    /// 저장 순서: 입력 확정(Commit) → 검증(틀리면 Alert, 닫지 않음) → Confirm("저장", Primary) → 결과를 속성에 담고 OK로 닫기.
    /// 메인 화면은 ShowDialog(FindForm()) == OK일 때 결과 속성을 읽어 데이터 인터페이스에 넘긴다.
    /// </summary>
    internal sealed class RecipeDialog : ApcForm
    {
        private const int RowH = 48, LabelW = 150, FieldW = 230;

        private readonly ApcTextBox _name = new ApcTextBox();
        private readonly ApcNumericBox _loadMax = Number(1), _loadMin = Number(1), _dispMax = Number(2), _dispMin = Number(2), _speed = Number(1);
        private int _y;

        public RecipeDialog() : this("6205-2RS", 12.0m, 8.0m, 5.20m, 4.80m, 2.0m) { }

        public RecipeDialog(string model, decimal loadMax, decimal loadMin, decimal dispMax, decimal dispMin, decimal speed)
        {
            TitleText = "레시피 편집";
            Resizable = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(ApcTheme.Px(460), ApcTheme.Px(470));

            _name.Text = model;
            _loadMax.Value = loadMax; _loadMin.Value = loadMin;
            _dispMax.Value = dispMax; _dispMin.Value = dispMin;
            _speed.Value = speed;

            _y = ContentBounds.Top + ApcTheme.Px(16);
            AddRow("모델명", _name);
            AddRow("하중 상한 (kN)", _loadMax);
            AddRow("하중 하한 (kN)", _loadMin);
            AddRow("변위 상한 (mm)", _dispMax);
            AddRow("변위 하한 (mm)", _dispMin);
            AddRow("압입 속도 (mm/s)", _speed);

            int pad = ApcTheme.Px(16), bw = ApcTheme.Px(110), bh = ApcTheme.Px(40);
            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            var cancel = new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel };
            save.SetBounds(ClientSize.Width - pad - bw, ClientSize.Height - pad - bh, bw, bh);
            cancel.SetBounds(save.Left - ApcTheme.Px(8) - bw, save.Top, bw, bh);
            save.Click += (s, e) => Save();
            Controls.Add(cancel);
            Controls.Add(save);
            CancelButton = cancel;   // Esc = 취소. Enter는 기본 버튼 없음(스캔 Enter로 저장되지 않게)
        }

        public string Model { get; private set; }
        public decimal LoadMax { get; private set; }
        public decimal LoadMin { get; private set; }
        public decimal DispMax { get; private set; }
        public decimal DispMin { get; private set; }
        public decimal Speed { get; private set; }

        private void AddRow(string caption, Control field)
        {
            int x = ApcTheme.Px(24), h = ApcTheme.Px(ApcTextBox.BaseHeight);
            var label = new Label
            {
                Text = caption, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent,
                Font = ApcTheme.GetFont(), ForeColor = ApcTheme.C(ApcTheme.Current.Colors.Text),
                Bounds = new Rectangle(x, _y, ApcTheme.Px(LabelW), h),
            };
            field.SetBounds(x + ApcTheme.Px(LabelW), _y, ApcTheme.Px(FieldW), h);
            Controls.Add(label);
            Controls.Add(field);
            _y += ApcTheme.Px(RowH);
        }

        private void Save()
        {
            foreach (var n in new[] { _loadMax, _loadMin, _dispMax, _dispMin, _speed }) n.Commit();
            string error = _name.Text.Trim().Length == 0 ? "모델명을 입력하세요."
                         : _loadMin.Value >= _loadMax.Value ? "하중 하한이 상한보다 작아야 합니다."
                         : _dispMin.Value >= _dispMax.Value ? "변위 하한이 상한보다 작아야 합니다." : null;
            if (error != null) { ApcDialog.Alert(this, error); return; }
            if (!ApcDialog.Confirm(this, "레시피 '" + _name.Text.Trim() + "'를 저장할까요?", "저장", ApcButtonVariant.Primary)) return;

            Model = _name.Text.Trim();
            LoadMax = _loadMax.Value; LoadMin = _loadMin.Value;
            DispMax = _dispMax.Value; DispMin = _dispMin.Value;
            Speed = _speed.Value;
            DialogResult = DialogResult.OK;
        }

        private static ApcNumericBox Number(int decimals)
        {
            return new ApcNumericBox { Minimum = 0, Maximum = 1000, DecimalPlaces = decimals, Increment = decimals == 2 ? 0.01m : 0.1m };
        }
    }
}
