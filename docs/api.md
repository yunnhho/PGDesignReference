# APC.UI API (v1.3.1.0)

> build.sh가 자동 생성한다. 직접 고치지 않는다.

## ApcAlarmBar

`APC.UI.Controls` · class : Panel

하단 알람 띠. 들어온 순서대로 쌓고, 여러 개면 3초마다 순환 표시. '확인'은 현재 보이는 알람 하나를 지운다. 비면 숨는다.
- `ApcAlarmBar()`
- `const int CycleIntervalMs`
- `event EventHandler Acknowledged`
- `int Count { get; }`
- `string CurrentMessage { get; }`
- `void AcknowledgeCurrent()`
- `void Push(string message)`

## ApcButton

`APC.UI.Controls` · class : Button

9-slice 이미지 버튼. 이미지: btn_{variant}_{normal|hover|pressed|disabled}
- `ApcButton()`
- `ApcButtonVariant Variant { get; set; }`

## ApcButtonVariant

`APC.UI.Controls` · enum

- Normal, Primary, Danger

## ApcComboBox

`APC.UI.Controls` · class : ComboBox

테마 선택 목록(드롭다운 전용, 직접 입력 없음). 닫힌 상태는 입력칸 모양 + 오른쪽 그라데이션 화살표 버튼으로 직접 그리고, 펼친 목록은 항목 높이 28px×배율 이상(장갑 낀 손가락·마우스 모두 누르기 쉽게). 높이는 ItemHeight로 정해진다.
- `ApcComboBox()`

## ApcDateBox

`APC.UI.Controls` · class : UserControl

날짜 입력: 입력칸(yyyy-MM-dd) + 달력 버튼. 버튼을 누르면 달력이 펼쳐지고, 날짜를 누르면 닫힌다. 직접 입력은 yyyy-MM-dd / yyyyMMdd를 받아 Enter 또는 칸을 떠날 때 확정, 틀리면 이전 값. ↑↓ 키는 하루씩.
- `ApcDateBox()`
- `ApcTextBox TextBox { get; }`
- `DateTime Value { get; set; }`
- `const string Format`
- `event EventHandler ValueChanged`
- `protected void OnValueChanged(EventArgs e)`
- `static readonly DateTime MaxDate` — 받는 범위. 달력 이동·하루 이동이 DateTime 끝에서 예외를 내지 않게.
- `static readonly DateTime MinDate` — 받는 범위. 달력 이동·하루 이동이 DateTime 끝에서 예외를 내지 않게.
- `void Commit()` — 입력칸 글자를 확정한다. 날짜가 아니면 이전 값을 다시 보여 준다.

## ApcDialog

`APC.UI.Controls` · static class

되돌릴 수 없는 동작(삭제, 작업 종료 등) 확인. 기본 포커스와 Enter·Esc는 모두 '취소' — 바코드 스캔의 Enter나 습관적 Enter로 실수 확인되지 않게 한다.
- `static bool Confirm(IWin32Window owner, string message)` — 파괴적 동작 확인: 확인 버튼은 빨간 "확인".
- `static bool Confirm(IWin32Window owner, string message, string okText, ApcButtonVariant okVariant)` — 확인 버튼 글자·종류 지정. 저장처럼 위험하지 않은 확인은 ("저장", Primary).
- `static void Alert(IWin32Window owner, string message)` — 알림(입력 오류 안내 등). 버튼은 "확인" 하나, Enter·Esc로 닫힌다.

## ApcForm

`APC.UI.Controls` · class : Form

테두리 없는 APCSOFT 창. 타이틀바(로고·제목·최소화·닫기 이미지 버튼), 드래그 이동, 더블클릭 최대화, 가장자리 리사이즈. 최대화해도 작업표시줄을 가리지 않는다.
- `ApcForm()`
- `Rectangle ContentBounds { get; }` — 타이틀바·테두리(리사이즈 그립 포함)를 뺀 내용 영역(클라이언트 좌표) = Padding 안쪽.
- `bool Resizable { get; set; }`
- `int ResizeGrip { get; }` — 가장자리 리사이즈 감지 폭(px).
- `int TitleBarHeight { get; }`
- `protected CreateParams CreateParams { get; }`
- `string TitleText { get; set; }`

## ApcGrid

`APC.UI.Controls` · class : DataGridView

테마 스타일을 입힌 DataGridView. 헤더 그라데이션, 행 배경 한 색(번갈아 칠하지 않음), 행 전체 선택. 사용자가 표를 누르거나 키로 움직이기 전에는 첫 행이 저절로 선택되지 않는다(CurrentRow = null) — "선택한 행에 대해" 동작하는 버튼은 CurrentRow가 null이면 아무것도 하지 않게 둔다.
- `ApcGrid()`

## ApcHeaderBar

`APC.UI.Controls` · class : Panel

화면 상단 띠: 왼쪽 제목(설비명 등), 오른쪽 상태 램프 + 상태 문구.
- `ApcHeaderBar()`
- `StatusKind Status { get; set; }`
- `string StatusText { get; set; }`
- `string Title { get; set; }`

## ApcJudgePanel

`APC.UI.Controls` · class : Control

판정 표시판: 상태색 바탕에 큰 글자(OK / NG / READY 등) + 아래 한 줄 안내. 판정 화면의 가장 큰 요소로 쓴다. OK=Run, NG=Alarm, 대기=Stop, 확인 필요=Warn. 글자 크기는 높이에 맞춘다.
- `ApcJudgePanel()`
- `StatusKind Status { get; set; }`
- `string SubText { get; set; }` — 큰 글자 아래 한 줄(예: "시리얼을 스캔하세요", "하중 19.4 kN > 상한 18.0").
- `string Text { get; set; }` — 큰 글자(OK / NG / READY 등).

## ApcLamp

`APC.UI.Controls` · class : Control

상태 램프. 이미지: lamp_{run|warn|alarm|stop|info}. Blink는 Alarm일 때만 동작.
- `ApcLamp()`
- `StatusKind Status { get; set; }`
- `bool Blink { get; set; }`
- `const int BlinkIntervalMs`

## ApcLampCell

`APC.UI.Controls` · class : DataGridViewTextBoxCell

- `ApcLampCell()`
- `Type ValueType { get; }`

## ApcLampColumn

`APC.UI.Controls` · class : DataGridViewColumn

값이 StatusKind인 셀을 상태 램프 이미지로 그리는 컬럼. 그 외 값(null 등)은 빈 셀.
- `ApcLampColumn()`

## ApcNumericBox

`APC.UI.Controls` · class : UserControl

숫자 입력: [−] 입력칸 [+]. 버튼·↑↓ 키·마우스 휠은 Increment만큼, 직접 입력은 Enter 또는 칸을 떠날 때 확정. 범위 밖은 Minimum/Maximum으로 잘리고, 숫자가 아니면 이전 값으로 되돌린다. 소수 자릿수는 DecimalPlaces.
- `ApcNumericBox()`
- `ApcTextBox TextBox { get; }` — 안쪽 입력칸(포커스 지정·스캔 복원 대상).
- `decimal Increment { get; set; }`
- `decimal Maximum { get; set; }`
- `decimal Minimum { get; set; }`
- `decimal Value { get; set; }`
- `event EventHandler ValueChanged`
- `int DecimalPlaces { get; set; }`
- `protected void OnValueChanged(EventArgs e)`
- `void Commit()` — 입력칸 글자를 확정한다. 숫자가 아니면 이전 값을 다시 보여 준다.

## ApcScanBox

`APC.UI.Controls` · class : ApcTextBox

바코드 입력란. Attach(form) 하면 그 폼 어디에 포커스가 있든 스캔을 받는다. 스캔으로 판정되면 포커스 칸을 버스트 시작 전 값으로 되돌리고 코드를 여기로 옮긴 뒤 Scanned를 낸다. 스캔의 Enter는 삼켜서 폼의 기본 버튼(AcceptButton)을 누르지 않게 한다. 피드백은 Scanned 처리 후 한 번: e.Accepted에 따라 성공/실패.
- `ApcScanBox()`
- `const int FlashMs`
- `event EventHandler<ScanEventArgs> Scanned`
- `void Attach(Form host)` — 이 폼에서 스캔을 받기 시작한다. 화면(UserControl) 생성자에서는 아직 폼이 없으므로 화면의 OnHandleCreated에서 FindForm()으로 부른다: var f = FindForm(); if (f != null) _scan.Attach(f);
- `void Flash(bool success)` — 성공 = 초록 플래시 + 짧은 비프, 실패 = 빨강 플래시 + 경고음.

## ApcStatusTile

`APC.UI.Controls` · class : Control

설비 타일: 램프 + 설비명 + 상태 문구. 누르면 Click(상세 dialog 열기 등). 마우스를 올리거나 포커스면 파란 테두리, 알람이면 굵은 알람색 테두리. Tab으로 옮겨 Enter·Space로도 누른다.
- `ApcStatusTile()`
- `StatusKind Status { get; set; }`
- `string StatusText { get; set; }` — 설비명 아래 한 줄(예: "가동 · 정상", "알람 · 온도 상한 초과").
- `string Text { get; set; }` — 설비명(굵게, 램프 오른쪽 윗줄).

## ApcTemplateView

`APC.UI.Controls` · class : UserControl

모든 기능 화면의 공통 틀: 상단 ApcHeaderBar, 하단 ApcAlarmBar, 가운데 Body.
- `protected ApcTemplateView(string title)`
- `protected readonly ApcAlarmBar Alarm`
- `protected readonly ApcHeaderBar Header`
- `protected readonly Panel Body` — 내용 영역(Padding 16px). 도킹 순서: 같은 Dock(Top 등)끼리는 나중에 Add한 것이 바깥(위)에 온다. Fill을 먼저 Add하고, Top 줄들은 화면 아래쪽 것부터 위쪽 것 순서로 Add한다(또는 SetChildIndex(fill, 0)).
- `protected static ApcButton Button(string text, ApcButtonVariant variant, int width = 110)`
- `protected static FlowLayoutPanel Row(params Control[] controls)` — 왼쪽→오른쪽으로 쌓는 한 줄(필터·버튼 줄).
- `protected static Label Caption(string text)`
- `protected static int S(int px)` — 배율 적용 px. ApcForm dialog 등 이 클래스 밖에서는 ApcTheme.Px.

## ApcTextBox

`APC.UI.Controls` · class : TextBox

테마 입력칸. 오목한 테두리·포커스 테두리는 비클라이언트 영역에 직접 그리고, 글자는 높이 가운데에 온다. 높이를 자유롭게 줄 수 있다(기본 34px×배율, 최소 28px×배율). TextBox를 상속하므로 Text·ReadOnly·MaxLength 등은 그대로. Multiline이면 스크롤바 자리가 필요해 Windows 기본 한 줄 테두리로 돌아간다.
- `ApcTextBox()`
- `const int BaseHeight`
- `protected void ResetFieldColor()` — 바탕색을 상태(사용 가능·읽기 전용)에 맞춘다. 스캔 플래시 등으로 바꾼 색을 되돌릴 때도 쓴다.

## ApcTrendChart

`APC.UI.Controls` · class : Control

추이 꺾은선. SetValues로 시간순 값(왼쪽=과거), XLabels로 가로축 글자(양끝·가운데 등, 고르게 배치). Reference = 설정값(회색 점선), UpperLimit/LowerLimit = 상·하한(알람색 점선). 세로축은 값과 기준선을 모두 담게 자동으로 잡는다.
- `ApcTrendChart()`
- `double? LowerLimit { get; set; }`
- `double? Reference { get; set; }` — null·NaN·무한대 = 선 없음.
- `double? UpperLimit { get; set; }`
- `string[] XLabels { get; set; }`
- `void SetValues(IEnumerable<double> values)`

## ApcValueCard

`APC.UI.Controls` · class : Control

현재값 카드: 제목 띠(그리드 헤더 그라데이션, ShowLamp면 오른쪽에 램프) + 큰 값·단위 + 아래 한 줄(기준·범위·수신 시각 등). 값 색은 Status로: 경고/알람만 상태색, 정지는 흐리게. 알람이면 굵은 알람색 테두리. Value가 비면 "--".
- `ApcValueCard()`
- `HorizontalAlignment ValueAlign { get; set; }`
- `StatusKind Status { get; set; }`
- `bool ShowLamp { get; set; }`
- `string Footer { get; set; }` — 아래 한 줄. 비우면 줄이 없다.
- `string Title { get; set; }`
- `string Unit { get; set; }`
- `string Value { get; set; }`

## ScanEventArgs

`APC.UI.Controls` · class : EventArgs

- `ScanEventArgs(string code)`
- `bool Accepted { get; set; }` — 앱이 코드를 거부하면 false로 둔다 → 실패 피드백(빨강 + 경고음). 기본 true.
- `string Code { get; }`

## Gradients

`APC.UI.Drawing` · static class

- `static void Fill(Graphics g, Rectangle r, Color top, Color bottom)`
- `static void Fill(Graphics g, Rectangle r, string[] pair)` — 세로 그라데이션. pair = [위, 아래] (#RRGGBB)

## NineSlice

`APC.UI.Drawing` · static class

모서리는 그대로, 변·가운데만 늘려 그린다. 둥근 모서리·광택이 크기와 무관하게 유지된다.
- `static void Draw(Graphics g, Image img, Rectangle dest, Padding slice, float imageScale)`

## ScanDetector

`APC.UI.Input` · class

바코드 스캐너 입력 판별. 스캐너는 키를 사람보다 훨씬 빠르게 연달아 보내고 Enter로 끝낸다. 모든 키 간격(Enter 포함)이 maxKeyIntervalMs 이하이고 길이가 minLength 이상이면 스캔.
- `ScanDetector(int maxKeyIntervalMs, int minLength)`
- `const char Terminator`
- `int Length { get; }` — 현재 쌓인 버스트 길이. Feed 직후 1이면 새 버스트가 시작된 것.
- `string Feed(char c, long timestampMs)` — 글자 하나를 넣는다. Enter에서 스캔으로 판정되면 코드, 아니면 null.
- `void Reset()`

## ApcTheme

`APC.UI.Theming` · static class

현재 테마 한 곳. 앱 시작 시 Load 한 번, 이후 컨트롤들이 여기서 색·폰트·이미지를 가져간다.
- `const int BaseMinHeight`
- `static Color C(string hex)`
- `static Color Status(StatusKind kind)`
- `static Font GetFont(FontStyle style = 0)`
- `static Image GetImage(string baseName)` — 캐시된 이미지. 호출 측에서 Dispose 하지 않는다.
- `static ThemeData Current { get; }`
- `static event EventHandler Changed`
- `static float Dpi { get; }`
- `static float ImageScale(Image img)` — 이미지가 몇 배율 파일인지(9-slice 여백 환산용).
- `static float Scale { get; }`
- `static int MinHeight { get; }`
- `static int Px(int px)` — 96dpi 기준 px을 현재 배율로. 화면·dialog 어디서나 크기·여백에 쓴다(ApcTemplateView의 S()와 같다).
- `static string AssetsDir { get; }`
- `static string ThemeDir { get; }`
- `static void Load(string themeDir)`

## AssetResolver

`APC.UI.Theming` · static class

이미지 이름 → 실제 파일 경로. 규칙: {baseName}@{1x|1.5x|2x}.png
- `const float BaseDpi`
- `const string FallbackScale`
- `static string PickScale(float dpi)` — 경계값(125%, 175%)은 큰 쪽을 택한다 — 축소가 확대보다 덜 흐리다.
- `static string Resolve(string assetsDir, string baseName, float dpi)`

## ScanSettings

`APC.UI.Theming` · class

- `ScanSettings()`
- `int MaxKeyIntervalMs { get; set; }`
- `int MinLength { get; set; }`

## StatusColors

`APC.UI.Theming` · class

- `StatusColors()`
- `string Alarm { get; set; }`
- `string Info { get; set; }`
- `string Run { get; set; }`
- `string Stop { get; set; }`
- `string Warn { get; set; }`

## StatusKind

`APC.UI.Theming` · enum

상태 의미. 색상(hue)은 고객사 테마에서도 바꿀 수 없다.
- Run, Warn, Alarm, Stop, Info

## ThemeColors

`APC.UI.Theming` · class

- `ThemeColors()`
- `string Border { get; set; }`
- `string GridLine { get; set; }`
- `string GridSelected { get; set; }`
- `string Text { get; set; }`
- `string TextInverse { get; set; }`
- `string TextMuted { get; set; }`
- `string[] Background { get; set; }` — 그라데이션 [시작, 끝]
- `string[] GridHeader { get; set; }`
- `string[] Header { get; set; }`
- `string[] TitleBar { get; set; }` — 창 타이틀바(로고가 올라가므로 밝은 톤)

## ThemeData

`APC.UI.Theming` · class

theme.json 매핑. JSON 키는 camelCase.
- `ScanSettings Scan { get; set; }`
- `StatusColors Status { get; set; }`
- `ThemeColors Colors { get; set; }`
- `ThemeData()`
- `int FontSize { get; set; }`
- `int[] ButtonSlice { get; set; }`
- `string FontFamily { get; set; }`
- `string Logo { get; set; }`
- `string Name { get; set; }`

## ThemeException

`APC.UI.Theming` · class : Exception

theme.json을 읽을 수 없거나 규칙 위반. 메시지에 파일 경로와 필드명을 담는다.
- `ThemeException(string message)`
- `ThemeException(string message, Exception inner)`

## ThemeLoader

`APC.UI.Theming` · static class

- `const string FileName`
- `static Color GetStatusColor(ThemeData t, StatusKind k)`
- `static Color ParseColor(string hex)`
- `static ThemeData Load(string themeDir)`
