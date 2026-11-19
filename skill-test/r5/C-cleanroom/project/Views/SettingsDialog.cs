using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CleanroomMonitor.Data;

namespace CleanroomMonitor.Views
{
    /// <summary>설정 dialog: 구역별 온도·습도 상하한, 파티클 상한(입력 표) + 데이터 저장 주기. 결과는 Result.</summary>
    internal sealed class SettingsDialog : ApcForm
    {
        private const int ColTempLow = 1, ColTempHigh = 2, ColHumiLow = 3, ColHumiHigh = 4, ColParticle = 5;
        private readonly ApcGrid _grid = new ApcGrid
        {
            ReadOnly = false, ClearInitialSelection = false, EditMode = DataGridViewEditMode.EditOnEnter,
            SelectionMode = DataGridViewSelectionMode.CellSelect, MultiSelect = false, ScrollBars = ScrollBars.None,
        };
        private readonly ApcNumericBox _period = new ApcNumericBox { Minimum = 1, Maximum = 3600, Increment = 1, DecimalPlaces = 0, Width = ApcTheme.Px(140) };
        private readonly ZoneLimits[] _original;

        public SettingsDialog() : this(new SampleMainViewSource().GetSettings()) { }

        public SettingsDialog(ZoneSettings settings)
        {
            _original = settings.Limits;
            SetupDialog("환경 기준 설정", 980);

            AddContent(SectionTitle("구역별 기준"));
            AddColumn("zone", "구역", 90, true);
            AddColumn("tl", "온도 하한(℃)", 150, false);
            AddColumn("th", "온도 상한(℃)", 150, false);
            AddColumn("hl", "습도 하한(%RH)", 150, false);
            AddColumn("hh", "습도 상한(%RH)", 150, false);
            AddColumn("pc", "파티클 상한(개/ft³)", 150, false).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (DataGridViewColumn c in _grid.Columns)
            {
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            _grid.AllowUserToResizeColumns = false;
            foreach (var l in settings.Limits)
                _grid.Rows.Add(l.Id, F(l.TempLow), F(l.TempHigh), F(l.HumiLow), F(l.HumiHigh), l.ParticleHigh.ToString("#,0", CultureInfo.InvariantCulture));
            _grid.Height = ApcGrid.HeightForRows(settings.Limits.Length);
            AddContent(_grid);

            _period.Value = settings.SavePeriodSec;
            AddFieldRow("데이터 저장 주기 (s)", _period, FieldCaption("차압 하한 " + ZoneJudge.PressureLow.ToString("0.0", CultureInfo.InvariantCulture) + " Pa는 고정 기준"));

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
            Shown += (s, e) => { _grid.CurrentCell = _grid.Rows[0].Cells[ColTempLow]; _grid.Focus(); };   // 입력 표: 첫 입력 칸에서 시작
        }

        public ZoneSettings Result { get; private set; }

        private static string F(double v) { return v.ToString("0.0", CultureInfo.InvariantCulture); }

        private DataGridViewColumn AddColumn(string name, string header, int width, bool readOnly)
        {
            var c = _grid.Columns[_grid.Columns.Add(name, header)];
            c.Width = ApcTheme.Px(width);
            c.ReadOnly = readOnly;
            return c;
        }

        private void Save()
        {
            _grid.EndEdit();
            var limits = new ZoneLimits[_original.Length];
            for (int i = 0; i < limits.Length; i++)
            {
                string id = _original[i].Id;
                double tl, th, hl, hh; int pc;
                if (!Num(i, ColTempLow, out tl) || !Num(i, ColTempHigh, out th) || !Num(i, ColHumiLow, out hl) || !Num(i, ColHumiHigh, out hh))
                    { Fail(i, id + " 구역: 온도·습도 값은 숫자로 입력하세요."); return; }
                if (!int.TryParse(Cell(i, ColParticle).Replace(",", ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out pc) || pc <= 0)
                    { Fail(i, id + " 구역: 파티클 상한은 1 이상의 정수로 입력하세요."); return; }
                if (tl >= th || hl >= hh)
                    { Fail(i, id + " 구역: 하한은 상한보다 작아야 합니다."); return; }
                limits[i] = new ZoneLimits(id, tl, th, hl, hh, pc);
            }
            if (!ApcDialog.Confirm(this, "설정을 저장할까요?", "저장", ApcButtonVariant.Primary)) return;
            Result = new ZoneSettings(limits, (int)_period.Value);
            DialogResult = DialogResult.OK;
        }

        private string Cell(int row, int col) { return Convert.ToString(_grid.Rows[row].Cells[col].Value).Trim(); }

        private bool Num(int row, int col, out double v)
        {
            return double.TryParse(Cell(row, col), NumberStyles.Float, CultureInfo.InvariantCulture, out v);
        }

        private void Fail(int row, string message)
        {
            ApcDialog.Alert(this, message);
            _grid.CurrentCell = _grid.Rows[row].Cells[ColTempLow];
        }
    }
}
