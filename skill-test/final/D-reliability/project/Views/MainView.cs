using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using ReliabilityTester.Data;

namespace ReliabilityTester.Views
{
    /// <summary>
    /// 메인 화면: 채널 타일(누르면 챔버 상세) → 채널 현황 표 → 금일 불량 분류 표.
    /// 데이터는 IMainViewSource에서만 받는다(기본: 샘플 데이터).
    /// </summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const int ChannelCount = 6;
        private readonly IMainViewSource _source;

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("신뢰성 시험기 모니터링")
        {
            _source = source;
            var channels = source.GetChannels();

            Header.Status = Criteria.WorstOf(channels);
            Header.StatusText = Criteria.Summary(channels);

            var tiles = new Control[channels.Count];
            for (int i = 0; i < channels.Count; i++)
            {
                var r = channels[i];
                var tile = new ApcStatusTile { Text = r.Name, Status = Criteria.ChannelStatus(r), StatusText = Criteria.StatusText(r), Dock = DockStyle.Fill };
                string name = r.Name;
                tile.Click += (s, e) => OpenDetail(name);
                tiles[i] = tile;
                if (Criteria.ChannelStatus(r) == StatusKind.Alarm) Alarm.Push(Criteria.AlarmText(r));
            }

            var conditionButton = Button("시험 조건 설정", ApcButtonVariant.Normal, 160);
            conditionButton.Click += (s, e) => OpenCondition();

            // Top 도킹은 나중에 Add한 것이 위 → 불량 표(Fill) 먼저, 아래 줄부터
            Body.Controls.Add(BuildDefectGrid(source));
            var defectTitle = SectionTitle("금일 불량 분류 결과");
            defectTitle.Dock = DockStyle.Top;
            Body.Controls.Add(defectTitle);
            Body.Controls.Add(Columns(ApcGrid.HeightForRows(ChannelCount), new Control[] { BuildChannelGrid(channels) }));
            Body.Controls.Add(Row(SectionTitle("채널 현황"), conditionButton));
            Body.Controls.Add(Columns(S(90), tiles));
        }

        private static ApcGrid BuildChannelGrid(System.Collections.Generic.IList<ChannelRow> channels)
        {
            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn { HeaderText = "상태", Width = ApcTheme.Px(70) });
            grid.Columns.Add("ch", "채널");
            grid.Columns.Add("sample", "시료 ID");
            grid.Columns.Add("cond", "시험 조건");
            grid.Columns.Add("prog", "진행률(%)");
            grid.Columns.Add("remain", "남은 시간(h:mm)");
            grid.Columns.Add("ir", "절연저항(MΩ)");
            grid.Columns.Add("leak", "누설전류(µA)");
            grid.Columns.Add("text", "내용");
            for (int c = 1; c < 8; c++) grid.Columns[c].Width = ApcTheme.Px(c == 2 ? 170 : 150);
            grid.Columns["text"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns["text"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            foreach (var r in channels)
            {
                int i = grid.Rows.Add(Criteria.ChannelStatus(r), r.Name, r.SampleId, r.Condition, Criteria.F1(r.Progress),
                    r.State == Criteria.Standby ? "" : Criteria.Hm(r.RemainingMinutes),
                    Criteria.F1(r.InsulationMOhm), Criteria.F1(r.LeakageUA), Criteria.StatusText(r));
                MarkCell(grid.Rows[i].Cells["ir"], Criteria.InsulationStatus(r));
                MarkCell(grid.Rows[i].Cells["leak"], Criteria.LeakageStatus(r));
            }
            return grid;
        }

        /// <summary>기준 이탈(경고·알람) 값은 상태색 + 굵게. 선택 행에서도 유지된다.</summary>
        private static void MarkCell(DataGridViewCell cell, StatusKind kind)
        {
            if (kind != StatusKind.Warn && kind != StatusKind.Alarm) return;
            cell.Style.ForeColor = ApcTheme.Status(kind);
            cell.Style.Font = ApcTheme.GetFont(FontStyle.Bold);
        }

        private static ApcGrid BuildDefectGrid(IMainViewSource source)
        {
            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add("ch", "채널");
            grid.Columns.Add("sample", "시료 ID");
            grid.Columns.Add("mode", "불량 모드");
            grid.Columns.Add("time", "판정 시각");
            foreach (DataGridViewColumn col in grid.Columns) col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (var d in source.GetTodayDefects()) grid.Rows.Add(d.Channel, d.SampleId, d.Mode, d.JudgedAt);
            return grid;
        }

        private void OpenDetail(string channel)
        {
            using (var dlg = new ChamberDetailDialog(_source.GetChamberDetail(channel))) dlg.ShowDialog(FindForm());
        }

        private void OpenCondition()
        {
            using (var dlg = new TestConditionDialog())
            {
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    _source.SaveCondition(dlg.ConditionName, dlg.Temperature, dlg.Humidity, dlg.Hours, dlg.Cycles);
            }
        }
    }
}
