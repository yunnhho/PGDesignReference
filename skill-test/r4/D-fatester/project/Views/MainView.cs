using System.Collections.Generic;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using FATester.Data;

namespace FATester.Views
{
    /// <summary>메인 화면: 채널 타일(누르면 설비 상세) → 채널별 현재값 표 → 불량 분류 결과 표. 데이터는 IMainViewSource에서만 받는다.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const int ChannelCount = 4;

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("FA 시험 현황")
        {
            IList<ChannelRow> channels = source.GetChannels();

            var tiles = new Control[channels.Count];
            for (int i = 0; i < channels.Count; i++)
            {
                ChannelRow ch = channels[i];
                var tile = new ApcStatusTile { Text = ch.Name, Status = ch.Status, StatusText = ch.StatusText, Dock = DockStyle.Fill };
                tile.Click += (s, e) => OpenDetail(ch);
                tiles[i] = tile;
                if (ch.Status == StatusKind.Alarm) Alarm.Push(ch.Name + ": " + ch.StatusText);
            }

            ApplyHeader(channels);

            // Top 도킹은 나중에 Add한 것이 위 → 불량 표(Fill) 먼저, 그다음 아래 줄부터
            Body.Controls.Add(BuildDefectGrid(source.GetDefects()));
            Body.Controls.Add(Row(SectionTitle("불량 분류 결과")));
            Body.Controls.Add(Columns(ApcGrid.HeightForRows(ChannelCount), new Control[] { BuildChannelGrid(channels) }));
            Body.Controls.Add(Row(SectionTitle("채널별 현재값")));
            Body.Controls.Add(Columns(S(90), tiles));
        }

        private void ApplyHeader(IList<ChannelRow> channels)
        {
            int alarm = 0, warn = 0, wait = 0, stop = 0;
            foreach (ChannelRow c in channels)
            {
                if (c.Status == StatusKind.Alarm) alarm++;
                else if (c.Status == StatusKind.Warn) warn++;
                else if (c.Status == StatusKind.Stop) { if (c.ProcessText == "대기") wait++; else stop++; }
            }
            var parts = new List<string> { "전체" };
            if (alarm > 0) parts.Add("알람 " + alarm);
            if (warn > 0) parts.Add("경고 " + warn);
            if (wait > 0) parts.Add("대기 " + wait);
            if (stop > 0) parts.Add("정지 " + stop);
            if (parts.Count == 1) parts.Add("정상");
            Header.StatusText = string.Join(" · ", parts.ToArray());
            Header.Status = alarm > 0 ? StatusKind.Alarm : warn > 0 ? StatusKind.Warn : (wait + stop) > 0 ? StatusKind.Stop : StatusKind.Run;
        }

        private static ApcGrid BuildChannelGrid(IList<ChannelRow> channels)
        {
            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn { HeaderText = "상태" });
            grid.Columns.Add("name", "채널");
            grid.Columns.Add("item", "시험 항목");
            grid.Columns.Add("sample", "시료 ID");
            grid.Columns.Add("progress", "진행률(%)");
            grid.Columns.Add("res", "저항(mΩ)");
            grid.Columns.Add("leak", "누설전류(nA)");
            grid.Columns.Add("text", "판정");
            grid.Columns["text"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns["text"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            int[] widths = { 80, 110, 150, 190, 130, 130, 160 };
            for (int i = 0; i < widths.Length; i++) grid.Columns[i].Width = ApcTheme.Px(widths[i]);

            foreach (ChannelRow c in channels)
            {
                int r = grid.Rows.Add(c.Status, c.Name, c.TestItem, c.SampleId,
                    c.Progress.HasValue ? c.Progress.Value.ToString() : "",
                    c.Resistance.HasValue ? c.Resistance.Value.ToString("0.0") : "",
                    c.Leak.HasValue ? c.Leak.Value.ToString("0.0") : "",
                    c.StatusText);
                if (c.Resistance.HasValue) MarkIfAbnormal(grid.Rows[r].Cells["res"], ChannelRules.JudgeResistance(c.Resistance.Value));
                if (c.Leak.HasValue) MarkIfAbnormal(grid.Rows[r].Cells["leak"], ChannelRules.JudgeLeak(c.Leak.Value));
            }
            return grid;
        }

        private static void MarkIfAbnormal(DataGridViewCell cell, StatusKind kind)
        {
            if (kind == StatusKind.Run) return;
            cell.Style.ForeColor = ApcTheme.Status(kind);
            cell.Style.Font = ApcTheme.GetFont(System.Drawing.FontStyle.Bold);
        }

        private static ApcGrid BuildDefectGrid(IList<DefectRow> defects)
        {
            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add("sample", "시료 ID");
            grid.Columns.Add("mode", "불량 모드");
            grid.Columns.Add("pos", "위치");
            grid.Columns.Add("time", "판정 시각");
            foreach (DataGridViewColumn c in grid.Columns) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (DefectRow d in defects) grid.Rows.Add(d.SampleId, d.Mode, d.Position, d.JudgedAt);
            return grid;
        }

        private void OpenDetail(ChannelRow channel)
        {
            using (var dlg = new ChannelDetailDialog(channel)) dlg.ShowDialog(FindForm());
        }
    }
}
