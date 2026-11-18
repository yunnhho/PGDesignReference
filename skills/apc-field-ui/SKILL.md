---
name: apc-field-ui
description: Use when building a new APCSOFT field PC program screen in WinForms (.NET Framework 4.x) — 현장 PC 프로그램, 설비 모니터링, 라인별/센서 모니터링, 공정 설비 프로그램(Failure Analysis 등), 새 화면·새 프로젝트, APC.UI, ApcForm, ApcGrid. Not for WPF, web, or .NET 6/8 projects.
---

# APC 현장 UI 화면 만들기

APCSOFT 현장 프로그램 화면은 **APC.UI 라이브러리**로 만든다. 원본 저장소: `C:\00P_PGDesignReference` (이하 `$REPO`, Git Bash 경로 `/c/00P_PGDesignReference`. 다른 위치에 클론했으면 `tools/install-skill.sh`가 이 문서의 경로를 실제 위치로 바꿔 설치한다).
결과물: 샘플 데이터가 들어간 **메인 화면 하나 + (필요하면) dialog** 껍데기와, 데이터 연결용 **인터페이스 자리**. 실제 PLC/DB/시리얼 연결 코드는 만들지 않는다.

## 1. 입력 확인

| 필수 | 선택 |
|---|---|
| **목적**: 모니터링(라인 전체 / 설비 부착 센서 등) 또는 공정 설비 프로그램(Failure Analysis 등) | 예전 프로그램 캡처 |
| **메인 기능** | 고객사 요구서 파일 |
| **상세 dialog 유무** | 추가 상세 요청 |

필수 중 빠진 것만 한 번에 묻는다. 셋 다 있으면 바로 진행한다. 요구서가 있으면 메인 기능 목록을 거기서 보완한다. 고객사명이 없으면 `default`로 하고 그렇게 했다고 말한다. 고객사명이 있어도 저장소에 그 테마가 없으면 `new-project.sh`가 `알림: ... themes/default 를 복사했습니다`를 출력한다 — 이것도 사용자에게 전한다.

## 2. 프로젝트 판별과 준비

먼저 저장소 도구가 빌드돼 있어야 한다: `$REPO/src/APC.UI.Tools/bin/Debug/APC.UI.Tools.exe`가 없으면 `bash /c/00P_PGDesignReference/build.sh`를 실행한다(`ALL PASSED` 확인).

| 작업 폴더 상태 | 할 일 |
|---|---|
| csproj 없음 | `bash /c/00P_PGDesignReference/tools/new-project.sh <폴더> <프로젝트명> <고객사> [--framework v4.x]` (기본 v4.0, v4.0~v4.8.1. 폴더가 없으면 스크립트가 만든다) |
| 구형 csproj, `TargetFrameworkVersion` v4.x, WinForms | 같은 명령에 `--into-existing` (dll·테마·참조만 추가, 재실행 시 dll만 교체) |
| SDK 형식(`<Project Sdk=`), net6/8, WPF, 웹 | **중단.** APC.UI는 .NET Framework 4.x WinForms 전용이라고 알린다 |

`--into-existing`일 때 스크립트가 하지 않는 두 가지를 직접 한다:
- `Main`에서 첫 폼을 띄우기 전에 `ApcTheme.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"themes\<고객사>"))`를 호출하고 `ThemeException`이면 메시지를 보여 주고 끝낸다. 없으면 앱이 시작하자마자 죽는다(render는 테마를 직접 로드하므로 이 누락을 잡지 못한다).
- csproj가 `<Compile Include="파일.cs" />`를 하나씩 나열하면, 새로 만든 `.cs`마다 항목을 추가한다(`**\*.cs` 와일드카드면 불필요).

코드는 **그 프로젝트의** `LangVersion`/`TargetFrameworkVersion`을 따른다(v4.x면 C# 7.3까지: `record`, `init`, `string?`, switch 식, `new()` 금지).

파일은 Write 도구로 하나씩 만든다. 한글이 든 긴 Bash heredoc 여러 개를 한 번에 보내면 따옴표 파싱 오류로 아무것도 만들어지지 않는다.

## 3. API 파악

`$REPO/docs/api.md`(자동 생성 API 요약)를 읽는다. 컨트롤 소스 전체를 읽지 않는다. 배치 참고용 템플릿은 아래에서 고른 1~2개만 `$REPO/src/APC.UI.Demo/Templates/`에서 읽는다.

## 4. 배치

배치는 생성자 코드로만 한다(디자이너 파일 없음). 크기는 배율 적용: 화면(`ApcTemplateView`)에서는 `S(px)`, dialog(`ApcForm`) 등 그 밖에서는 `ApcTheme.Px(px)`. 캡처가 있으면 원본의 정보 배치(무엇이 어디)는 유지하고 모양만 APC.UI로 바꾼다.

### 4a. 메인 화면

| 목적 | 출발 템플릿 |
|---|---|
| 모니터링: 라인 전체(설비 타일 + 현재값 카드 + 알람 이력, 타일 누르면 상세) | `EquipmentView` |
| 모니터링: 센서 값 카드·추이 | `TempHumiView` |
| 공정 설비 프로그램: 설비 한 대 판정(스캔 → OK/NG, 측정값, 금일 집계, 이력) | `JudgeView` |
| 공정 설비 프로그램: 여러 설비 분석표 | `FailureAnalysisView` |
| 작업 입력: 시작·실적·불량 | `WorkStartView`, `ProductionView`, `DefectView` |
| 조회 | `HistoryView` |

- **템플릿 가져오기**: 템플릿은 데모 프로젝트의 `internal sealed` 클래스라 참조할 수 없다. 읽고 프로젝트에 새 파일로 옮겨 쓴다 — 네임스페이스를 프로젝트 것으로, 클래스 이름을 화면에 맞게, 코드에 박힌 샘플 값(배열·`Rows.Add` 반복)은 `SampleMainViewSource`로 옮기고 화면은 `IMainViewSource`에서 받는다. `Columns()`·`Row()`·`Caption()`·`SectionTitle()`·`Button()`·`S()`는 `ApcTemplateView`에 있으니 옮기지 않는다.
- 맞는 템플릿이 없거나 추가 요청이 있으면 `ApcTemplateView`(헤더 + Body + 알람바) 위에 **기존 Apc 컨트롤로 새 배치**를 짠다.
- `new-project.sh`가 만드는 `MainView`·`IMainViewSource`는 빈 틀(TODO)이다. 여기에 채운다. `MainForm.TitleText`도 화면 이름으로 바꾼다(기본값은 영문 프로젝트명).
- **도킹 순서**: 같은 Dock끼리는 나중에 `Add`한 것이 바깥(위)에 온다. `Body`에는 Fill 컨트롤을 먼저 `Add`하고, Top 줄은 화면 아래쪽 것부터 위쪽 것 순서로 `Add`한다(또는 `Body.Controls.SetChildIndex(fill, 0)`).
- 카드·타일·판정판을 한 줄에: `ApcTemplateView`의 `Columns(S(높이), cells)`(같은 폭) 또는 `Columns(S(높이), cells, new[] { 50f, 25f, 25f })`(퍼센트 폭). 칸 간격·줄 간격은 헬퍼가 정한다.
- `ApcGrid`는 사용자가 누르기 전엔 선택 행이 없다(`CurrentRow == null`). "선택한 행" 버튼은 null이면 아무것도 하지 않게 한다.

### 4b. dialog

- **dialog는 `ApcForm`을 상속한 별도 폼**이다(`ApcDialog`는 확인창만 띄우는 static 클래스라 상속할 수 없다). 생성자에서 `TitleText`, `Resizable = false`, `MinimizeBox = false`, `ShowInTaskbar = false`, `StartPosition = FormStartPosition.CenterParent`, `ClientSize`를 정하고, 내용은 `ContentBounds.Top` 아래에 둔다. 메인에서 `dlg.ShowDialog(FindForm())`.
- 템플릿에서 출발한다(모두 `src/APC.UI.Demo/Templates/`, 갤러리 "dialog 템플릿"에서 열어 볼 수 있다). 메인 화면과 같은 방법으로 옮겨 쓴다:

| dialog 유형 | 템플릿 |
|---|---|
| 보기용 상세(상태 줄 + 센서 표 + 추이 + 닫기) | `EquipmentDetailDialog` |
| 입력 폼(이름표+입력칸 행, 검증 `Alert` → `Confirm("저장")` → 결과 속성 → OK) | `RecipeDialog` |
| 관리자 비밀번호 확인(설정 진입 전 등) | `AdminPasswordDialog` |
| 칸·슬롯별 LOT 입력(표에 바로 스캔, 칸 수 가변, 빈 칸 Alert) | `LotInputDialog` |
| 통신·장비 설정(블록별 `ApcGroup` 묶음 + 공통 설정 + 비밀번호 변경) | `SettingsDialog` |

- **dialog 배치는 `ApcForm` 헬퍼로만 한다.** 숫자(여백·행 높이·버튼 크기·`ClientSize`)를 직접 정하지 않는다 — 치수는 `ApcLayout`이 정하고 표시 직전에 한 번에 배치된다. 생성자에서 위→아래 순서로 부른다:

| 헬퍼 | 하는 일 |
|---|---|
| `SetupDialog(title, width[, height])` | 제목·크기 고정·작업표시줄 숨김·부모 가운데. 폭은 배율 전 px. height 생략 = 내용으로 자동 |
| `AddFieldRow(caption, field[, trailing...])` | 이름표 + 입력칸 한 줄. 이름표 열은 dialog 전체에 하나(가장 긴 이름표 폭) → 입력칸 왼쪽 끝이 모두 같다. trailing이 없으면 입력칸이 남은 폭을 채우고, 있으면(`FieldCaption("신호")`, 입력칸, 버튼…) 각자 `Width`로 왼→오 |
| `AddGroup(title)` → `AddFieldRow(group, ...)` | 관련 행 묶음(`ApcGroup`). 묶음 사이 16 > 행 사이 12로 구분된다 |
| `AddContent(c)` / `AddContentFill(c)` | 표·상태 줄·그래프. `AddContent`는 `c.Height`(`ApcTheme.Px`) 그대로, `AddContentFill`은 남은 높이(`SetupDialog`에 height 필수, 하나만) |
| `SectionTitle(text)` | 구획 제목(굵게). `AddContent(SectionTitle("..."))` |
| `SetFooter(cancel, confirm[, left...])` | 오른쪽 아래 **[취소][확정]**(확정이 오른쪽 끝, 110×40), 왼쪽 보조 버튼(`비밀번호 변경` 등). Esc = 취소. 보기용은 `SetFooter(null, 닫기)` + 닫기에 `DialogResult = OK`(그래야 Esc도 닫기). 확정 버튼 하나뿐인 입력 dialog는 Esc가 없다 — 취소 버튼을 둔다. Enter(`AcceptButton`)는 스캔이 없는 dialog에서만 직접 지정 |

- 헬퍼는 **생성자에서만** 부른다(표시 직전 한 번 배치, 그 뒤 호출은 예외). 이름표는 `AddFieldRow`가 만든다(보통 굵기·테마 글꼴). `Label`·`Font`·`Panel` 버튼 줄을 직접 만들지 않는다. 한 dialog가 폭에 안 들어가면(render "부모 밖으로 넘침") `SetupDialog` 폭을 늘린다.
- `Columns()`·`Row()`·`S()`는 `ApcTemplateView` 것이라 dialog에서는 쓸 수 없다(dialog 크기는 `ApcTheme.Px`).

### 4c. 컨트롤

| 화면 요소 | 컨트롤 |
|---|---|
| 글자 입력 / 바코드 | `ApcTextBox` / `ApcScanBox`: 화면의 `OnHandleCreated`에서 `var f = FindForm(); if (f != null) _scan.Attach(f);`(생성자엔 폼이 없다) |
| 선택 목록 / 숫자 / 날짜 | `ApcComboBox` / `ApcNumericBox`(Minimum·Maximum·DecimalPlaces·Increment, ▲/▼는 칸 안 오른쪽에 위아래로) / `ApcDateBox` |
| 버튼 | `ApcButton`(Normal·Primary·Danger) |
| 표 | `ApcGrid` + 상태 열 `ApcLampColumn`(셀 값 = `StatusKind`) |
| 큰 현재값(센서·수량·불량률) | `ApcValueCard`(Title·Value·Unit·Footer·Status) |
| 설비 상태 한 칸, 누르면 상세 | `ApcStatusTile`(Text=설비명·Status·StatusText, `Click`) |
| OK/NG 판정 크게 | `ApcJudgePanel`(Text=큰 글자·SubText·Status: OK=Run, NG=Alarm, 대기=Stop) |
| 추이 그래프 | `ApcTrendChart`(`SetValues`, XLabels, Reference, UpperLimit/LowerLimit) |
| 상태 램프 | `ApcLamp` |
| 설정 묶음(제목 있는 상자, GroupBox 대신) | `ApcGroup`(Text=제목, 안은 Dock/FlowLayoutPanel) |
| 입력 표(칸에 바로 입력·스캔) | `ApcGrid { ReadOnly = false, ClearInitialSelection = false }` + 열별 `ReadOnly`(읽기 전용 열은 자동 회색) |

- **표준 컨트롤 금지**: `Button`/`TextBox`/`ComboBox`/`NumericUpDown`/`DateTimePicker`/`DataGridView`/`Form`과 `Control`을 상속해 직접 그린 카드·타일·그래프를 만들지 않는다. 위 표의 Apc 컨트롤을 쓴다. 표준으로 쓰는 것은 `Label`과 배치용 `Panel`/`FlowLayoutPanel`/`TableLayoutPanel`뿐. 표에 없는 요소가 꼭 필요하면 만들지 말고 사용자에게 알린다(라이브러리에 추가할 후보).
- 확인: 되돌릴 수 없는 동작은 `ApcDialog.Confirm(this, "...")`(빨간 확인), 저장처럼 위험하지 않은 것은 `ApcDialog.Confirm(this, "...", "저장", ApcButtonVariant.Primary)`. 입력 오류 안내는 `ApcDialog.Alert(this, "...")`. 메인 화면의 이상 알림은 `Alarm.Push("...")`(알람바).
- 상태 표시는 `StatusKind`(Run/Warn/Alarm/Stop/Info) + `ApcLamp`/`ApcLampColumn`. 색은 테마가 정한다.

## 4-1. 디테일 기준 (사용자 확정, 요구서가 다르게 정하면 요구서)

| 항목 | 기준 |
|---|---|
| 창 제목 | 화면 이름만(`MainForm.TitleText`, 예: "도장 라인 모니터링"). 고객사·버전 붙이지 않음 |
| 날짜·시각 | 날짜 `yyyy-MM-dd`, 당일 이력 시각 `HH:mm:ss`, 여러 날에 걸친 이력 `yyyy-MM-dd HH:mm:ss` |
| 소수 자리 | ℃·%RH·kN·bar·kPa·m/min·A·V → `0.0`, mm → `0.00`, 불량률·가동률 % → `0.0`, 수량 → `#,0`(정수). 카드 값·표·알람 문구·기준 문구 모두 같은 자리수 |
| 표 정렬 | `ApcGrid` 기본이 가운데(헤더 포함). 긴 글 열(알람 내용·비고·사유)만 `DefaultCellStyle.Alignment = MiddleLeft`. 숫자 열도 가운데 |
| 표 열 폭 | 긴 글 열이 있으면 그 열 하나만 `AutoSizeMode = Fill`. 짧은 값 열(ID·시각·숫자·판정)뿐이면 램프 열 빼고 모두 Fill로 고르게 나눈다(한 열만 Fill이면 나머지가 오른쪽 끝으로 밀린다. render가 "표 폭 절반 넘게 차지"로 잡는다) |
| 값 없음(대기·통신 전) | 빈칸(정지 설비라도 읽은 값이 있으면 그 값). `ApcValueCard.Value = ""`(값·단위 모두 안 그림), 표 셀은 `""`. `-`, `--`, `0` 쓰지 않음 |
| 대기 중 직전 값 | 판정 화면이 대기(READY)인데 측정값 카드에 직전 검사 값을 남기면 `Footer`에 출처를 적는다(예: "직전 검사 CELL0260418 · 10:15:00"). 출처 없이 남기지 않는다 |
| 상태 경계값 | 상·하한과 같은 값은 그 상태에 넣는다(경고 근접·한계 판정은 `>=`/`<=`). 상태 문구·램프·카드 색이 같은 판정 함수 하나를 쓴다 |
| 글꼴 | 테마 글꼴·`fontSize` 그대로(10). `new Font(...)`·`FontSize * 배수` 금지, 글자는 `ApcTheme.GetFont()`만. 굵게는 구획 제목(`SectionTitle`)·묶음 제목(`ApcGroup`)·값 강조에만 — 입력칸 앞 이름표(`Caption`, `AddFieldRow`)는 보통 굵기. 판정판·카드 큰 글자는 컨트롤이 정함 |
| 단위 표기 | 입력은 이름표 괄호 `하중 상한 (kN)`, 표시는 `ApcValueCard.Unit`·표 열 제목 `최대 하중(kN)`. 값 칸 안에 단위를 붙이지 않음 |
| 간격 | 숫자로 정하지 않는다. 화면은 `Columns()`·`Row()`(줄 사이 12), dialog는 4b 헬퍼(여백 16·행 사이 12·묶음 사이 16·버튼 110×40). 값은 `ApcLayout` |
| 샘플 데이터 | 이력·목록 표는 표 높이만큼 행을 채운다. 행 수는 짐작하지 말고 첫 render가 알려 주는 수로 맞춘다("참고: 표 아래가 빔 … N개로", "표 세로 스크롤바 … N개로"). 설비 목록처럼 개수가 정해진 표는 그대로. 늘린 행은 같은 문구·같은 간격을 되풀이하지 않는다(설비·문구·값·시각 간격을 섞는다). 상태는 정상(Run) 위주에 그 화면에서 뜻이 있는 이상 상태를 섞는다(모니터링: Warn·Alarm·Stop 각각 하나 이상, 판정: NG(Alarm) 몇 건). 집계(금일 생산·OK·NG·불량률)는 서로 맞고 이력 행 수 이상 |

## 5. 새 그림이 필요할 때만

1. 프로젝트의 `assets-src/`에 SVG를 만든다. 이름 규칙 `{btn|lamp|win|icon|logo}_{이름}_{normal|hover|pressed|disabled}.svg`(램프·로고는 상태 없음), `<svg width="..">` 필수. 기존 SVG(`$REPO/assets-src/`)를 본떠 그린다.
2. `$REPO/tools/node_modules`가 없으면 먼저 `cd /c/00P_PGDesignReference/tools && npm install`(한 번만). 그다음
   `node /c/00P_PGDesignReference/tools/render-assets.mjs --out <프로젝트>/themes/<고객사>/assets --override <프로젝트>/assets-src`
3. 기존 이름을 대체한 그림은 컨트롤이 자동으로 쓴다. 새 이름은 `ApcTheme.GetImage("icon_xxx_normal")`로 쓴다.

## 6. 데이터 자리

메인 화면은 `Data/IMainViewSource`(인터페이스)에만 의존하고, `Data/SampleMainViewSource`가 샘플을 준다. 화면에 맞게 인터페이스 메서드와 행 모델을 바꾼다. 화면에는 **매개변수 없는 생성자**(샘플 사용)를 남긴다 — render가 쓴다. dialog는 메인 화면에서 값을 넘겨받고 자체 인터페이스를 두지 않는다. dialog에도 **샘플 값을 넣는 매개변수 없는 생성자**를 둔다 — 없으면 render할 수 없다. 샘플 값은 서로 맞춘다(알람 문구의 기준값 = 설정값 표 = 그래프 기준선, 추이 마지막 점 = 현재값).

## 7. 완료 확인

1. MSBuild로 빌드: `. /c/00P_PGDesignReference/tools/env.sh && "$MSBUILD" <sln> -v:m -nologo -noconlog "-flp:logfile=build.log;verbosity=minimal;encoding=utf-8;summary"; tail -8 build.log` (`env.sh`가 설치된 Visual Studio/Build Tools에서 MSBuild를 찾는다. 못 찾으면 그 메시지를 사용자에게 전한다) (한글 VS의 MSBuild 콘솔 출력은 CP949와 UTF-8이 섞여 깨진다. `PreferredUILang`·`iconv`로는 안 된다 → UTF-8 로그 파일로 읽는다) — 마지막 요약 `경고 0개`·`오류 0개`(영문 VS면 `0 Warning(s)`·`0 Error(s)`)를 확인한다. 경고도 고친다.
2. 메인 폼과 각 dialog를 PNG로(형식 이름은 네임스페이스 없이 클래스 이름만, 예: `MainForm`, `RecipeDialog`):
   `/c/00P_PGDesignReference/src/APC.UI.Tools/bin/Debug/APC.UI.Tools.exe render <bin/Debug/앱.exe> MainForm <bin/Debug/themes/고객사> <out.png>` (Windows 경로는 `cygpath -w`)
   `--size`는 생략한다: 크기 고정 dialog(`Resizable = false`)는 자기 크기, 메인 폼·화면은 1920×1080으로 그린다. 출력 폴더는 없으면 만든다.
   예: `T=/c/00P_PGDesignReference/src/APC.UI.Tools/bin/Debug/APC.UI.Tools.exe; $T render "$(cygpath -w bin/Debug/App.exe)" RecipeDialog "$(cygpath -w "bin/Debug/themes/고객사")" "$(cygpath -w out/RecipeDialog.png)"`
   캡처를 받았다면 `--compare <캡처.png>` → `<out>.compare.png`
3. **render 점검 0건까지 고친다.** render는 배치를 검사해 `점검: <컨트롤 경로>: <문제>`와 마지막 줄 `점검 N건`을 출력한다 — 글자 잘림, 부모 밖 넘침, 형제 겹침, 같은 줄 입력칸·버튼 위/높이 어긋남, 표준 컨트롤·직접 그린 `Control`, 표 스크롤바, 짧은 값 열이 표 폭 절반 넘게 차지, [확인][취소] 순서, 글자 전체 선택된 채 표시(숫자칸·날짜칸 안쪽 포함), 테마와 다른 글꼴, 입력 행 이름표 굵게, 입력칸 왼쪽 끝 어긋남, 입력 행 간격 다름, dialog 하단 버튼 크기·하단 여백·버튼 위 빈 공간. 고치고 다시 render해서 `점검 0건`을 확인한다. `참고:` 줄은 건수에 들지 않지만 4-1 샘플 기준대로 판단한다.
4. PNG를 직접 열어 아래를 눈으로 확인한다(도구가 못 보는 것). 걸리면 고치고 다시 render:
   - 4-1 기준(창 제목, 날짜·소수 형식, 빈 값, 긴 글 열만 왼쪽)
   - 샘플 값 정합(알람 문구 기준값 = 설정값 = 그래프 기준선, 추이 마지막 점 = 현재값, 집계 합)
   - 상태가 색만이 아니라 글(상태 문구·판정 글자)로도 보이는가, 한 화면에 Warn·Alarm·Stop이 다 나오는가
   - 상한·하한 바로 옆 값(근접 경고 등)의 램프 색이 상태 문구와 같은가(경계값 판정)
   - 대기 화면 카드에 남긴 값이 있으면 Footer에 직전 값이라는 출처가 있는가
   - 이력 표의 늘린 행이 같은 문구·같은 간격의 반복으로 보이지 않는가
   - 큰 빈 공간·한쪽으로 쏠린 배치가 없는가(dialog는 `ClientSize`를 내용에 맞춘다)
5. 사용자에게 PNG 경로와 함께 보여 주고, 마지막 render의 `점검 0건`·빌드 경고 0을 보고한다.

x86 앱: 위 도구는 64비트로 돌아서 `PlatformTarget`이 x86인 앱이나 생성자에서 32비트 네이티브 DLL을 부르는 화면은 "어셈블리를 읽을 수 없습니다"로 실패한다. 그때는 같은 인자로 32비트 도구 `/c/00P_PGDesignReference/src/APC.UI.Tools/bin/Debug-x86/APC.UI.Tools.exe`를 쓴다(`build.sh`가 함께 만든다). 32비트 도구로도 실패하면(네이티브 DLL·장비가 이 PC에 없음 등) 빌드 성공까지만 확인하고 render를 못 한 이유를 사용자에게 말한다.

## 하지 않는 것

| 하지 않음 | 대신 |
|---|---|
| 표준 입력·표·폼 컨트롤, `Control` 상속 직접 그리기 | 4단계 표의 Apc 컨트롤 |
| 상태색 색상 변경(run=초록, warn=노랑, alarm=빨강, stop=회색, info=파랑) | 명도만 고객사 `theme.json`에서 |
| `$REPO/themes/default`, `$REPO/assets-src/*.svg` 수정 | 프로젝트의 `themes/<고객사>`, `assets-src/` |
| 컨트롤 API 추측 | `docs/api.md` 확인 |
