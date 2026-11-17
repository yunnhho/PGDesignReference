using System;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;

namespace APC.UI.Demo.Templates
{
    /// <summary>불량 등록: 유형 + 수량 + LOT(스캔 또는 입력) → 등록.</summary>
    internal sealed class DefectView : TemplateView
    {
        private static readonly string[] Types = { "미성형", "버(Burr)", "변색", "치수 불량", "이물" };

        private readonly ComboBox _type = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown _qty = new NumericUpDown { Minimum = 1, Maximum = 9999, Value = 1 };
        private readonly ApcScanBox _lot = new ApcScanBox();
        private readonly ApcGrid _grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };

        public DefectView() : base("불량 등록 · 사출기 03")
        {
            _type.Items.AddRange(Types);
            _type.SelectedIndex = 0;
            _type.Font = _qty.Font = ApcTheme.GetFont();
            _type.Width = S(160);
            _qty.Width = S(90);
            _lot.Width = S(220);

            var add = Button("등록", ApcButtonVariant.Primary);
            add.Click += (s, e) => Register();

            _grid.Columns.Add("time", "시각");
            _grid.Columns.Add("type", "불량 유형");
            _grid.Columns.Add("qty", "수량");
            _grid.Columns.Add("lot", "LOT");
            _grid.Columns["lot"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            Body.Controls.Add(_grid);
            Body.Controls.Add(Row(Caption("유형"), _type, Caption("수량"), _qty, Caption("LOT"), _lot, add));
            Body.Controls.SetChildIndex(_grid, 0);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var form = FindForm();
            if (form != null) _lot.Attach(form);
        }

        private void Register()
        {
            if (_lot.Text.Trim().Length == 0)
            {
                _lot.Flash(false);
                Alarm.Push("LOT를 스캔하거나 입력하세요");
                return;
            }
            _grid.Rows.Insert(0, DateTime.Now.ToString("HH:mm:ss"), _type.SelectedItem, _qty.Value, _lot.Text.Trim());
            _lot.Clear();
            _qty.Value = 1;
        }
    }
}
