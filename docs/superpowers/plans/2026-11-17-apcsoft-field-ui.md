# APCSOFT 현장 UI 라이브러리 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** WinForms 현장 PC 프로그램에 참조만 하면 쓰는 입체 스타일 컨트롤 라이브러리 `APC.UI.dll` + 교체 가능한 테마/이미지 세트 + 기능별 데모 갤러리.

**Architecture:** 순수 로직(테마 로드·검증, 이미지 경로 해석, 스캔 판별)은 WinForms 비의존 클래스로 분리해 콘솔 자가점검(`APC.UI.Checks`)으로 검증한다. 컨트롤은 그 위에 얇게 얹고, 모든 그림은 `themes/<name>/assets/*.png`(SVG 마스터에서 렌더)에서, 색·폰트는 `theme.json`에서 온다.

**Tech Stack:** C# 7.3, .NET Framework 4.0, WinForms, 구식(non-SDK) csproj, VS2019 MSBuild; 에셋 렌더는 Node 24 + `@resvg/resvg-js`(tools 전용).

**Spec:** `docs/superpowers/specs/2026-11-17-apcsoft-field-ui-design.md`

## Global Constraints

- `<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>`, `<LangVersion>7.3</LangVersion>`. `async/await`, `System.Text.Json`, `Task.Run`, per-monitor DPI API 금지.
- 라이브러리 NuGet 의존성 0개. JSON = `System.Runtime.Serialization.Json.DataContractJsonSerializer`.
- csproj는 구식 형식(이 PC엔 .NET SDK 없음). 빌드 명령: `"/c/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe" APC.UI.sln -p:Configuration=Debug -v:m` (이하 `$MSBUILD`).
- 이미지 이름: `{kind}_{name}_{state}@{scale}.png`, kind ∈ `btn|lamp|win|icon|logo`, state ∈ `normal|hover|pressed|disabled`(램프·로고는 state 생략: `lamp_run@1x.png`, `logo_apcsoft@1x.png`), scale ∈ `1x|1.5x|2x`.
- 상태색 의미 고정: run=초록, warn=노랑, alarm=빨강, stop=회색, info=파랑. 테마는 명도만 변경 가능.
- 최소 컨트롤 높이: 28px (96dpi 기준, DPI 배율 곱함).
- 스캔 기본값: 키 간격 ≤ 50ms, 최소 길이 4, 종료문자 Enter.
- 대상은 작업용 PC(1920×1080, 창 크기 가변)뿐. 키오스크/터치 기능(크기 세트, 전체화면 잠금, 숫자패드) 만들지 않음.
- 브랜드 빨강은 로고 이미지에만. 코드/테마에 브랜드 색 토큰 없음.
- `Resources/`는 읽기 전용(참고 원본).
- 이 폴더는 git 저장소가 아님 → 각 Task 끝의 "체크포인트"는 빌드+자가점검 통과 확인이며, 커밋은 사용자가 요청할 때만.

## Review Focus

1. **깨진 theme.json** (파일 없음, JSON 문법 오류, 필드 누락, 상태색 색상 변경) → 어떤 파일·필드가 문제인지 적힌 `ThemeException`. 한글 `fontFamily`("맑은 고딕")는 깨지지 않고 로드. → Task 1 테스트.
2. **특정 배율 이미지 누락** → `@1x`로 대체; `@1x`까지 없으면 기대 경로를 담은 `FileNotFoundException`(빈 버튼으로 조용히 넘어가지 않음). → Task 3 테스트.
3. **125% 배율(96×1.25=120dpi)** → 1x/1.5x 중간값은 큰 쪽(1.5x)을 택해 흐림 방지. → Task 3 테스트.
4. **사람이 빠르게 친 짧은 입력 / 중간에 한 번 느린 키가 낀 입력 / 빈 Enter** → 스캔으로 오인하지 않음. → Task 6 테스트.
5. **디자이너가 버튼 높이를 20px로 지정** → 28px(×DPI 배율)로 올림. → Task 4 테스트.

---

### Task 1: 솔루션 골격 + 테마 로드·검증

**Files:**
- Create: `APC.UI.sln`
- Create: `src/APC.UI/APC.UI.csproj` (Class Library, net40, 참조: System, System.Drawing, System.Windows.Forms, System.Runtime.Serialization)
- Create: `src/APC.UI/Theming/ThemeData.cs`
- Create: `src/APC.UI/Theming/ThemeLoader.cs`
- Create: `src/APC.UI/Theming/ThemeException.cs`
- Create: `src/APC.UI/Theming/StatusKind.cs`
- Create: `src/APC.UI.Checks/APC.UI.Checks.csproj` (Console, net40, 참조: APC.UI, System.Drawing, System.Windows.Forms)
- Create: `src/APC.UI.Checks/Program.cs`
- Create: `themes/default/theme.json`
- Create: `src/APC.UI.Checks/fixtures/` (bad JSON 픽스처들, 빌드 출력으로 복사)

**Interfaces:**
- Produces:
  - `enum StatusKind { Run, Warn, Alarm, Stop, Info }`
  - `[DataContract] class ThemeData` — 멤버(JSON 키는 camelCase): `string Name; string FontFamily; int FontSize; ThemeColors Colors; StatusColors Status; int[] ButtonSlice; ScanSettings Scan {int MaxKeyIntervalMs; int MinLength}; string Logo;`
  - `ThemeColors`: `string[] Background, Header, GridHeader` (각 길이 2 = 그라데이션 시작/끝), `string Border, Text, TextMuted, TextInverse, GridAltRow, GridSelected`.
  - `StatusColors`: `string Run, Warn, Alarm, Stop, Info`.
  - `static class ThemeLoader { static ThemeData Load(string themeDir); static Color ParseColor(string hex); static Color GetStatusColor(ThemeData t, StatusKind k); }`
  - `class ThemeException : Exception` — 메시지에 파일 경로와 필드명 포함.
  - Checks 러너: `static void Check(bool ok, string name)` — 실패 수 집계, 끝에 `FAILED n` 출력 후 exit code = n. 이후 Task들은 `Program.cs`에 `Run<Area>Checks()` 메서드를 추가.

- [ ] **Step 1: `themes/default/theme.json` 작성 (스펙 값 그대로)**

```json
{
  "name": "APCSOFT Default",
  "fontFamily": "맑은 고딕",
  "fontSize": 10,
  "colors": {
    "background": ["#E6E6E6", "#CFCFCF"],
    "header": ["#5A6A7E", "#2F3B4A"],
    "gridHeader": ["#FAFAFA", "#D9DDE2"],
    "border": "#888888", "text": "#1F2937", "textMuted": "#6B7280", "textInverse": "#FFFFFF",
    "gridAltRow": "#F3F4F6", "gridSelected": "#CFE0F5"
  },
  "status": { "run": "#16A34A", "warn": "#EAB308", "alarm": "#DC2626", "stop": "#9CA3AF", "info": "#2563EB" },
  "buttonSlice": [8, 8, 8, 8],
  "scan": { "maxKeyIntervalMs": 50, "minLength": 4 },
  "logo": "logo_apcsoft"
}
```
UTF-8(BOM 없음)으로 저장.

- [ ] **Step 2: 실패하는 점검 작성 (`RunThemeChecks`)**

```csharp
var t = ThemeLoader.Load(Path.Combine(Root, "themes/default"));
Check(t.FontFamily == "맑은 고딕", "theme: korean fontFamily round-trips");
Check(t.Scan.MaxKeyIntervalMs == 50 && t.Scan.MinLength == 4, "theme: scan defaults");
Check(ThemeLoader.GetStatusColor(t, StatusKind.Alarm) == ThemeLoader.ParseColor("#DC2626"), "theme: alarm color");
Check(Throws<ThemeException>(() => ThemeLoader.Load(Fx("missing")), "theme.json"), "theme: missing file names file");
Check(Throws<ThemeException>(() => ThemeLoader.Load(Fx("syntax")), "theme.json"), "theme: bad json");
Check(Throws<ThemeException>(() => ThemeLoader.Load(Fx("no-status")), "status"), "theme: missing field named");
Check(Throws<ThemeException>(() => ThemeLoader.Load(Fx("alarm-blue")), "status.alarm"), "theme: alarm hue changed rejected");
Check(!Throws<ThemeException>(() => ThemeLoader.Load(Fx("alarm-dark")), ""), "theme: darker alarm red accepted");
```
`Root` = 저장소 루트(실행 파일에서 위로 `APC.UI.sln` 찾기), `Fx(n)` = fixtures 폴더, `Throws<T>(action, mustContain)` = T가 던져지고 메시지에 문자열 포함 시 true. 픽스처: `missing/`(빈 폴더), `syntax/`(`{` 하나), `no-status/`(status 키 삭제), `alarm-blue/`(alarm `#2563EB`), `alarm-dark/`(alarm `#991B1B`).

- [ ] **Step 3: 빌드 → 실패 확인**

Run: `$MSBUILD APC.UI.sln ... && src/APC.UI.Checks/bin/Debug/APC.UI.Checks.exe`
Expected: 컴파일 오류(ThemeLoader 없음) 또는 FAILED > 0.

- [ ] **Step 4: `ThemeLoader.Load` 구현**

파일 읽기는 `File.ReadAllBytes` → `MemoryStream` → `DataContractJsonSerializer`(인코딩 자동 감지로 UTF-8 한글 보존). `SerializationException`/`IOException`을 `ThemeException(path + 원인)`으로 감싼다. 필수 필드 null 검사 시 JSON 키 경로(`status`, `colors.header` …)를 메시지에. 상태색 검증(색상 범위는 `Color.GetHue()`/`GetSaturation()`):

| 키 | 허용 |
|---|---|
| run | hue 90–160 |
| warn | hue 40–65 |
| alarm | hue ≥ 345 또는 ≤ 15 |
| info | hue 200–245 |
| stop | saturation ≤ 0.15 |

그라데이션 배열 길이 ≠ 2, `buttonSlice` 길이 ≠ 4도 `ThemeException`.

- [ ] **Step 5: 빌드 + 점검 통과 확인**

Expected: 출력 끝 `ALL PASSED`, exit 0.

- [ ] **Step 6: 체크포인트** — 빌드 경고 0(Nullable 등 무관), 점검 통과.

---

### Task 2: 에셋 파이프라인 (SVG 마스터 → PNG 세트) + 기존 이미지 이관

**Files:**
- Create: `tools/package.json` (devDependency `@resvg/resvg-js`), `tools/render-assets.mjs`
- Create: `assets-src/*.svg` (아래 목록)
- Create(생성물): `themes/default/assets/*.png`
- Create: `themes/default/assets/logo_apcsoft@1x.png` (← `Resources/APCSOFT.png` 복사)

**Interfaces:**
- Produces: `themes/default/assets/`에 아래 파일 전체. 기준(@1x) 크기:
  - `btn_{primary,normal,danger}_{normal,hover,pressed,disabled}` — 48×48, 9-slice 여백 8px 안에 모서리·광택이 들어가게.
  - `win_{close,minimize}_{normal,hover,pressed,disabled}` — 46×32.
  - `lamp_{run,warn,alarm,stop,info}` — 24×24.
  - `logo_apcsoft@1x` — 155×50 (래스터 원본뿐이라 1x만; 1.5x/2x는 회사 벡터 원본 확보 시 추가 → Task 3의 대체 규칙이 처리).

- [ ] **Step 1: SVG 마스터 작성** — 스타일 기준은 `Resources/` 원본과 목업 B(`.superpowers/brainstorm/735-1794878105/content/visual-style.html`의 `.B` 규칙):
  - 램프: `1~4.png`처럼 radialGradient(하이라이트 35%/30% 위치 → 본색 60% → 어두운 테두리) + 1px 어두운 외곽선. 본색 = theme.json status 값. info는 새로 그림.
  - 버튼: 세로 linearGradient(normal `#FDFDFD→#D4D8DE→#BFC5CD`), 1px `#7A7A7A` 테두리, 반경 5, 상단 1px 흰 하이라이트. hover = 밝게 + 테두리 `#5A7FB0`, pressed = 그라데이션 반전 + 내부 그림자, disabled = 불투명도 50% 회색. primary = 청회색 계열(`#7FA3D1→#3E6CA8`, 흰 글자 대비), danger = 빨강 계열(`#F05252→#B91C1C`).
  - 창 버튼: `btn_close.jpg`/`btn_minimize.jpg` 형태(옅은 청회색 라운드 사각 + 진한 ✕/—). hover는 **기존보다 확실히 구분**(밝기 +, 테두리 `#5A7FB0`), close hover는 아이콘만 `#B91C1C`.
- [ ] **Step 2: `render-assets.mjs` 작성** — `assets-src/*.svg` 각 파일을 `{basename}@1x/1.5x/2x.png`로 `fitTo: {mode:'width', value: round(svgWidth*scale)}` 렌더, 출력 `themes/default/assets/`. 인자 `--out <dir>`로 다른 테마 출력, `--override <dir>`가 있으면 같은 basename의 SVG를 그 폴더 것으로 대체(고객사별 일부 이미지만 바꿀 때).
- [ ] **Step 3: 실행**

Run: `cd tools && npm install && node render-assets.mjs`
Expected: `ls themes/default/assets/*.png | wc -l` → **76** (btn 36 + win 24 + lamp 15 + logo 1).
- [ ] **Step 4: 육안 확인** — `lamp_*@2x.png`, `win_close_hover@2x.png`, `btn_primary_pressed@2x.png`를 열어 Resources 원본과 톤이 이어지는지, 투명 배경인지 확인.
- [ ] **Step 5: 체크포인트.**

---

### Task 3: 이미지 해석 + 테마 파사드

**Files:**
- Create: `src/APC.UI/Theming/AssetResolver.cs`
- Create: `src/APC.UI/Theming/ApcTheme.cs`
- Modify: `src/APC.UI.Checks/Program.cs` (`RunAssetChecks`)

**Interfaces:**
- Consumes: `ThemeLoader.Load`, `ThemeData` (Task 1)
- Produces:
  - `static class AssetResolver { static string PickScale(float dpi); static string Resolve(string assetsDir, string baseName, float dpi); }` — `baseName` 예 `"btn_primary_hover"`, `"lamp_run"`. 반환 = 실제 존재하는 전체 경로.
  - `static class ApcTheme { static ThemeData Current; static string ThemeDir; static float Dpi; static void Load(string themeDir); static Image GetImage(string baseName); static Font GetFont(FontStyle style = Regular); static Color Status(StatusKind k); static Color C(string hex); static int MinHeight { get; } static event EventHandler Changed; }` — `MinHeight` = `28 × Dpi/96`.

- [ ] **Step 1: 실패하는 점검**

```csharp
Check(AssetResolver.PickScale(96f) == "1x", "scale 100%");
Check(AssetResolver.PickScale(120f) == "1.5x", "scale 125% rounds up");
Check(AssetResolver.PickScale(144f) == "1.5x", "scale 150%");
Check(AssetResolver.PickScale(168f) == "2x", "scale 175% rounds up");
Check(AssetResolver.PickScale(240f) == "2x", "scale 250% caps at 2x");
var dir = Path.Combine(Root, "themes/default/assets");
Check(AssetResolver.Resolve(dir, "lamp_run", 192f).EndsWith("lamp_run@2x.png"), "resolve 2x");
Check(AssetResolver.Resolve(dir, "logo_apcsoft", 192f).EndsWith("logo_apcsoft@1x.png"), "fallback to 1x");
Check(Throws<FileNotFoundException>(() => AssetResolver.Resolve(dir, "btn_nope_normal", 96f), "btn_nope_normal@1x.png"), "missing names expected path");
```
- [ ] **Step 2: 빌드·실행 → FAIL 확인.**
- [ ] **Step 3: 구현** — `PickScale`: 비율 r=dpi/96; r<1.25→1x, r<1.75→1.5x, 그 외 2x (경계값은 큰 쪽). `ApcTheme.GetImage`는 경로별 `Dictionary<string, Image>` 캐시(`Image.FromStream`으로 읽어 파일 잠금 회피); `Load` 시 캐시 Dispose·초기화 후 `Changed` 발생. `Dpi`는 `Load` 때 `Graphics.FromHwnd(IntPtr.Zero).DpiX`.
- [ ] **Step 4: 통과 확인.**
- [ ] **Step 5: 체크포인트.**

---

### Task 4: 9-slice 그리기 + `ApcButton`

**Files:**
- Create: `src/APC.UI/Drawing/NineSlice.cs`
- Create: `src/APC.UI/Controls/ApcButton.cs`
- Modify: `src/APC.UI.Checks/Program.cs` (`RunButtonChecks`)

**Interfaces:**
- Consumes: `ApcTheme.GetImage`, `ApcTheme.MinHeight`, `ApcTheme.GetFont`, `ThemeData.ButtonSlice`
- Produces:
  - `static class NineSlice { static void Draw(Graphics g, Image img, Rectangle dest, Padding slice, float scale); }` — slice는 @1x 기준 px, 이미지 배율만큼 곱해 원본 좌표로.
  - `enum ApcButtonVariant { Normal, Primary, Danger }`
  - `class ApcButton : Button { ApcButtonVariant Variant; }` — 이미지 이름 `btn_{variant}_{state}`. state: `!Enabled`→disabled, 마우스 누름→pressed, 마우스 위→hover, 그 외 normal. 텍스트 색: Primary/Danger = `textInverse`, Normal = `text`.

- [ ] **Step 1: 실패하는 점검** (테마 로드 후, 화면 표시 없이 인스턴스만)

```csharp
var d = new ApcButton { Height = 20 };
Check(d.Height >= ApcTheme.MinHeight, "button clamps to 28*dpi");
var big = new ApcButton { Height = 80 };
Check(big.Height == 80, "larger height kept");
```
- [ ] **Step 2: FAIL 확인.**
- [ ] **Step 3: 구현** — 최소 높이는 `SetBoundsCore` 오버라이드로 보정(디자이너·코드 모두 적용). 폰트는 `ApcTheme.GetFont()`. `SetStyle(UserPaint|AllPaintingInWmPaint|OptimizedDoubleBuffer|SupportsTransparentBackColor)`. `ApcTheme.Changed` 구독해 `Invalidate`, Dispose 시 해제. 포커스 시 점선 대신 1px `#5A7FB0` 안쪽 사각형(키보드 사용자용).
- [ ] **Step 4: 통과 확인.**
- [ ] **Step 5: 체크포인트.**

---

### Task 5: 상태 표시 — `ApcLamp`, `ApcHeaderBar`, `ApcAlarmBar`

**Files:**
- Create: `src/APC.UI/Controls/ApcLamp.cs`, `ApcHeaderBar.cs`, `ApcAlarmBar.cs`
- Create: `src/APC.UI/Drawing/Gradients.cs`

**Interfaces:**
- Consumes: `ApcTheme.*`, `StatusKind`
- Produces:
  - `static class Gradients { static void Fill(Graphics g, Rectangle r, string[] pair); }` — 세로 `LinearGradientBrush`.
  - `class ApcLamp : Control { StatusKind Status; bool Blink; }` — 이미지 `lamp_{status소문자}`; `Blink && Status==Alarm`이면 500ms `System.Windows.Forms.Timer`로 표시/숨김 토글.
  - `class ApcHeaderBar : Panel { string Title; StatusKind Status; string StatusText; }` — 배경 `colors.header`, 글자 `textInverse`, 오른쪽에 램프+상태 문구.
  - `class ApcAlarmBar : Panel { void Push(string message); void AcknowledgeCurrent(); event EventHandler Acknowledged; int Count; }` — 배경 alarm 계열 그라데이션, 메시지 여러 개면 3초마다 순환, 오른쪽 `확인` `ApcButton`(Normal)으로 현재 메시지 제거 후 `Acknowledged`. `Count==0`이면 `Visible=false`.

- [ ] **Step 1: 점검 추가 (`RunAlarmChecks`)**

```csharp
var bar = new ApcAlarmBar();
Check(!bar.Visible, "alarm bar hidden when empty");
bar.Push("금형 온도 상한 초과"); bar.Push("원자재 부족");
Check(bar.Visible && bar.Count == 2, "alarm bar shows queued");
bar.AcknowledgeCurrent();
Check(bar.Count == 1, "ack removes one");
```
- [ ] **Step 2: FAIL 확인 → Step 3: 구현 → Step 4: 통과 확인.**
- [ ] **Step 5: 체크포인트.**

---

### Task 6: 바코드 — `ScanDetector` + `ApcScanBox`

**Files:**
- Create: `src/APC.UI/Input/ScanDetector.cs`
- Create: `src/APC.UI/Controls/ApcScanBox.cs`
- Modify: `src/APC.UI.Checks/Program.cs` (`RunScanChecks`)

**Interfaces:**
- Consumes: `ThemeData.Scan`
- Produces:
  - `class ScanDetector { ScanDetector(int maxKeyIntervalMs, int minLength); string Feed(char c, long timestampMs); void Reset(); }` — Enter(`'\r'`)에서 버퍼가 조건을 만족하면 문자열 반환, 아니면 null. 그 외 문자는 null.
  - `class ApcScanBox : TextBox { event EventHandler<ScanEventArgs> Scanned; void Attach(Form host); void Flash(bool success); }`, `class ScanEventArgs : EventArgs { string Code; }`

- [ ] **Step 1: 실패하는 점검**

```csharp
string Run(ScanDetector d, string s, params int[] gaps) { long t=0; string r=null; for(int i=0;i<s.Length;i++){ t+= i<gaps.Length?gaps[i]:10; r=d.Feed(s[i],t);} return r; }
var d = new ScanDetector(50, 4);
Check(Run(d, "LOT-0032\r") == "LOT-0032", "fast burst is scan");
d.Reset(); Check(Run(d, "abc\r") == null, "short fast input not scan");
d.Reset(); Check(Run(d, "LOT-0032\r", 10,10,10,200,10,10,10,10,10) == null, "one slow gap breaks scan");
d.Reset(); Check(Run(d, "\r") == null, "empty enter ignored");
d.Reset(); Run(d, "abc\r"); Check(Run(d, "LOT-0033\r") == "LOT-0033", "detector recovers after rejection");
```
- [ ] **Step 2: FAIL 확인.**
- [ ] **Step 3: 구현** — 간격 > max이면 버퍼를 해당 문자부터 새로 시작. Enter 판정 후 버퍼 항상 비움. `ApcScanBox.Attach(host)`: `host.KeyPreview = true`, `host.KeyPress`에서 `Environment.TickCount`로 `Feed`; 스캔 성공 시 `e.Handled = true`(포커스된 다른 입력칸에 바코드가 섞여 들어가지 않게), `Text = code`, `Scanned` 발생, `Flash(true)`. `Flash`: 배경을 run/alarm 밝은 톤으로 300ms 바꾸고 `SystemSounds.Beep`(성공) / `SystemSounds.Hand`(실패) — 실패 호출은 사용 측 검증 실패 시. 테두리 2px info 색으로 "스캔 대기" 표시.
- [ ] **Step 4: 통과 확인.**
- [ ] **Step 5: 체크포인트.**

---

### Task 7: `ApcForm` (테두리 없는 창) + 확인 대화상자

**Files:**
- Create: `src/APC.UI/Controls/ApcForm.cs`
- Create: `src/APC.UI/Controls/ApcDialog.cs` — 스펙 불변원칙 "파괴적 동작은 확인 단계 필수"용. `static bool ApcDialog.Confirm(IWin32Window owner, string message)`: `ApcForm` 기반 모달, `취소`(Normal, 기본 포커스·Esc) / `확인`(Danger). 기본 포커스를 취소에 둬서 Enter·바코드 Enter로 실수 확인되지 않게.

**Interfaces:**
- Consumes: `ApcTheme.GetImage("win_*")`, `ApcTheme.GetImage(Current.Logo)`, `Gradients.Fill`
- Produces: `class ApcForm : Form { string TitleText; protected Rectangle ContentBounds; }`
  - `FormBorderStyle.None`, 높이 40px(×DPI) 타이틀바(헤더 그라데이션, 왼쪽 로고·제목, 오른쪽 `win_minimize`/`win_close` 이미지 버튼 4상태), 타이틀바 드래그로 이동(`WM_NCLBUTTONDOWN`+`HTCAPTION`), 더블클릭 최대화/복원, 가장자리 6px 리사이즈(`WM_NCHITTEST`), 1px `border` 테두리. 최대화 시 작업표시줄을 가리지 않게 `MaximizedBounds = Screen.FromControl(this).WorkingArea`.

- [ ] **Step 1: 데모 없이 확인 가능한 최소 점검** (`RunFormChecks`) — `new ApcForm().FormBorderStyle == None`; `WindowState=Maximized` 후 `Bounds`가 `WorkingArea`를 넘지 않음.
- [ ] **Step 2: FAIL → 구현 → 통과.**
- [ ] **Step 3: 체크포인트** (드래그·리사이즈·hover는 Task 9 데모에서 육안 확인).

---

### Task 8: `ApcGrid` + `ApcLampColumn`

**Files:**
- Create: `src/APC.UI/Controls/ApcGrid.cs`, `ApcLampColumn.cs`

**Interfaces:**
- Produces:
  - `class ApcGrid : DataGridView` — 생성 시 `EnableHeadersVisualStyles=false`, 헤더 `gridHeader` 그라데이션(`CellPainting`), 짝수행 `gridAltRow`, 선택 `gridSelected`, 행 높이 `ApcTheme.MinHeight`, `SelectionMode=FullRowSelect`, `RowHeadersVisible=false`, `AllowUserToAddRows=false`.
  - `class ApcLampColumn : DataGridViewColumn` + `ApcLampCell` — 셀 값 `StatusKind`를 `lamp_*` 이미지로 가운데 그림. 값이 null/DBNull이면 빈 셀(예외 없음).

- [ ] **Step 1: 점검** (`RunGridChecks`): `ApcGrid` 생성 후 `RowTemplate.Height >= ApcTheme.MinHeight`; `ApcLampColumn` 셀에 `null`을 넣고 `GetFormattedValue` 호출 시 예외 없음.
- [ ] **Step 2: FAIL → 구현 → 통과.**
- [ ] **Step 3: 체크포인트.**

---

### Task 9: 데모 갤러리 + 샘플 테마

**Files:**
- Create: `src/APC.UI.Demo/APC.UI.Demo.csproj` (WinExe, net40, app.manifest에 `<dpiAware>true</dpiAware>`)
- Create: `src/APC.UI.Demo/Program.cs` — 인자 `--theme <name>`(기본 `default`)으로 `ApcTheme.Load(Root/themes/<name>)`; `ThemeException`이면 메시지박스로 원인 표시 후 종료.
- Create: `src/APC.UI.Demo/GalleryForm.cs` — `ApcForm`, 시작 크기 1600×900. 왼쪽 목록: `컨트롤 카탈로그` + 템플릿 5개. 오른쪽에 선택 화면 표시.
- Create: `src/APC.UI.Demo/Catalog/ControlCatalog.cs` — 모든 컨트롤 × 상태(normal/hover 안내/disabled) 나열, 램프 5종(alarm은 Blink), `ApcDialog.Confirm` 호출 버튼.
- Create: `src/APC.UI.Demo/Templates/*.cs` — 스펙 §5.5 표 그대로 5개 `UserControl`, 각각 상단 `ApcHeaderBar` + 하단 `ApcAlarmBar` 포함:
  - `WorkStartView`, `ProductionView`(ScanBox→Grid 행 추가→F9 저장, 행 삭제는 `ApcDialog.Confirm`), `DefectView`, `EquipmentView`(LampColumn), `HistoryView`(기간 `DateTimePicker` 2개 + 그리드 + `엑셀 내보내기` 버튼 자리, 비활성)
  - 가짜 데이터는 각 템플릿 안 정적 배열.
- Create: `themes/sample/theme.json` (default 복사, header/background/primary 계열을 녹회색으로, alarm은 `#B91C1C`로 명도만 변경) + `themes/sample/assets/` (`node tools/render-assets.mjs --out ../themes/sample/assets` 후 primary 버튼 SVG 색만 바꿔 재렌더 — `assets-src/sample/btn_primary_*.svg` 덮어쓰기 규칙: `--override assets-src/sample`).

- [ ] **Step 1: 빌드** — Expected: 3개 프로젝트 빌드 성공, `APC.UI.Checks.exe` → `ALL PASSED`.
- [ ] **Step 2: 수동 확인 (default)** — `APC.UI.Demo.exe` 실행: 카탈로그의 모든 버튼 hover/pressed 변화, 창 드래그·리사이즈·최대화, `ProductionView`에서 메모장에 `LOT-0032`를 빠르게 붙여넣는 대신 실제 스캐너(없으면 `ScanDetector` 점검으로 갈음)로 행 추가, F9 저장 메시지.
- [ ] **Step 3: 수동 확인 (테마 교체)** — `APC.UI.Demo.exe --theme sample`: **코드 변경 없이** 색·버튼 이미지가 바뀜. `themes/sample/theme.json`의 alarm을 `#2563EB`로 바꿔 실행 → 원인 필드가 적힌 메시지 후 종료.
- [ ] **Step 4: 수동 확인 (DPI)** — Windows 배율 125% / 150% / 200%에서 로그오프 후 실행, 버튼·램프·창 버튼이 흐리거나 잘리지 않는지 (로고는 1x만 있으므로 확대 흐림 허용 — 벡터 원본 확보 시 해결).
- [ ] **Step 5: 체크포인트 + `docs/README.md` 작성**: 새 프로젝트에서 쓰는 법 5줄(`APC.UI.dll` 참조 → `themes/<고객사>` 폴더 복사 → `ApcTheme.Load` → 폼을 `ApcForm` 상속 → 이미지 재렌더 명령).
