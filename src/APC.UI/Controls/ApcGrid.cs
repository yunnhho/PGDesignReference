using System;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Drawing;
using APC.UI.Theming;

namespace APC.UI.Controls
{
    /// <summary>
    /// 테마 스타일을 입힌 DataGridView. 헤더 그라데이션, 행 배경 한 색(번갈아 칠하지 않음), 행 전체 선택.
    /// 사용자가 표를 누르거나 키로 움직이기 전에는 첫 행이 저절로 선택되지 않는다(CurrentRow = null) —
    /// "선택한 행에 대해" 동작하는 버튼은 CurrentRow가 null이면 아무것도 하지 않게 둔다.
    /// </summary>
    public class ApcGrid : DataGridView
    {
        public ApcGrid()
        {
            DoubleBuffered = true;
            EnableHeadersVisualStyles = false;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            RowHeadersVisible = false;
            AllowUserToAddRows = false;
            AllowUserToResizeRows = false;
            BorderStyle = BorderStyle.FixedSingle;
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            // 현장 표 기준: 전부 가운데. 긴 글(알람 문구·비고) 열만 그 열에서 MiddleLeft로.
            DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ApcTheme.Changed += OnThemeChanged;
            ApplyTheme();
        }

        /// <summary>머리줄 + 행 rows개가 빈 줄·스크롤바 없이 들어가는 표 높이(배율 적용 px). dialog의 AddContent 표 높이에 쓴다.</summary>
        public static int HeightForRows(int rows) { return HeaderHeight() + ApcTheme.MinHeight * Math.Max(0, rows) + 2; }

        private static int HeaderHeight() { return (int)Math.Round(ApcTheme.MinHeight * 1.15); }

        private void ApplyTheme()
        {
            RowTemplate.Height = ApcTheme.MinHeight;
            ColumnHeadersHeight = HeaderHeight();
            if (ApcTheme.Current == null) return;

            var c = ApcTheme.Current.Colors;
            var text = ApcTheme.C(c.Text);
            Font = ApcTheme.GetFont();
            BackgroundColor = Color.White;
            GridColor = ApcTheme.C(c.GridLine);
            DefaultCellStyle.ForeColor = text;
            DefaultCellStyle.SelectionBackColor = ApcTheme.C(c.GridSelected);
            DefaultCellStyle.SelectionForeColor = text;
            ColumnHeadersDefaultCellStyle.Font = ApcTheme.GetFont(FontStyle.Bold);
            ColumnHeadersDefaultCellStyle.ForeColor = text;
            ColumnHeadersDefaultCellStyle.SelectionBackColor = ApcTheme.C(c.GridHeader[1]);
            foreach (DataGridViewRow row in Rows) row.Height = RowTemplate.Height;
        }

        /// <summary>기본 폭 100px은 한글 폰트에서 코드값(WO-24117-01 등)이 잘린다.</summary>
        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            if (!(e.Column is ApcLampColumn) && e.Column.Width == 100)
                e.Column.Width = (int)Math.Round(130 * ApcTheme.Scale);
            base.OnColumnAdded(e);
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            base.OnCellPainting(e);
            if (e.Handled || e.RowIndex != -1 || ApcTheme.Current == null) return;

            var c = ApcTheme.Current.Colors;
            Gradients.Fill(e.Graphics, e.CellBounds, c.GridHeader);
            using (var pen = new Pen(ApcTheme.C(c.Border)))
            {
                e.Graphics.DrawLine(pen, e.CellBounds.Right - 1, e.CellBounds.Top + 4, e.CellBounds.Right - 1, e.CellBounds.Bottom - 5);
                e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
            }
            e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);
            e.Handled = true;
        }

        // DataGridView는 화면에 뜰 때·행이 추가될 때 첫 행을 저절로 선택해 파란 줄이 생긴다(모니터링 표에서 '선택된 행'으로 오해).
        // 사용자가 표를 누르거나 키로 움직이기 전까지는 저절로 생긴 선택을 지운다. 앱이 코드로 고른 선택은 그 뒤 그대로 둔다.
        private bool _userSelected;

        private void ClearAutoSelection()
        {
            if (_userSelected || !ClearInitialSelection || !IsHandleCreated) return;
            ClearSelection();
            if (CurrentCell != null && !IsCurrentCellInEditMode) CurrentCell = null;
        }

        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); ClearAutoSelection(); }
        protected override void OnVisibleChanged(EventArgs e) { base.OnVisibleChanged(e); ClearAutoSelection(); }
        protected override void OnEnter(EventArgs e) { base.OnEnter(e); ClearAutoSelection(); }   // 폼이 뜨며 첫 포커스를 줄 때 여기서 선택된다
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); ClearAutoSelection(); }
        protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
        {
            base.OnRowsAdded(e);
            // 현재 셀은 RowsAdded 이벤트 뒤에 잡히므로 처리 후로 미룬다.
            if (!_userSelected && IsHandleCreated) BeginInvoke((MethodInvoker)ClearAutoSelection);
        }
        protected override void OnMouseDown(MouseEventArgs e) { _userSelected = true; base.OnMouseDown(e); }
        protected override void OnKeyDown(KeyEventArgs e) { _userSelected = true; base.OnKeyDown(e); }

        /// <summary>
        /// true(기본)면 사용자가 누르기 전엔 선택 행이 없다. 입력 표(스캔으로 칸을 채우는 dialog 등)처럼
        /// 앱이 CurrentCell을 정해 두고 바로 입력받아야 하면 false.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool ClearInitialSelection { get; set; } = true;

        /// <summary>편집 가능한 표에서 읽기 전용 열은 회색 바탕 — 어느 칸에 입력하는지 한눈에 보이게.</summary>
        protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
        {
            base.OnCellFormatting(e);
            if (ReadOnly || ApcTheme.Current == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (Columns[e.ColumnIndex].ReadOnly) e.CellStyle.BackColor = ApcTheme.C(ApcTheme.Current.Colors.Background[0]);
        }

        private void OnThemeChanged(object sender, EventArgs e) { ApplyTheme(); Invalidate(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing) ApcTheme.Changed -= OnThemeChanged;
            base.Dispose(disposing);
        }
    }
}
