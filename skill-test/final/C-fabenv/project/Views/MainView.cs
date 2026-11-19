using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Theming;
using FabEnvMonitor.Data;

namespace FabEnvMonitor.Views
{
    /// <summary>메인 화면: 베이 12곳 카드 격자(4×3) + 최근 이탈 이력. 카드를 누르면 베이 상세, 설정 버튼은 상하한 설정.</summary>
    internal sealed class MainView : ApcTemplateView
    {
        private const int Columns4 = 4;
        private readonly IMainViewSource _source;
        private readonly List<ApcValueCard> _cards = new List<ApcValueCard>();

        public MainView() : this(new SampleMainViewSource()) { }

        public MainView(IMainViewSource source) : base("클린룸 환경 감시")
        {
            _source = source;
            IList<BayReading> bays = source.GetBays();

            var rows = new List<Control>();
            for (int r = 0; r < bays.Count / Columns4; r++)
            {
                var cells = new Control[Columns4];
                for (int c = 0; c < Columns4; c++)
                {
                    int index = r * Columns4 + c;
                    var card = new ApcValueCard { Dock = DockStyle.Fill, ShowLamp = true, Cursor = Cursors.Hand };
                    card.Click += (s, e) => OpenDetail(index);
                    _cards.Add(card);
                    cells[c] = card;
                }
                rows.Add(Columns(S(120), cells));
            }

            var grid = new ApcGrid { Dock = DockStyle.Fill, ReadOnly = true };
            grid.Columns.Add(new ApcLampColumn());
            grid.Columns.Add("time", "시각");
            grid.Columns.Add("bay", "베이");
            grid.Columns.Add("text", "내용");
            grid.Columns["text"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns["text"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            foreach (var e in source.GetRecentExcursions())
                grid.Rows.Add(e.Status, e.Time.ToString("HH:mm:ss"), e.Bay, e.Text);

            var hint = Row(Caption("베이 카드를 누르면 상세를 볼 수 있습니다."));
            hint.Dock = DockStyle.Fill;
            var settings = Button("설정", ApcButtonVariant.Normal, 110);
            settings.Dock = DockStyle.Right;
            settings.Click += (s, e) => OpenSettings();
            var bar = new Panel { Dock = DockStyle.Top, Height = S(46), BackColor = Color.Transparent };
            bar.Controls.Add(hint);
            bar.Controls.Add(settings);

            // Top 도킹: 아래쪽 것부터 Add
            Body.Controls.Add(grid);
            Body.Controls.Add(Row(SectionTitle("최근 이탈 이력")));
            for (int i = rows.Count - 1; i >= 0; i--) Body.Controls.Add(rows[i]);
            Body.Controls.Add(bar);

            UpdateCards();
            IList<BaySetting> st = source.GetSettings();
            for (int i = 0; i < bays.Count; i++)
            {
                BayEvaluation ev = EnvJudge.Evaluate(bays[i], st[i]);
                if (ev.Status == StatusKind.Alarm) Alarm.Push(bays[i].Name + ": " + (ev.Temp != null && ev.Temp.Status == StatusKind.Alarm ? ev.Temp.Detail
                    : ev.Humi != null && ev.Humi.Status == StatusKind.Alarm ? ev.Humi.Detail : ev.Particle.Detail));
            }
        }

        /// <summary>카드 12장과 헤더 요약을 현재 값·설정으로 다시 채운다.</summary>
        private void UpdateCards()
        {
            IList<BayReading> bays = _source.GetBays();
            IList<BaySetting> st = _source.GetSettings();
            var evals = new List<BayEvaluation>();
            for (int i = 0; i < bays.Count; i++)
            {
                BayEvaluation ev = EnvJudge.Evaluate(bays[i], st[i]);
                evals.Add(ev);
                Fill(_cards[i], bays[i], ev);
            }
            StatusKind worst;
            Header.StatusText = EnvJudge.Summarize(evals, out worst);
            Header.Status = worst;
        }

        private static void Fill(ApcValueCard card, BayReading r, BayEvaluation ev)
        {
            card.Title = r.Name + " · " + ev.StateText;
            card.Unit = "℃";
            card.Value = r.Temp.HasValue ? EnvJudge.Fmt(EnvMetric.Temp, r.Temp.Value) : "";
            card.Status = ev.Status;
            card.ValueStatus = ev.Temp != null ? ev.Temp.Status : (StatusKind?)null;
            if (r.State == BayState.Disconnected)
            {
                card.Footer = r.Note;
                card.FooterStatus = null;
                return;
            }
            card.Footer = "습도 " + EnvJudge.Fmt(EnvMetric.Humi, r.Humi.Value) + " %RH · 파티클 " + EnvJudge.Fmt(EnvMetric.Particle, r.Particle.Value) + " 개/m³";
            card.FooterStatus = Worse(ev.Humi, ev.Particle);
        }

        private static StatusKind? Worse(Verdict a, Verdict b)
        {
            if (a == null || b == null) return null;
            if (a.Status == StatusKind.Alarm || b.Status == StatusKind.Alarm) return StatusKind.Alarm;
            if (a.Status == StatusKind.Warn || b.Status == StatusKind.Warn) return StatusKind.Warn;
            return StatusKind.Run;
        }

        private void OpenDetail(int index)
        {
            BayReading r = _source.GetBays()[index];
            using (var dlg = new BayDetailDialog(r, _source.GetSettings()[index],
                _source.GetTrend24h(r.Name, EnvMetric.Temp), _source.GetTrend24h(r.Name, EnvMetric.Humi)))
                dlg.ShowDialog(FindForm());
        }

        private void OpenSettings()
        {
            using (var dlg = new BaySettingsDialog(_source.GetSettings()))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                _source.SaveSettings(dlg.Result);
                UpdateCards();
            }
        }
    }
}
