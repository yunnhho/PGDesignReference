using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CleanroomMonitor.Data;

namespace CleanroomMonitor.Views
{
    /// <summary>
    /// 설정 dialog: 구역별 온도·습도 상하한, 파티클 상한(입력 표) + 데이터 저장 주기.
    /// 확인 시 검증 → 저장 확인 → Limits / SavePeriodSec 속성. 매개변수 없는 생성자는 render용 샘플.
    /// </summary>
    internal sealed class SettingsDialog : ApcForm
    {
        private const int ZoneCount = 8, MinPeriodSec = 1, MaxPeriodSec = 3600;
        private const int ColZone = 0, ColTempLow = 1, ColTempHigh = 2, ColHumiLow = 3, ColHumiHigh = 4, ColParticle = 5;

        private readonly ApcGrid _grid = new ApcGrid { ReadOnly = false, ClearInitialSelection = false,
            SelectionMode = DataGridViewSelectionMode.CellSelect, MultiSelect = false, ScrollBars = ScrollBars.None,
            EditMode = DataGridViewEditMode.EditOnEnter };
        private readonly ApcNumericBox _period = new ApcNumericBox { Minimum = MinPeriodSec, Maximum = MaxPeriodSec, DecimalPlaces = 0, Increment = 10, Width = ApcTheme.Px(140) };

        public SettingsDialog() : this(new SampleMainViewSource().GetLimits(), new SampleMainViewSource().GetSavePeriodSec()) { }

        public SettingsDialog(IList<ZoneLimits> limits, int savePeriodSec)
        {
            SetupDialog("설정", 900);

            _grid.AllowUserToResizeColumns = false;
            AddColumn("zone", "구역", true);
            AddColumn("tl", "온도 하한(℃)", false);
            AddColumn("th", "온도 상한(℃)", false);
            AddColumn("hl", "습도 하한(%RH)", false);
            AddColumn("hh", "습도 상한(%RH)", false);
            AddColumn("pp", "파티클 상한(개/ft³)", false);
            foreach (var l in limits)
                _grid.Rows.Add(l.Zone, l.TempLow.ToString("0.0"), l.TempHigh.ToString("0.0"), l.HumiLow.ToString("0.0"), l.HumiHigh.ToString("0.0"), l.ParticleHigh.ToString("#,0"));
            _grid.Height = ApcGrid.HeightForRows(ZoneCount);

            _period.Value = savePeriodSec;
            AddContent(SectionTitle("구역별 상·하한"));
            AddContent(_grid);
            AddFieldRow("데이터 저장 주기 (s)", _period, FieldCaption("1 ~ 3600"));

            var ok = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            ok.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, ok);
        }

        public IList<ZoneLimits> Limits { get; private set; }
        public int SavePeriodSec { get; private set; }

        private void AddColumn(string name, string header, bool readOnly)
        {
            int i = _grid.Columns.Add(name, header);
            var c = _grid.Columns[i];
            c.ReadOnly = readOnly;
            c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            c.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private bool Parse(int row, int col, string label, out double value)
        {
            string s = Convert.ToString(_grid.Rows[row].Cells[col].Value).Replace(",", "").Trim();
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return true;
            ApcDialog.Alert(this, _grid.Rows[row].Cells[ColZone].Value + " 구역 " + label + " 값이 숫자가 아닙니다.");
            _grid.CurrentCell = _grid.Rows[row].Cells[col];
            return false;
        }

        private void Save()
        {
            _grid.EndEdit();
            _period.Commit();
            var list = new List<ZoneLimits>();
            for (int i = 0; i < _grid.Rows.Count; i++)
            {
                double tl, th, hl, hh, pp;
                if (!Parse(i, ColTempLow, "온도 하한", out tl) || !Parse(i, ColTempHigh, "온도 상한", out th)
                    || !Parse(i, ColHumiLow, "습도 하한", out hl) || !Parse(i, ColHumiHigh, "습도 상한", out hh)
                    || !Parse(i, ColParticle, "파티클 상한", out pp)) return;
                string zone = (string)_grid.Rows[i].Cells[ColZone].Value;
                if (tl >= th) { Reject(i, ColTempLow, zone + " 구역 온도 하한은 상한보다 작아야 합니다."); return; }
                if (hl >= hh) { Reject(i, ColHumiLow, zone + " 구역 습도 하한은 상한보다 작아야 합니다."); return; }
                if (pp <= 0) { Reject(i, ColParticle, zone + " 구역 파티클 상한은 0보다 커야 합니다."); return; }
                list.Add(new ZoneLimits(zone, tl, th, hl, hh, pp));
            }
            if (!ApcDialog.Confirm(this, "설정을 저장하시겠습니까?", "저장", ApcButtonVariant.Primary)) return;
            Limits = list;
            SavePeriodSec = (int)_period.Value;
            DialogResult = DialogResult.OK;
        }

        private void Reject(int row, int col, string message)
        {
            ApcDialog.Alert(this, message);
            _grid.CurrentCell = _grid.Rows[row].Cells[col];
        }
    }
}
