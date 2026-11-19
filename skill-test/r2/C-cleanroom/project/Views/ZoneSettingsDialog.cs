using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using CleanroomMonitor.Data;

namespace CleanroomMonitor.Views
{
    /// <summary>
    /// 설정 dialog: 구역별 온도·습도 상하한·파티클 상한 입력 표(칸에 바로 입력) + 데이터 저장 주기.
    /// 저장은 검증 → Confirm("저장") → Limits / SaveIntervalSec 속성 → OK.
    /// </summary>
    internal sealed class ZoneSettingsDialog : ApcForm
    {
        private const int ColZone = 0, ColTempLow = 1, ColTempHigh = 2, ColHumiLow = 3, ColHumiHigh = 4, ColParticle = 5;
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private const NumberStyles Num = NumberStyles.Number;

        private readonly ApcGrid _grid = new ApcGrid { ReadOnly = false, ClearInitialSelection = false, EditMode = DataGridViewEditMode.EditOnEnter,
            SelectionMode = DataGridViewSelectionMode.CellSelect, MultiSelect = false, ScrollBars = ScrollBars.None };
        private readonly ApcNumericBox _interval = new ApcNumericBox { Minimum = 1, Maximum = 3600, Increment = 1, DecimalPlaces = 0 };

        public ZoneSettingsDialog() : this(new SampleMainViewSource().GetLimits(), 10) { }

        public ZoneSettingsDialog(IList<ZoneLimit> limits, int saveIntervalSec)
        {
            SetupDialog("설정", 1000);

            AddColumn("zone", "구역", 100, true);
            AddColumn("tl", "온도 하한(℃)", 150, false);
            AddColumn("th", "온도 상한(℃)", 150, false);
            AddColumn("hl", "습도 하한(%RH)", 160, false);
            AddColumn("hh", "습도 상한(%RH)", 160, false);
            AddColumn("pp", "파티클 상한(개/ft³)", 100, false).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (DataGridViewColumn c in _grid.Columns)
            {
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            _grid.AllowUserToResizeColumns = false;
            foreach (var l in limits)
                _grid.Rows.Add(l.Zone, l.TempLow.ToString("0.0", Inv), l.TempHigh.ToString("0.0", Inv),
                    l.HumiLow.ToString("0.0", Inv), l.HumiHigh.ToString("0.0", Inv), l.ParticleHigh.ToString("#,0", Inv));
            _grid.CellEndEdit += (s, e) => Normalize(e.RowIndex, e.ColumnIndex);
            _grid.Height = ApcGrid.HeightForRows(limits.Count);

            AddContent(SectionTitle("구역별 기준"));
            AddContent(_grid);

            _interval.Value = saveIntervalSec;
            AddFieldRow(AddGroup("데이터 저장"), "저장 주기 (s)", _interval);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
        }

        public IList<ZoneLimit> Limits { get; private set; }
        public int SaveIntervalSec { get; private set; }

        private DataGridViewColumn AddColumn(string name, string header, int width, bool readOnly)
        {
            var c = _grid.Columns[_grid.Columns.Add(name, header)];
            c.Width = Px(width);
            c.ReadOnly = readOnly;
            return c;
        }

        private static int Px(int px) { return APC.UI.Theming.ApcTheme.Px(px); }

        // 입력 자릿수를 기준 문구와 같게(온도·습도 0.0, 파티클 #,0)
        private void Normalize(int row, int col)
        {
            double v;
            var cell = _grid.Rows[row].Cells[col];
            if (col == ColZone || !double.TryParse(System.Convert.ToString(cell.Value), Num, Inv, out v)) return;
            cell.Value = v.ToString(col == ColParticle ? "#,0" : "0.0", Inv);
        }

        private bool TryRead(int row, int col, out double v)
        {
            return double.TryParse(System.Convert.ToString(_grid.Rows[row].Cells[col].Value), Num, Inv, out v);
        }

        private void Save()
        {
            _grid.EndEdit();
            var list = new List<ZoneLimit>();
            for (int i = 0; i < _grid.Rows.Count; i++)
            {
                string zone = System.Convert.ToString(_grid.Rows[i].Cells[ColZone].Value);
                double tl, th, hl, hh, pp;
                if (!TryRead(i, ColTempLow, out tl) || !TryRead(i, ColTempHigh, out th) || !TryRead(i, ColHumiLow, out hl)
                    || !TryRead(i, ColHumiHigh, out hh) || !TryRead(i, ColParticle, out pp))
                {
                    ApcDialog.Alert(this, zone + " 구역에 숫자가 아닌 값이 있습니다.");
                    return;
                }
                if (tl >= th || hl >= hh || pp <= 0)
                {
                    ApcDialog.Alert(this, zone + " 구역: 하한은 상한보다 작아야 하고 파티클 상한은 0보다 커야 합니다.");
                    return;
                }
                list.Add(new ZoneLimit(zone, tl, th, hl, hh, pp));
            }
            if (!ApcDialog.Confirm(this, "설정을 저장할까요?", "저장", ApcButtonVariant.Primary)) return;
            Limits = list;
            SaveIntervalSec = (int)_interval.Value;
            DialogResult = DialogResult.OK;
        }
    }
}
