using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CleanroomMonitor.Data;

namespace CleanroomMonitor
{
    /// <summary>
    /// 설정 dialog: 구역별 온도·습도 상하한, 파티클 상한(표에서 직접 입력) + 데이터 저장 주기.
    /// 저장 순서: 입력 확정 → 검증(틀리면 Alert) → Confirm("저장", Primary) → Limits/SaveIntervalSec → OK.
    /// </summary>
    internal sealed class SettingsDialog : ApcForm
    {
        private const int ColName = 0, ColTempLow = 1, ColTempHigh = 2, ColHumiLow = 3, ColHumiHigh = 4, ColParticle = 5;
        private readonly ApcGrid _grid = new ApcGrid { ReadOnly = false, ClearInitialSelection = false, EditMode = DataGridViewEditMode.EditOnEnter,
            SelectionMode = DataGridViewSelectionMode.CellSelect, MultiSelect = false, ScrollBars = ScrollBars.None };
        private readonly ApcNumericBox _interval = new ApcNumericBox { Minimum = 1, Maximum = 3600, DecimalPlaces = 0, Increment = 1 };

        public SettingsDialog() : this(new SampleMainViewSource().GetLimits(), 10) { }

        public SettingsDialog(IList<ZoneLimit> limits, int saveIntervalSec)
        {
            SetupDialog("환경 설정", 1000);

            int rowH = ApcTheme.Px(44), headH = ApcTheme.Px(44);
            _grid.RowTemplate.Height = rowH;
            _grid.ColumnHeadersHeight = headH;
            _grid.AllowUserToResizeColumns = false;
            AddCol("zone", "구역", true);
            AddCol("tl", "온도 하한(℃)", false);
            AddCol("th", "온도 상한(℃)", false);
            AddCol("hl", "습도 하한(%RH)", false);
            AddCol("hh", "습도 상한(%RH)", false);
            AddCol("pm", "파티클 상한(개/ft³)", false);
            foreach (var l in limits)
                _grid.Rows.Add(l.Name, l.TempLow.ToString("0.0"), l.TempHigh.ToString("0.0"), l.HumiLow.ToString("0.0"), l.HumiHigh.ToString("0.0"), l.ParticleMax.ToString("#,0"));
            _grid.CellEndEdit += (s, e) => Normalize(e.RowIndex, e.ColumnIndex);
            _grid.Height = headH + rowH * limits.Count + 2;

            _interval.Value = saveIntervalSec;

            AddContent(SectionTitle("구역별 기준"));
            AddContent(_grid);
            AddFieldRow("데이터 저장 주기 (s)", _interval);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
        }

        public IList<ZoneLimit> Limits { get; private set; }
        public int SaveIntervalSec { get; private set; }

        private void AddCol(string name, string header, bool readOnly)
        {
            int i = _grid.Columns.Add(name, header);
            var c = _grid.Columns[i];
            c.ReadOnly = readOnly;
            c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            c.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private static bool TryParse(object v, out decimal d)
        {
            return decimal.TryParse(Convert.ToString(v), NumberStyles.Number, CultureInfo.CurrentCulture, out d);
        }

        private void Normalize(int row, int col)
        {
            if (col == ColName) return;
            decimal d;
            var cell = _grid.Rows[row].Cells[col];
            if (TryParse(cell.Value, out d)) cell.Value = d.ToString(col == ColParticle ? "#,0" : "0.0");
        }

        private void Save()
        {
            _grid.EndEdit();
            _interval.Commit();
            var list = new List<ZoneLimit>();
            foreach (DataGridViewRow r in _grid.Rows)
            {
                string zone = Convert.ToString(r.Cells[ColName].Value);
                decimal tl, th, hl, hh, pm;
                if (!TryParse(r.Cells[ColTempLow].Value, out tl) || !TryParse(r.Cells[ColTempHigh].Value, out th) ||
                    !TryParse(r.Cells[ColHumiLow].Value, out hl) || !TryParse(r.Cells[ColHumiHigh].Value, out hh) ||
                    !TryParse(r.Cells[ColParticle].Value, out pm))
                { ApcDialog.Alert(this, zone + " 구역에 숫자가 아닌 값이 있습니다."); return; }
                if (tl >= th) { ApcDialog.Alert(this, zone + " 구역: 온도 하한이 상한보다 작아야 합니다."); return; }
                if (hl >= hh) { ApcDialog.Alert(this, zone + " 구역: 습도 하한이 상한보다 작아야 합니다."); return; }
                if (pm <= 0) { ApcDialog.Alert(this, zone + " 구역: 파티클 상한은 0보다 커야 합니다."); return; }
                list.Add(new ZoneLimit(zone, tl, th, hl, hh, pm));
            }
            if (!ApcDialog.Confirm(this, "설정을 저장할까요?", "저장", ApcButtonVariant.Primary)) return;
            Limits = list;
            SaveIntervalSec = (int)_interval.Value;
            DialogResult = DialogResult.OK;
        }
    }
}
