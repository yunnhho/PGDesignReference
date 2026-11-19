using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using FabEnvMonitor.Data;

namespace FabEnvMonitor.Views
{
    /// <summary>설정: 베이별 온도·습도 상하한과 파티클 상한을 표에 바로 입력. 저장 시 검증 → 확인 → Result.</summary>
    internal sealed class BaySettingsDialog : ApcForm
    {
        private const int ColTempLow = 1, ColTempHigh = 2, ColHumiLow = 3, ColHumiHigh = 4, ColParticle = 5;
        private readonly ApcGrid _grid = new ApcGrid { ClearInitialSelection = false, EditMode = DataGridViewEditMode.EditOnEnter,
            SelectionMode = DataGridViewSelectionMode.CellSelect, MultiSelect = false, ScrollBars = ScrollBars.None, ReadOnly = false };

        public BaySettingsDialog() : this(new SampleMainViewSource().GetSettings()) { }

        public BaySettingsDialog(IList<BaySetting> current)
        {
            SetupDialog("베이 설정", 900);

            AddColumn("bay", "베이", true);
            AddColumn("tl", "온도 하한(℃)", false);
            AddColumn("th", "온도 상한(℃)", false);
            AddColumn("hl", "습도 하한(%RH)", false);
            AddColumn("hh", "습도 상한(%RH)", false);
            AddColumn("pp", "파티클 상한(개/m³)", false);
            foreach (var s in current)
                _grid.Rows.Add(s.Name, s.TempLow.ToString("0.0"), s.TempHigh.ToString("0.0"), s.HumiLow.ToString("0.0"),
                    s.HumiHigh.ToString("0.0"), s.ParticleHigh.ToString("#,0"));
            _grid.CellEndEdit += (s, e) => Normalize(e.RowIndex, e.ColumnIndex);
            _grid.Height = ApcGrid.HeightForRows(current.Count);

            AddContent(SectionTitle("베이별 허용 범위"));
            AddContent(_grid);

            var ok = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            ok.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, ok);
            Shown += (s, e) => { _grid.CurrentCell = _grid.Rows[0].Cells[ColTempLow]; _grid.Focus(); };
        }

        /// <summary>저장 확정 후의 설정(베이 순서). 취소하면 null.</summary>
        public BaySetting[] Result { get; private set; }

        private void AddColumn(string name, string header, bool readOnly)
        {
            int i = _grid.Columns.Add(name, header);
            var c = _grid.Columns[i];
            c.ReadOnly = readOnly;
            c.SortMode = DataGridViewColumnSortMode.NotSortable;
            c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private static bool TryNumber(object cell, bool integer, out decimal value)
        {
            string t = (System.Convert.ToString(cell) ?? "").Trim();
            NumberStyles st = integer ? NumberStyles.AllowThousands : NumberStyles.AllowDecimalPoint;
            return decimal.TryParse(t, st, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>입력이 숫자면 표시 자리수(0.0 / #,0)로 맞춘다.</summary>
        private void Normalize(int row, int col)
        {
            if (col == 0) return;
            decimal v;
            var cell = _grid.Rows[row].Cells[col];
            if (TryNumber(cell.Value, col == ColParticle, out v)) cell.Value = col == ColParticle ? v.ToString("#,0") : v.ToString("0.0");
        }

        private void Save()
        {
            _grid.EndEdit();
            var result = new List<BaySetting>();
            foreach (DataGridViewRow row in _grid.Rows)
            {
                string bay = System.Convert.ToString(row.Cells[0].Value);
                var v = new decimal[6];
                for (int c = ColTempLow; c <= ColParticle; c++)
                {
                    if (TryNumber(row.Cells[c].Value, c == ColParticle, out v[c])) continue;
                    ApcDialog.Alert(this, bay + ": " + _grid.Columns[c].HeaderText + "에 숫자를 입력하세요.");
                    _grid.CurrentCell = row.Cells[c];
                    return;
                }
                if (v[ColTempLow] >= v[ColTempHigh] || v[ColHumiLow] >= v[ColHumiHigh])
                {
                    ApcDialog.Alert(this, bay + ": 하한은 상한보다 작아야 합니다.");
                    _grid.CurrentCell = row.Cells[v[ColTempLow] >= v[ColTempHigh] ? ColTempLow : ColHumiLow];
                    return;
                }
                if (v[ColParticle] <= 0)
                {
                    ApcDialog.Alert(this, bay + ": 파티클 상한은 0보다 커야 합니다.");
                    _grid.CurrentCell = row.Cells[ColParticle];
                    return;
                }
                result.Add(new BaySetting(bay, v[ColTempLow], v[ColTempHigh], v[ColHumiLow], v[ColHumiHigh], v[ColParticle]));
            }
            if (!ApcDialog.Confirm(this, "베이 설정을 저장합니다.", "저장", ApcButtonVariant.Primary)) return;
            Result = result.ToArray();
            DialogResult = DialogResult.OK;
        }
    }
}
