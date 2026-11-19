using System;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using CleanroomMonitor.Data;

namespace CleanroomMonitor.Views
{
    /// <summary>
    /// 설정 dialog: 구역별 온도·습도 상하한, 파티클 상한(입력 표) + 데이터 저장 주기.
    /// 저장은 Confirm("저장") 뒤 Limits / StorageIntervalSec 속성으로 넘긴다.
    /// </summary>
    internal sealed class SettingsDialog : ApcForm
    {
        private const int FirstValueCol = 1;
        private static readonly string[] Headers =
            { "구역", "온도 하한(℃)", "온도 상한(℃)", "습도 하한(%RH)", "습도 상한(%RH)", "파티클 상한(개/ft³)" };

        private readonly ApcGrid _grid = new ApcGrid
        {
            ReadOnly = false, ClearInitialSelection = false, EditMode = DataGridViewEditMode.EditOnEnter,
            SelectionMode = DataGridViewSelectionMode.CellSelect, MultiSelect = false, ScrollBars = ScrollBars.None,
        };
        private readonly ApcNumericBox _interval = new ApcNumericBox
        {
            Minimum = 1, Maximum = 3600, Increment = 10, DecimalPlaces = 0, Width = ApcTheme.Px(120),
        };

        public SettingsDialog() : this(SampleMainViewSource.DefaultLimits(), ZoneJudge.DefaultStorageIntervalSec) { }

        public SettingsDialog(ZoneLimits[] limits, int storageIntervalSec)
        {
            SetupDialog("설정", 900);

            AddContent(SectionTitle("구역별 기준값"));

            _grid.AllowUserToResizeColumns = false;
            for (int i = 0; i < Headers.Length; i++)
            {
                int c = _grid.Columns.Add("c" + i, Headers[i]);
                var col = _grid.Columns[c];
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                col.ReadOnly = i == 0;
            }
            foreach (var l in limits)
                _grid.Rows.Add(l.Zone, ZoneJudge.Fmt1(l.TempLow), ZoneJudge.Fmt1(l.TempHigh),
                    ZoneJudge.Fmt1(l.HumiLow), ZoneJudge.Fmt1(l.HumiHigh), ZoneJudge.FmtCount(l.ParticleHigh));
            _grid.CellEndEdit += (s, e) => Normalize(e.RowIndex, e.ColumnIndex);
            _grid.Height = ApcGrid.HeightForRows(limits.Length);
            AddContent(_grid);

            _interval.Value = storageIntervalSec;
            AddFieldRow("데이터 저장 주기 (s)", _interval);

            var save = new ApcButton { Text = "저장", Variant = ApcButtonVariant.Primary };
            save.Click += (s, e) => Save();
            SetFooter(new ApcButton { Text = "취소", DialogResult = DialogResult.Cancel }, save);
            Shown += (s, e) => { _grid.CurrentCell = _grid.Rows[0].Cells[FirstValueCol]; _grid.Focus(); };
        }

        public ZoneLimits[] Limits { get; private set; }
        public int StorageIntervalSec { get; private set; }

        private static bool TryParse(object cell, out double v)
        {
            return double.TryParse(Convert.ToString(cell), NumberStyles.Number, CultureInfo.InvariantCulture, out v);
        }

        // 입력 뒤 표시 자리수를 맞춘다(숫자가 아니면 그대로 두고 저장 때 알린다).
        private void Normalize(int row, int col)
        {
            double v;
            if (col < FirstValueCol || !TryParse(_grid.Rows[row].Cells[col].Value, out v)) return;
            _grid.Rows[row].Cells[col].Value = col == _grid.Columns.Count - 1 ? ZoneJudge.FmtCount(v) : ZoneJudge.Fmt1(v);
        }

        private void Save()
        {
            _grid.EndEdit();
            _interval.Commit();
            var result = new ZoneLimits[_grid.Rows.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var cells = _grid.Rows[i].Cells;
                var v = new double[5];
                for (int c = 0; c < 5; c++)
                {
                    if (!TryParse(cells[c + 1].Value, out v[c]))
                        { Fail(i, c + 1, cells[0].Value + " " + Headers[c + 1] + " 값이 숫자가 아닙니다."); return; }
                }
                if (v[0] >= v[1]) { Fail(i, 1, cells[0].Value + " 온도 하한은 상한보다 작아야 합니다."); return; }
                if (v[2] >= v[3]) { Fail(i, 3, cells[0].Value + " 습도 하한은 상한보다 작아야 합니다."); return; }
                if (v[4] <= 0) { Fail(i, 5, cells[0].Value + " 파티클 상한은 0보다 커야 합니다."); return; }
                result[i] = new ZoneLimits(Convert.ToString(cells[0].Value), v[0], v[1], v[2], v[3], v[4]);
            }
            if (!ApcDialog.Confirm(this, "설정을 저장할까요?", "저장", ApcButtonVariant.Primary)) return;
            Limits = result;
            StorageIntervalSec = (int)_interval.Value;
            DialogResult = DialogResult.OK;
        }

        private void Fail(int row, int col, string message)
        {
            ApcDialog.Alert(this, message);
            _grid.CurrentCell = _grid.Rows[row].Cells[col];
        }
    }
}
