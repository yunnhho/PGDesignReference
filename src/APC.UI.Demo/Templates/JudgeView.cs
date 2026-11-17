using System;
using System.Globalization;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;

namespace APC.UI.Demo.Templates
{
    /// <summary>
    /// 설비 한 대 판정 화면: 모델 선택 + 스캔 → 판정판(OK/NG) + 측정값 카드 2 → 금일 집계 카드 4 → 판정 이력.
    /// 스캔 한 건마다 측정·판정해 위에서부터 갱신한다. NG는 알람바에도.
    /// </summary>
    internal sealed class JudgeView : ApcTemplateView
    {
        private const double LoadMin = 8.0, LoadMax = 12.0, DispMin = 4.80, DispMax = 5.20;

        private readonly ApcComboBox _model = new ApcComboBox();
        private readonly ApcScanBox _scan = new ApcScanBox();
        private readonly ApcJudgePanel _judge = new ApcJudgePanel { Dock = DockStyle.Fill, Text = "READY", SubText = "시리얼을 스캔하세요" };
        private readonly ApcValueCard _load = Card("최대 하중", "kN", "기준 8.0 ~ 12.0 kN");
        private readonly ApcValueCard _disp = Card("최종 변위", "mm", "기준 4.80 ~ 5.20 mm");
        private readonly ApcValueCard _total = Card("금일 생산", "EA", "");
        private readonly ApcValueCard _ok = Card("OK", "EA", "");
        private readonly ApcValueCard _ng = Card("NG", "EA", "");
        private readonly ApcValueCard _rate = Card("불량률", "%", "NG / 생산");
        private readonly ApcGrid _grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
        private readonly Random _sample = new Random(7);
        private int _okCount = 13, _ngCount = 3;

        public JudgeView() : base("베어링 압입 판정 · 압입기 01")
        {
            Header.Status = StatusKind.Stop;
            Header.StatusText = "대기";
            _model.Items.AddRange(new object[] { "6205-2RS", "6204-ZZ", "6306-2RS" });
            _model.SelectedIndex = 0;
            _model.Width = S(180);
            _scan.Width = S(300);
            _scan.Scanned += (s, e) => e.Accepted = Judge(e.Code);
            _judge.Margin = new Padding(0, 0, S(12), 0);

            _grid.Columns.Add(new ApcLampColumn());
            _grid.Columns.Add("serial", "시리얼");
            _grid.Columns.Add("time", "시각");
            _grid.Columns.Add("load", "최대 하중(kN)");
            _grid.Columns.Add("disp", "최종 변위(mm)");
            _grid.Columns.Add("result", "판정");
            _grid.Columns["serial"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _grid.Rows.Add(StatusKind.Run, "BP2411045", "10:15:00", "10.7", "5.03", "OK");
            _grid.Rows.Add(StatusKind.Alarm, "BP2411044", "10:12:00", "12.6", "5.01", "NG · 하중 상한");
            _grid.Rows.Add(StatusKind.Run, "BP2411043", "10:09:00", "10.2", "4.98", "OK");

            var recipe = Button("레시피 편집", ApcButtonVariant.Normal, 130);
            recipe.Click += (s, e) => ApcDialog.Alert(FindForm(), "레시피 편집 dialog 자리 (ApcForm 상속 별도 폼)");

            // Top 도킹은 나중에 Add한 것이 위 → 이력(Fill) 먼저, 그다음 아래 줄부터
            Body.Controls.Add(_grid);
            Body.Controls.Add(Columns(S(130), new Control[] { _total, _ok, _ng, _rate }, new[] { 25f, 25f, 25f, 25f }));
            Body.Controls.Add(Columns(S(220), new Control[] { _judge, _load, _disp }, new[] { 50f, 25f, 25f }));
            Body.Controls.Add(Row(Caption("모델"), _model, Caption("시리얼"), _scan, recipe));
            ShowCounts();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var form = FindForm();
            if (form != null) _scan.Attach(form);   // 생성자 시점엔 폼이 없다
        }

        private bool Judge(string serial)
        {
            // 샘플 측정값. 실제는 데이터 인터페이스(PLC 등)에서 받는다.
            double load = Math.Round(9 + _sample.NextDouble() * 4, 1), disp = Math.Round(4.85 + _sample.NextDouble() * 0.3, 2);
            string reason = load > LoadMax ? "하중 " + load + " kN > 상한 " + LoadMax
                          : load < LoadMin ? "하중 " + load + " kN < 하한 " + LoadMin
                          : disp > DispMax || disp < DispMin ? "변위 " + disp + " mm 범위 밖" : null;
            bool ok = reason == null;
            if (ok) _okCount++; else _ngCount++;

            _judge.Text = ok ? "OK" : "NG";
            _judge.Status = ok ? StatusKind.Run : StatusKind.Alarm;
            _judge.SubText = ok ? serial : serial + " · " + reason;
            _load.Value = load.ToString("0.0", CultureInfo.InvariantCulture);
            _load.Status = load > LoadMax || load < LoadMin ? StatusKind.Alarm : StatusKind.Run;
            _disp.Value = disp.ToString("0.00", CultureInfo.InvariantCulture);
            _disp.Status = disp > DispMax || disp < DispMin ? StatusKind.Alarm : StatusKind.Run;
            _grid.Rows.Insert(0, ok ? StatusKind.Run : StatusKind.Alarm, serial, DateTime.Now.ToString("HH:mm:ss"),
                _load.Value, _disp.Value, ok ? "OK" : "NG");
            Header.Status = ok ? StatusKind.Run : StatusKind.Alarm;
            Header.StatusText = ok ? "가동중" : "NG 발생";
            if (!ok) Alarm.Push(serial + ": " + reason);
            ShowCounts();
            return true;   // 판정 결과와 무관하게 스캔 자체는 정상 수신
        }

        private void ShowCounts()
        {
            int total = _okCount + _ngCount;
            _total.Value = total.ToString();
            _ok.Value = _okCount.ToString();
            _ng.Value = _ngCount.ToString();
            _ng.Status = _ngCount > 0 ? StatusKind.Alarm : StatusKind.Run;
            _rate.Value = total == 0 ? "" : (100.0 * _ngCount / total).ToString("0.0", CultureInfo.InvariantCulture);
        }

        private static ApcValueCard Card(string title, string unit, string footer)
        {
            return new ApcValueCard { Title = title, Unit = unit, Footer = footer, Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(12), 0) };
        }

        /// <summary>한 줄에 퍼센트 폭으로 나란히(카드·판정판). 줄 아래 간격은 Padding.Bottom.</summary>
        private static TableLayoutPanel Columns(int height, Control[] cells, float[] percents)
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Top, Height = height + S(12), ColumnCount = cells.Length, RowCount = 1, Padding = new Padding(0, 0, 0, S(12)) };
            foreach (var p in percents) t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, p));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            for (int i = 0; i < cells.Length; i++) t.Controls.Add(cells[i], i, 0);
            cells[cells.Length - 1].Margin = Padding.Empty;   // 마지막 칸은 오른쪽 여백 없이 끝선 맞춤
            return t;
        }
    }
}
