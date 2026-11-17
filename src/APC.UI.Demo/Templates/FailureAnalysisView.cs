using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Drawing;
using APC.UI.Theming;

namespace APC.UI.Demo.Templates
{
    /// <summary>
    /// references/FailureAnalysis.png 재구성: 설비군 탭 + 시험 모드 안내 + 시계, 2단 헤더 그리드(B1/B2/상태 묶음),
    /// 행 안의 전송·초기화 버튼, 상태 램프 + 문구.
    /// </summary>
    internal sealed class FailureAnalysisView : ApcTemplateView
    {
        private const int ColSend = 11, ColReset = 12, ColLamp = 13, ColStatusText = 14;
        private static readonly string[] Groups = { "저속 연마기 ST-GR-01~03", "저속 연마기 ST-GR-04~06", "고속 연마기 ST-GB-01~08" };

        private readonly GroupHeaderGrid _grid = new GroupHeaderGrid { Dock = DockStyle.Fill, ReadOnly = true };
        private readonly Label _clock = Caption("");
        private readonly Timer _clockTimer = new Timer { Interval = 1000 };
        private readonly List<ApcButton> _tabs = new List<ApcButton>();

        public FailureAnalysisView() : base("연마기 불량 방지 시스템")
        {
            var tabRow = Row();
            for (int i = 0; i < Groups.Length; i++)
            {
                int index = i;
                var tab = Button(Groups[i], ApcButtonVariant.Normal, 230);
                tab.Click += (s, e) => SelectGroup(index);
                _tabs.Add(tab);
                tabRow.Controls.Add(tab);
            }
            var testMode = Caption("ⓘ 시험 모드 : 가상 장비로 동작합니다.");
            testMode.ForeColor = ApcTheme.Status(StatusKind.Info);
            testMode.Margin = new Padding(S(12), S(8), 0, 0);
            tabRow.Controls.Add(testMode);

            var settings = Button("설정", ApcButtonVariant.Normal, 80);
            _clock.Margin = new Padding(0, S(8), S(12), 0);
            var right = new FlowLayoutPanel { Dock = DockStyle.Right, AutoSize = true, WrapContents = false, BackColor = Color.Transparent };
            right.Controls.Add(_clock);
            right.Controls.Add(settings);
            var top = new Panel { Dock = DockStyle.Top, Height = S(46), BackColor = Color.Transparent };
            tabRow.Dock = DockStyle.Fill;
            top.Controls.Add(tabRow);
            top.Controls.Add(right);

            BuildColumns();
            _grid.CellClick += OnCellClick;

            Body.Controls.Add(_grid);
            Body.Controls.Add(top);

            _clockTimer.Tick += (s, e) => _clock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _clockTimer.Start();
            _clock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SelectGroup(0);
        }

        private void BuildColumns()
        {
            _grid.Columns.Add("no", "No");
            _grid.Columns.Add("name", "설비명칭");
            foreach (var b in new[] { "b1", "b2" })
            {
                _grid.Columns.Add(b + "spec", "SPEC");
                _grid.Columns.Add(b + "lot", "LOT No");
                _grid.Columns.Add(b + "barrel", "바렐");
            }
            _grid.Columns.Add("target", "운전목표시간");
            _grid.Columns.Add("current", "현재운전시간");
            _grid.Columns.Add("rpm", "RPM");
            _grid.Columns.Add("send", "전송");
            _grid.Columns.Add("reset", "초기화");
            _grid.Columns.Add(new ApcLampColumn { HeaderText = "" });
            _grid.Columns.Add("status", "");

            // 1920 폭 기준 합계 ≈ 1600px. 좁은 창에서는 가로 스크롤(설비명은 고정 컬럼으로 남음).
            int[] widths = { 44, 230, 230, 140, 50, 230, 140, 50, 100, 100, 60, 64, 64, 36, 90 };
            for (int i = 0; i < widths.Length; i++) _grid.Columns[i].Width = S(widths[i]);
            _grid.Columns["name"].Frozen = true;
            foreach (DataGridViewColumn c in _grid.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
            _grid.Columns["no"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _grid.Columns["rpm"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            _grid.ColumnGroups.Add(new ColumnGroup("B1", 2, 4));
            _grid.ColumnGroups.Add(new ColumnGroup("B2", 5, 7));
            _grid.ColumnGroups.Add(new ColumnGroup("상태", ColLamp, ColStatusText));
        }

        private void SelectGroup(int index)
        {
            for (int i = 0; i < _tabs.Count; i++) _tabs[i].Variant = i == index ? ApcButtonVariant.Primary : ApcButtonVariant.Normal;

            string prefix = index == 2 ? "고속 연마기 ST-GB-" : "저속 연마기 ST-GR-";
            int firstMachine = index == 1 ? 4 : 1;
            int machines = index == 2 ? 8 : 3;
            _grid.Rows.Clear();
            int no = 0;
            for (int m = 0; m < machines; m++)
                for (int col = 1; col <= 2; col++)
                    for (int stage = 1; stage <= 4; stage++)
                    {
                        no++;
                        string name = prefix + (firstMachine + m).ToString("00") + " (" + col + "열 " + stage + "단)";
                        _grid.Rows.Add(SampleRow(no, name));
                    }

            int alarms = 0, offline = 0;
            foreach (DataGridViewRow r in _grid.Rows)
            {
                var k = (StatusKind)r.Cells[ColLamp].Value;
                if (k == StatusKind.Alarm) { alarms++; Alarm.Push(r.Cells[1].Value + ": " + r.Cells[ColStatusText].Value); }
                if (k == StatusKind.Stop) offline++;
            }
            Header.Status = alarms > 0 ? StatusKind.Alarm : StatusKind.Run;
            Header.StatusText = Groups[index] + " · 알람 " + alarms + " · 끊김 " + offline;
        }

        /// <summary>앞쪽 몇 행만 가동 데이터, 나머지는 끊김 — 원본 캡처(전부 끊김)보다 상태 표현을 보여주기 위해.</summary>
        private static object[] SampleRow(int no, string name)
        {
            switch (no)
            {
                case 1: return Row(no, name, "Q3225X7S106K500ARLNN01F", "P052609404712", "2", "S3216X5R106K500NRINN02F", "M052610084612", "1", "02:00:00", "01:12:40", "370", StatusKind.Run, "가동");
                case 2: return Row(no, name, "Q2012X7R475K250NRENN03F", "P052610056015", "1", "", "", "", "01:30:00", "01:28:05", "370", StatusKind.Warn, "종료 임박");
                case 3: return Row(no, name, "S2012X5R226M250NRENN03F", "S052607407714", "1", "", "", "", "02:00:00", "00:41:12", "352", StatusKind.Alarm, "RPM 이탈");
                case 4: return Row(no, name, "", "", "", "", "", "", "", "", "", StatusKind.Info, "대기");
                default: return Row(no, name, "", "", "", "", "", "", "", "", "", StatusKind.Stop, "끊김");
            }
        }

        private static object[] Row(int no, string name, string spec1, string lot1, string barrel1, string spec2, string lot2, string barrel2,
            string target, string current, string rpm, StatusKind status, string statusText)
        {
            return new object[] { no, name, spec1, lot1, barrel1, spec2, lot2, barrel2, target, current, rpm, "▶", "↻", status, statusText };
        }

        private void OnCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = _grid.Rows[e.RowIndex];
            bool offline = (StatusKind)row.Cells[ColLamp].Value == StatusKind.Stop;
            if (e.ColumnIndex == ColSend)
            {
                if (offline) Alarm.Push(row.Cells[1].Value + ": 끊김 상태라 전송할 수 없습니다");
                else Header.StatusText = row.Cells[1].Value + " 작업 조건 전송됨";
            }
            else if (e.ColumnIndex == ColReset && ApcDialog.Confirm(FindForm(), row.Cells[1].Value + "의 운전 시간을 초기화할까요?"))
            {
                row.Cells[9].Value = "00:00:00";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _clockTimer.Dispose();
            base.Dispose(disposing);
        }

        internal sealed class ColumnGroup
        {
            public ColumnGroup(string text, int first, int last) { Text = text; First = first; Last = last; }
            public string Text { get; private set; }
            public int First { get; private set; }
            public int Last { get; private set; }
            public bool Contains(int col) { return col >= First && col <= Last; }
        }

        /// <summary>
        /// 2단 헤더(묶음 제목 + 하위 컬럼) + 행 안의 이미지 버튼 셀(전송·초기화).
        /// ponytail: 데모 전용. 다른 현장 프로그램에서도 필요해지면 ApcGrid로 올린다.
        /// </summary>
        private sealed class GroupHeaderGrid : ApcGrid
        {
            public readonly List<ColumnGroup> ColumnGroups = new List<ColumnGroup>();

            public GroupHeaderGrid()
            {
                ColumnHeadersHeight = ApcTheme.MinHeight * 2;
            }

            private ColumnGroup GroupOf(int col)
            {
                foreach (var g in ColumnGroups) if (g.Contains(col)) return g;
                return null;
            }

            protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
            {
                if (e.RowIndex == -1 && e.ColumnIndex >= 0 && GroupOf(e.ColumnIndex) != null)
                {
                    // 묶음 컬럼: 위 절반은 OnPaint에서 묶음 제목, 아래 절반에 하위 제목
                    var c = ApcTheme.Current.Colors;
                    Gradients.Fill(e.Graphics, e.CellBounds, c.GridHeader);
                    var half = e.CellBounds.Height / 2;
                    var sub = new Rectangle(e.CellBounds.X, e.CellBounds.Y + half, e.CellBounds.Width, e.CellBounds.Height - half);
                    using (var pen = new Pen(ApcTheme.C(c.Border)))
                    {
                        e.Graphics.DrawLine(pen, sub.Right - 1, sub.Top + 3, sub.Right - 1, sub.Bottom - 4);
                        e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    }
                    TextRenderer.DrawText(e.Graphics, Convert.ToString(e.Value), ColumnHeadersDefaultCellStyle.Font, sub, ApcTheme.C(c.Text),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    e.Handled = true;
                    return;
                }
                if (e.RowIndex >= 0 && (e.ColumnIndex == ColSend || e.ColumnIndex == ColReset))
                {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
                    var img = ApcTheme.GetImage("btn_normal_normal");
                    var r = Rectangle.Inflate(e.CellBounds, -S(8), -S(3));
                    var s = ApcTheme.Current.ButtonSlice;
                    NineSlice.Draw(e.Graphics, img, r, new Padding(s[0], s[1], s[2], s[3]), ApcTheme.ImageScale(img));
                    TextRenderer.DrawText(e.Graphics, Convert.ToString(e.Value), ApcTheme.GetFont(FontStyle.Bold), r, ApcTheme.C(ApcTheme.Current.Colors.Text),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    e.Handled = true;
                    return;
                }
                if (e.RowIndex == -1 && e.ColumnIndex >= 0) e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                base.OnCellPainting(e);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var c = ApcTheme.Current.Colors;
                foreach (var g in ColumnGroups)
                {
                    // 화면에 보이는 하위 컬럼 헤더만 합친다(가로 스크롤로 일부·전부 숨을 수 있음).
                    var span = Rectangle.Empty;
                    for (int col = g.First; col <= g.Last; col++)
                    {
                        var r = GetCellDisplayRectangle(col, -1, false);
                        if (r.Width > 0) span = span.IsEmpty ? r : Rectangle.Union(span, r);
                    }
                    if (span.IsEmpty) continue;
                    var top = new Rectangle(span.Left, span.Top, span.Width, ColumnHeadersHeight / 2);
                    Gradients.Fill(e.Graphics, top, c.GridHeader);
                    using (var pen = new Pen(ApcTheme.C(c.Border)))
                    {
                        e.Graphics.DrawLine(pen, top.Left, top.Bottom - 1, top.Right, top.Bottom - 1);
                        e.Graphics.DrawLine(pen, top.Right - 1, top.Top + 3, top.Right - 1, top.Bottom);
                    }
                    TextRenderer.DrawText(e.Graphics, g.Text, ColumnHeadersDefaultCellStyle.Font, top, ApcTheme.C(c.Text),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }

            private static int S(int px) { return (int)Math.Round(px * ApcTheme.Scale); }
        }
    }
}
