using System;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using ModuleLeakTester.Data;

namespace ModuleLeakTester.Views
{
    /// <summary>메인 화면: 모델·바코드 → 판정판 + 시험 압력·누설량 카드 → 금일 집계 → 최근 검사 이력. 데이터는 IMainViewSource에서만 받는다.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const string ScreenTitle = "모듈 기밀 검사";

        private readonly IMainViewSource _source;
        private LeakModel _model;

        private readonly ApcTextBox _modelBox = new ApcTextBox { ReadOnly = true };
        private readonly ApcScanBox _scan = new ApcScanBox();
        private readonly ApcJudgePanel _judge = new ApcJudgePanel { Dock = DockStyle.Fill, Text = "READY", SubText = "모듈 바코드를 스캔하세요", Status = StatusKind.Stop };
        private readonly ApcValueCard _pressure = Card("시험 압력", "kPa");
        private readonly ApcValueCard _leak = Card("누설량", "Pa/s");
        private readonly ApcValueCard _total = Card("금일 생산", "EA");
        private readonly ApcValueCard _ok = Card("OK", "EA");
        private readonly ApcValueCard _ng = Card("NG", "EA");
        private readonly ApcValueCard _rate = Card("불량률", "%");
        private readonly ApcGrid _grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base(ScreenTitle)
        {
            _source = source;
            _model = source.GetModels()[0];
            Header.Status = StatusKind.Stop;
            Header.StatusText = "대기";
            _scan.Width = S(300);
            _modelBox.Width = S(220);
            _scan.Scanned += (s, e) => e.Accepted = Test(e.Code);
            _judge.Margin = new Padding(0, 0, S(12), 0);

            BuildGrid();
            foreach (var r in source.GetHistory(_model)) AddHistoryRow(_grid.Rows.Count, r);

            var pick = Button("모델 선택", ApcButtonVariant.Normal, 130);
            pick.Click += (s, e) => PickModel();
            var admin = Button("관리자", ApcButtonVariant.Normal, 130);
            admin.Click += (s, e) => AskAdmin();

            // Top 도킹은 나중에 Add한 것이 위 → 이력(Fill) 먼저, 아래 줄부터
            Body.Controls.Add(_grid);
            Body.Controls.Add(SectionTitleTop("최근 검사 이력"));
            Body.Controls.Add(Columns(S(130), new Control[] { _total, _ok, _ng, _rate }, new[] { 25f, 25f, 25f, 25f }));
            Body.Controls.Add(Columns(S(220), new Control[] { _judge, _pressure, _leak }, new[] { 50f, 25f, 25f }));
            Body.Controls.Add(Row(Caption("모델"), _modelBox, pick, Caption("바코드"), _scan, admin));
            ShowModel();
            ShowCounts();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var form = FindForm();
            if (form != null) _scan.Attach(form);   // 생성자 시점엔 폼이 없다
        }

        private static Label SectionTitleTop(string text)
        {
            var l = SectionTitle(text);
            l.Dock = DockStyle.Top;
            return l;
        }

        private void BuildGrid()
        {
            _grid.Columns.Add(new ApcLampColumn { HeaderText = "상태" });
            _grid.Columns.Add("time", "시각");
            _grid.Columns.Add("barcode", "바코드");
            _grid.Columns.Add("pressure", "시험 압력(kPa)");
            _grid.Columns.Add("leak", "누설량(Pa/s)");
            _grid.Columns.Add("result", "판정");
            _grid.Columns.Add("reason", "NG 사유");
            // 긴 글 열(NG 사유) 하나만 Fill, 나머지는 고정 폭
            _grid.Columns[1].Width = S(120);
            _grid.Columns[2].Width = S(200);
            _grid.Columns[3].Width = S(160);
            _grid.Columns[4].Width = S(160);
            _grid.Columns[5].Width = S(90);
            _grid.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _grid.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        }

        private void AddHistoryRow(int index, HistoryRow r)
        {
            string reason = r.NgReason;
            _grid.Rows.Insert(index, reason == null ? StatusKind.Run : StatusKind.Alarm, r.Time.ToString("HH:mm:ss"), r.Barcode,
                LeakJudge.Fmt(r.Pressure), LeakJudge.Fmt(r.Leak), reason == null ? "OK" : "NG", reason ?? "");
            var row = _grid.Rows[index];
            MarkIfOut(row.Cells[3], LeakJudge.PressureStatus(r.Model, r.Pressure));
            MarkIfOut(row.Cells[4], LeakJudge.LeakStatus(r.Model, r.Leak));
        }

        private static void MarkIfOut(DataGridViewCell cell, StatusKind kind)
        {
            if (kind != StatusKind.Alarm) return;
            cell.Style.ForeColor = ApcTheme.Status(kind);
            cell.Style.Font = ApcTheme.GetFont(FontStyle.Bold);
        }

        private void PickModel()
        {
            using (var dlg = new ModelSelectDialog(_source.GetModels(), _model))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                _model = dlg.Selected;
                ShowModel();
            }
        }

        private void AskAdmin()
        {
            using (var dlg = new AdminPasswordDialog())
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                if (!_source.VerifyAdminPassword(dlg.Password)) { ApcDialog.Alert(FindForm(), "비밀번호가 맞지 않습니다."); return; }
                ApcDialog.Alert(FindForm(), "관리자 확인 완료. (설정 화면 진입 자리)");
            }
        }

        private void ShowModel()
        {
            _modelBox.Text = _model.Name;
            _pressure.Footer = LeakJudge.PressureCriterion(_model);
            _leak.Footer = LeakJudge.LeakCriterion(_model);
        }

        private bool Test(string barcode)
        {
            var r = _source.Test(barcode, _model);
            string reason = r.NgReason;
            bool ok = reason == null;

            _judge.Text = ok ? "OK" : "NG";
            _judge.Status = ok ? StatusKind.Run : StatusKind.Alarm;
            _judge.SubText = ok ? barcode : barcode + " · " + reason;
            _pressure.Value = LeakJudge.Fmt(r.Pressure);
            _pressure.Status = LeakJudge.PressureStatus(_model, r.Pressure);
            _leak.Value = LeakJudge.Fmt(r.Leak);
            _leak.Status = LeakJudge.LeakStatus(_model, r.Leak);
            AddHistoryRow(0, r);
            Header.Status = ok ? StatusKind.Run : StatusKind.Alarm;
            Header.StatusText = ok ? "가동중" : "NG 발생";
            if (!ok) Alarm.Push(barcode + ": " + reason);
            ShowCounts();
            return true;   // 판정과 무관하게 스캔 자체는 정상 수신
        }

        private void ShowCounts()
        {
            int ok = _source.TodayOk, ng = _source.TodayNg, total = ok + ng;
            _total.Value = total.ToString("#,0");
            _ok.Value = ok.ToString("#,0");
            _ng.Value = ng.ToString("#,0");
            _ng.Status = ng > 0 ? StatusKind.Alarm : StatusKind.Run;
            _rate.Value = total == 0 ? "" : LeakJudge.Fmt(100.0 * ng / total);
        }

        private static ApcValueCard Card(string title, string unit)
        {
            return new ApcValueCard { Title = title, Unit = unit, Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(12), 0) };
        }
    }
}
