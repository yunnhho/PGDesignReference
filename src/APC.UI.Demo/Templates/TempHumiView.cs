using System;
using System.Drawing;
using System.Windows.Forms;
using APC.UI.Controls;
using APC.UI.Drawing;
using APC.UI.Theming;

namespace APC.UI.Demo.Templates
{
    /// <summary>
    /// references/TempHumi.png 재구성: 상단 연결 정보 + 페이지 이동, 센서 카드 격자.
    /// 원본의 빨강(온도)/파랑(습도) 값 색은 상태색과 겹쳐서 쓰지 않는다 — 값은 기본 글자색,
    /// 범위를 벗어날 때만 경고/알람 색을 입힌다. 연결 상태는 빨간 사각형 대신 ApcLamp.
    /// </summary>
    internal sealed class TempHumiView : TemplateView
    {
        private static readonly object[][] Sensors =
        {
            // 이름, 온도, 습도, 상태, 하단 문구
            new object[] { "LineA SensorNM0", null, null, StatusKind.Stop, "수신 대기 중..." },
            new object[] { "LineA SensorNM1", 23.4, 41.0, StatusKind.Run, "11:20:10 수신" },
            new object[] { "LineA SensorNM2", 24.1, 39.5, StatusKind.Run, "11:20:10 수신" },
            new object[] { "LineB SensorNM0", 29.8, 58.2, StatusKind.Warn, "온도 상한 근접 (기준 30.0℃)" },
            new object[] { "LineB SensorNM1", 33.6, 61.0, StatusKind.Alarm, "온도 상한 초과 (기준 30.0℃)" },
            new object[] { "LineB SensorNM2", null, null, StatusKind.Stop, "연결 끊김 · 10:58:02 마지막 수신" },
        };

        public TempHumiView() : base("온습도 모니터링")
        {
            Header.Status = StatusKind.Alarm;
            Header.StatusText = "알람 1 · 경고 1 · 끊김 2";

            var prev = Button("◀ 이전", ApcButtonVariant.Normal, 80);
            var next = Button("다음 ▶", ApcButtonVariant.Normal, 80);
            prev.Enabled = next.Enabled = false;   // 1 / 1 페이지
            var page = Caption("1 / 1");
            page.Margin = new Padding(S(4), S(8), S(12), 0);
            var log = Button("로그", ApcButtonVariant.Normal, 80);
            log.Margin = new Padding(S(24), 0, 0, 0);
            var paging = new FlowLayoutPanel { Dock = DockStyle.Right, AutoSize = true, WrapContents = false, BackColor = Color.Transparent };
            paging.Controls.AddRange(new Control[] { prev, page, next, log });

            var info = Row(Info("SMG IP"), Value("192.168.100.180"), Info("Port"), Value("502"), Info("데이터 수집 주기"), Value("10초"));
            var infoBar = new Panel { Dock = DockStyle.Top, Height = S(46), BackColor = Color.Transparent };
            info.Dock = DockStyle.Fill;
            infoBar.Controls.Add(info);
            infoBar.Controls.Add(paging);

            var cards = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent };
            foreach (var s in Sensors)
                cards.Controls.Add(new SensorCard((string)s[0], s[1] as double?, s[2] as double?, (StatusKind)s[3], (string)s[4]));

            Body.Controls.Add(cards);
            Body.Controls.Add(infoBar);
            foreach (var s in Sensors) if ((StatusKind)s[3] == StatusKind.Alarm) Alarm.Push(s[0] + ": " + s[4]);
        }

        private static Label Info(string text)
        {
            var l = Caption(text);
            l.Font = ApcTheme.GetFont();
            l.ForeColor = ApcTheme.C(ApcTheme.Current.Colors.TextMuted);
            l.Margin = new Padding(0, S(8), S(6), 0);
            return l;
        }

        private static Label Value(string text)
        {
            var l = Caption(text);
            l.Margin = new Padding(0, S(8), S(28), 0);
            return l;
        }

        /// <summary>센서 카드. 제목 띠(그리드 헤더 그라데이션) + 상태 램프, 큰 값 2개, 하단 상태 문구.</summary>
        private sealed class SensorCard : Control
        {
            private const double TempWarn = 29.0, TempAlarm = 30.0;

            private readonly string _title;
            private readonly double? _temp;
            private readonly double? _humi;
            private readonly StatusKind _status;
            private readonly string _footer;

            public SensorCard(string title, double? temp, double? humi, StatusKind status, string footer)
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                _title = title; _temp = temp; _humi = humi; _status = status; _footer = footer;
                Size = new Size(S(340), S(210));
                Margin = new Padding(0, 0, S(16), S(16));
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                var c = ApcTheme.Current.Colors;
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                int pad = S(14), titleH = S(40);

                g.Clear(Color.White);
                Gradients.Fill(g, new Rectangle(1, 1, Width - 2, titleH), c.GridHeader);
                using (var pen = new Pen(ApcTheme.C(c.Border)))
                {
                    g.DrawRectangle(pen, rect);
                    g.DrawLine(pen, 0, titleH, Width, titleH);
                }
                // 알람 카드는 테두리를 굵은 알람색으로 — 멀리서도 어느 카드인지 보이게
                if (_status == StatusKind.Alarm)
                    using (var pen = new Pen(ApcTheme.Status(StatusKind.Alarm), S(3))) g.DrawRectangle(pen, S(1), S(1), Width - S(3), Height - S(3));

                var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
                int lamp = S(20);
                g.DrawImage(ApcTheme.GetImage("lamp_" + _status.ToString().ToLowerInvariant()), new Rectangle(Width - pad - lamp, (titleH - lamp) / 2, lamp, lamp));
                TextRenderer.DrawText(g, _title, ApcTheme.GetFont(FontStyle.Bold), new Rectangle(pad, 0, Width - pad * 3 - lamp, titleH), ApcTheme.C(c.Text), flags);

                int half = (Width - pad * 2) / 2, top = titleH + S(12);
                DrawValue(g, "온도", _temp, "℃", new Point(pad, top), half, TempColor());
                DrawValue(g, "습도", _humi, "%", new Point(pad + half, top), half, ApcTheme.C(c.Text));

                int lineY = Height - S(44);
                using (var pen = new Pen(ApcTheme.C(c.GridLine))) g.DrawLine(pen, pad, lineY, Width - pad, lineY);
                var footColor = _status == StatusKind.Alarm || _status == StatusKind.Warn ? ApcTheme.Status(_status) : ApcTheme.C(c.TextMuted);
                TextRenderer.DrawText(g, _footer, ApcTheme.GetFont(), new Rectangle(pad, lineY, Width - pad * 2, Height - lineY), footColor, flags);
            }

            private Color TempColor()
            {
                if (_temp >= TempAlarm) return ApcTheme.Status(StatusKind.Alarm);
                if (_temp >= TempWarn) return ApcTheme.Status(StatusKind.Warn);
                return ApcTheme.C(ApcTheme.Current.Colors.Text);
            }

            private static void DrawValue(Graphics g, string label, double? value, string unit, Point at, int width, Color color)
            {
                var muted = ApcTheme.C(ApcTheme.Current.Colors.TextMuted);
                TextRenderer.DrawText(g, label, ApcTheme.GetFont(), new Point(at.X, at.Y), muted);
                using (var big = new Font(ApcTheme.Current.FontFamily, ApcTheme.Current.FontSize * 2.6f, FontStyle.Bold))
                using (var unitFont = new Font(ApcTheme.Current.FontFamily, ApcTheme.Current.FontSize * 1.3f))
                {
                    string text = value.HasValue ? value.Value.ToString("0.0") : "--";
                    var pos = new Point(at.X - S(4), at.Y + S(22));
                    TextRenderer.DrawText(g, text, big, pos, value.HasValue ? color : muted);
                    var size = TextRenderer.MeasureText(g, text, big);
                    TextRenderer.DrawText(g, unit, unitFont, new Point(pos.X + size.Width - S(6), pos.Y + size.Height - S(28)), muted);
                }
            }
        }
    }
}
