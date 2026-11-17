# APC.UI API (v1.1.0.0)

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

## ApcDialog

`APC.UI.Controls` · static class

되돌릴 수 없는 동작(삭제, 작업 종료 등) 확인. 기본 포커스와 Enter·Esc는 모두 '취소' — 바코드 스캔의 Enter나 습관적 Enter로 실수 확인되지 않게 한다.
- `static bool Confirm(IWin32Window owner, string message)`

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

테마 스타일을 입힌 DataGridView. 헤더 그라데이션, 행 배경 한 색(번갈아 칠하지 않음), 행 전체 선택.
- `ApcGrid()`

## ApcHeaderBar

`APC.UI.Controls` · class : Panel

화면 상단 띠: 왼쪽 제목(설비명 등), 오른쪽 상태 램프 + 상태 문구.
- `ApcHeaderBar()`
- `StatusKind Status { get; set; }`
- `string StatusText { get; set; }`
- `string Title { get; set; }`

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

## ApcScanBox

`APC.UI.Controls` · class : TextBox

바코드 입력란. Attach(form) 하면 그 폼 어디에 포커스가 있든 스캔을 받는다. 스캔으로 판정되면 포커스 칸을 버스트 시작 전 값으로 되돌리고 코드를 여기로 옮긴 뒤 Scanned를 낸다. 스캔의 Enter는 삼켜서 폼의 기본 버튼(AcceptButton)을 누르지 않게 한다. 피드백은 Scanned 처리 후 한 번: e.Accepted에 따라 성공/실패.
- `ApcScanBox()`
- `const int FlashMs`
- `event EventHandler<ScanEventArgs> Scanned`
- `void Attach(Form host)`
- `void Flash(bool success)` — 성공 = 초록 플래시 + 짧은 비프, 실패 = 빨강 플래시 + 경고음.

## ApcTemplateView

`APC.UI.Controls` · class : UserControl

모든 기능 화면의 공통 틀: 상단 ApcHeaderBar, 하단 ApcAlarmBar, 가운데 Body.
- `protected ApcTemplateView(string title)`
- `protected readonly ApcAlarmBar Alarm`
- `protected readonly ApcHeaderBar Header`
- `protected readonly Panel Body`
- `protected static ApcButton Button(string text, ApcButtonVariant variant, int width = 110)`
- `protected static FlowLayoutPanel Row(params Control[] controls)` — 왼쪽→오른쪽으로 쌓는 한 줄(필터·버튼 줄).
- `protected static Label Caption(string text)`
- `protected static int S(int px)`

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
