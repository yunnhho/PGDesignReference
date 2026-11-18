# dialog 배치 고정 규칙 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** dialog의 치수·글꼴·정렬을 라이브러리 헬퍼가 정하고, 어기면 render 점검이 잡게 한다.

**Architecture:** 공통 치수 상수 `ApcLayout` → `ApcForm` partial에 dialog 헬퍼(호출 순서대로 쌓고 `OnLoad`에서 한 번에 배치) → 템플릿 5개 재작성 → `LayoutLint`에 규칙 5개 → SKILL.md.

**Tech Stack:** .NET Framework 4.0, C# 7.3, WinForms. 점검은 `src/APC.UI.Checks`(콘솔, `Check(bool, name)`).

**Spec:** `docs/superpowers/specs/2026-11-18-dialog-layout-rules-design.md`

## Global Constraints
- C# 7.3, NuGet 추가 금지. 완료 = `./build.sh` → `ALL PASSED`.
- 고정값(배율 전 px): 여백 16, 입력 행 사이 12, 묶음 사이 16, 이름표↔입력칸 12, 요소 사이 8, 하단 버튼 110×40·사이 8, 입력칸 높이 `ApcTextBox.BaseHeight`(34).
- 글꼴: 테마 글꼴·크기만. 굵게는 `ApcGroup` 제목·구획 제목(`SectionTitle`)에만.
- 하단 버튼 [취소][확정], 확정이 오른쪽 끝.
- public API 변경 → `src/APC.UI/Properties/AssemblyInfo.cs` 1.7.0.0.

## Review Focus
- 이름표가 아주 긴 행(예: "압입 속도 상한 (mm/s)") → 이름표 열이 넓어져도 모든 입력칸 왼쪽 끝이 같다. (Task 2 테스트)
- 묶음 안 행과 묶음 밖 행이 섞인 dialog → 같은 이름표 열 폭. (Task 2 테스트)
- Fill 내용인데 `SetupDialog` 높이 0 / Fill 두 개 → `InvalidOperationException`(조용히 0 높이 금지). (Task 2 테스트)
- 배율 1.25(고DPI) → 헬퍼 결과가 `ApcTheme.Px` 기준으로 맞고 점검 허용 오차 ±1px. (Task 4: 비교는 모두 `ApcTheme.Px`로, 오차 1)
- 버튼 하나뿐인 보기용 dialog(닫기만) → `SetFooter(null, close)`가 동작하고 점검에 걸리지 않는다. (Task 2·3)

---

### Task 1: `ApcLayout` 상수와 메인 화면 간격 통일

**Files:**
- Create: `src/APC.UI/Theming/ApcLayout.cs`
- Modify: `src/APC.UI/Controls/ApcTemplateView.cs` (Body.Padding 16, `Caption`/`Button` Margin 8, `Columns` 12, `Row` 10→12)
- Modify: `src/APC.UI/APC.UI.csproj` (Compile 항목)
- Test: `src/APC.UI.Checks/DialogLayoutChecks.cs`(새로) + `Program.cs`에 `Run("dialog-layout", RunDialogLayoutChecks)`(lint 앞), csproj Compile 항목

**Interfaces:**
- Produces: `public static class ApcLayout { public const int Pad = 16, RowGap = 12, GroupGap = 16, LabelGap = 12, ItemGap = 8, FooterButtonWidth = 110, FooterButtonHeight = 40; public const int FieldHeight = ApcTextBox.BaseHeight; }` (namespace `APC.UI.Theming`)

- [ ] **Step 1:** 실패 테스트 — `RunDialogLayoutChecks()`에 `Check(ApcLayout.Pad == 16 && ApcLayout.RowGap == 12 && ApcLayout.GroupGap == 16 && ApcLayout.FooterButtonWidth == 110 && ApcLayout.FooterButtonHeight == 40, "layout: fixed values")`. 테마는 `ApcTheme.Load(Path.Combine(Root, @"themes\default"))`.
- [ ] **Step 2:** `./build.sh` → 컴파일 오류(`ApcLayout` 없음) 확인.
- [ ] **Step 3:** `ApcLayout` 작성, `ApcTemplateView`의 숫자를 상수로 교체(`Row` 아래 간격 10→`RowGap`).
- [ ] **Step 4:** `./build.sh` → `ALL PASSED`. 메인 템플릿 lint가 줄 간격 변화로 깨지면(넘침·스크롤바) 해당 템플릿 높이를 고친다.
- [ ] **Step 5:** 커밋 `refactor: 공통 치수 ApcLayout, 메인 화면 줄 간격 12로 통일`

### Task 2: `ApcForm` dialog 헬퍼

**Files:**
- Modify: `src/APC.UI/Controls/ApcForm.cs` (`partial` 추가, `OnLoad`에서 `LayoutDialog()` 호출)
- Create: `src/APC.UI/Controls/ApcForm.Dialog.cs`
- Test: `src/APC.UI.Checks/DialogLayoutChecks.cs`

**Interfaces:**
- Consumes: `ApcLayout` (Task 1)
- Produces (모두 `ApcForm`의 protected, 크기 인자는 배율 전 px):
  - `void SetupDialog(string title, int width, int height = 0)` — TitleText, Resizable=false, MinimizeBox=false, ShowInTaskbar=false, CenterParent, 폭. height 0 = 내용으로 자동.
  - `void AddFieldRow(string caption, Control field, params Control[] trailing)` — trailing 없으면 입력칸이 남은 폭을 채움, 있으면 field·trailing은 자기 Width 유지하고 `ItemGap` 간격으로 왼→오.
  - `ApcGroup AddGroup(string title)` / `void AddFieldRow(ApcGroup group, string caption, Control field, params Control[] trailing)`
  - `void AddContent(Control c)` — 자기 Height 그대로(호출 측이 `ApcTheme.Px`로 지정), 폭은 내용 폭.
  - `void AddContentFill(Control c)` — 남은 높이. `height == 0`이거나 두 번째 호출이면 `InvalidOperationException`.
  - `void SetFooter(ApcButton cancel, ApcButton confirm, params ApcButton[] left)` — cancel null 허용. `CancelButton = cancel ?? confirm`(보기용 닫기도 Esc). AcceptButton은 건드리지 않음.
  - `static Label SectionTitle(string text)` — 굵은 테마 글꼴, 높이 `Px(28)`, BottomLeft 정렬.
  - `static Label FieldCaption(string text)` — 행 안 trailing 이름표: 보통 굵기 테마 글꼴, 폭 = 글자 폭 + `Px(4)`, MiddleLeft.
- 배치 규칙(`LayoutDialog`, OnLoad에서 한 번): 시작 y = `ContentBounds.Top + Px(Pad)`, x = `ContentBounds.Left + Px(Pad)`, 내용 폭 = `ContentBounds.Width - 2*Px(Pad)`. 이름표 열 폭 = 모든 행(묶음 안 포함) 이름표 `TextRenderer.MeasureText` 최댓값 + `Px(4)`, 이름표는 MiddleLeft·보통 굵기·높이 `Px(FieldHeight)`. 블록 사이 간격: 입력 행→입력 행 `RowGap`, `SectionTitle` 다음 `ItemGap`, 그 밖 `GroupGap`. 묶음 높이 = 묶음 Padding 세로 합 + 행 높이 합 + 행 사이. 하단 버튼 줄은 마지막 블록 아래 `GroupGap`, 버튼 `FooterButtonWidth×FooterButtonHeight`, 확정 오른쪽 끝·취소는 그 왼쪽 `ItemGap`, left 버튼은 x부터 오른쪽으로. height 0이면 `ClientSize.Height`를 하단 버튼 아래 `Pad` + `Padding.Bottom`까지로 맞춘다. 도킹 쓰지 않고 SetBounds로 둔다(Add 순서 함정 없음).

- [ ] **Step 1:** 실패 테스트 — Checks 안에 `ApcForm` 상속 시험 dialog 2개를 둔다: (a) `SetupDialog("t", 460)` + 행 2개("모델명", "압입 속도 상한 (mm/s)") + `AddGroup("묶음")` 안 행 1개 + `SetFooter(취소, 저장)`; (b) `SetupDialog("t", 600, 400)` + `AddContent(grid h=Px(100))` + `AddContentFill(chart)` + `SetFooter(null, 닫기)`. 화면 밖에 `Show()` + `DoEvents()` 후:
  - `"dialog: field left edges equal"` — 세 입력칸의 화면 좌표 Left 같음(묶음 안 포함, `PointToScreen`).
  - `"dialog: row gap is RowGap"` — 행1.Top - 행0.Bottom == Px(12).
  - `"dialog: footer order and size"` — 취소.Right + Px(8) == 저장.Left, 저장.Right == ContentBounds.Right - Px(16), 두 버튼 크기 Px(110)×Px(40), `CancelButton == 취소`.
  - `"dialog: auto height"` — 저장.Bottom == ContentBounds.Bottom - Px(16), 저장.Top - 마지막 블록 Bottom == Px(16).
  - `"dialog: fill content"` — (b) chart.Bottom + Px(16) == 닫기.Top, `CancelButton == 닫기`.
  - `"dialog: fill without height throws"` / `"dialog: second fill throws"` — `InvalidOperationException`.
- [ ] **Step 2:** `./build.sh` → 컴파일 오류 확인.
- [ ] **Step 3:** `ApcForm.Dialog.cs` 구현(위 배치 규칙).
- [ ] **Step 4:** `./build.sh` → `ALL PASSED`.
- [ ] **Step 5:** 커밋 `feat: ApcForm dialog 헬퍼(SetupDialog·AddFieldRow·AddGroup·AddContent·SetFooter)`

### Task 3: dialog 템플릿 5개를 헬퍼로 재작성

**Files:**
- Modify: `src/APC.UI.Demo/Templates/{RecipeDialog,SettingsDialog,AdminPasswordDialog,LotInputDialog,EquipmentDetailDialog}.cs`

**Interfaces:**
- Consumes: Task 2 헬퍼 전부.

정보 배치·동작(검증·Confirm·결과 속성·Enter/Esc)은 유지하고, 숫자·글꼴·직접 만든 Panel/TableLayoutPanel/버튼 줄을 헬퍼로 바꾼다:
- `RecipeDialog`: `SetupDialog("레시피 편집", 460)`, `AddFieldRow` 6개, `SetFooter(취소, 저장)`. `AddRow`·`_y`·`ClientSize` 삭제.
- `SettingsDialog`: `SetupDialog("시스템 설정", 680)`, 블록마다 `AddGroup` + `AddFieldRow(group, "포트", 콤보, FieldCaption("신호"), 콤보, ..., 연결 시도)`. trailing 이름표는 `FieldCaption`. `SetFooter(취소, 저장, 비밀번호 변경)`.
- `AdminPasswordDialog`: `SetupDialog(title, 400)`, `AddFieldRow("비밀번호", _password)`, `SetFooter(취소, 확인)`, `AcceptButton = ok` 유지.
- `LotInputDialog`: 설비 줄은 `AddFieldRow("설비", 값 Label)`(보통 굵기, `FontSize*1.3f` 삭제), 표 `AddContent`(높이 = 머리줄 + 행 수), `SetFooter(취소, 확인)`.
- `EquipmentDetailDialog`: `SetupDialog(name + " 상세", 760, 560)`, `AddContent(head)`, `AddContent(grid)`, `AddContent(SectionTitle("최근 1시간 온도 추이 …"))`, `AddContentFill(chart)`, `SetFooter(null, 닫기)`.

- [ ] **Step 1:** 다섯 파일 재작성.
- [ ] **Step 2:** `./build.sh` → `ALL PASSED`(기존 lint: 템플릿 5개 점검 0건 포함).
- [ ] **Step 3:** 다섯 dialog를 render해서 PNG를 눈으로 확인(이름표 열 정렬, 버튼 크기, 빈 공간):
  `T=src/APC.UI.Tools/bin/Debug/APC.UI.Tools.exe; for d in RecipeDialog SettingsDialog AdminPasswordDialog LotInputDialog EquipmentDetailDialog; do $T render "$(cygpath -w src/APC.UI.Demo/bin/Debug/APC.UI.Demo.exe)" $d "$(cygpath -w themes/default)" "$(cygpath -w out/$d.png)"; done` → 각각 `점검 0건`.
- [ ] **Step 4:** 커밋 `refactor: dialog 템플릿을 ApcForm 헬퍼로 — 치수·글꼴 통일`

### Task 4: render 점검 규칙 5개

**Files:**
- Modify: `src/APC.UI.Tools/LayoutLint.cs` (APC.UI 사본이 둘일 수 있어 `ApcLayout`을 참조하지 않고 같은 값을 private const로 둔다 — 기존 방식과 동일, 주석에 `ApcLayout`과 같게 유지하라고 적는다)
- Test: `src/APC.UI.Checks/LintChecks.cs` (`CheckLintCatches`에 추가)

**Interfaces:**
- Consumes: `ApcTheme.Current.FontFamily/FontSize`, `ApcTheme.Px`.
- 규칙(메시지 키워드가 테스트 기준):
  1. 모든 화면: `Label`과 Apc 컨트롤의 `Font.Name`/`Font.Size`가 테마와 다름 → `"테마와 다른 글꼴"`.
  2. 모든 화면: 같은 줄 오른쪽에 입력 컨트롤(`FieldTypes`에서 `ApcButton` 제외)이 있는 굵은 Label → `"이름표가 굵게"`.
  3. 같은 부모 안 "이름표 바로 오른쪽 첫 입력칸"들의 Left가 다름(오차 1) → `"입력칸 왼쪽 끝 어긋남"`. dialog(아래 정의)에서는 부모가 달라도 화면 좌표로 비교.
  4. 같은 부모 안 그 행들(3개 이상)의 세로 간격이 서로 다름 → `"행 간격 다름"`.
  5. dialog(루트가 `ApcForm`이고 `Resizable` 속성(리플렉션)이 false)에서만, 가장 아래 버튼 줄: 버튼 크기 ≠ `Px(110)×Px(40)` → `"하단 버튼 크기"`; 버튼 줄 위 빈 공간 ≠ `Px(16)` 또는 아래 여백 ≠ `Px(16)` → `"하단 버튼 위 빈 공간"`/`"하단 여백"`.

- [ ] **Step 1:** 실패 테스트(기존 `Lint(Action<Panel>)` + 새 `LintDialog(Action<ApcForm>)`: Resizable=false ApcForm을 화면 밖 Show):
  - `"lint: off-theme font"` — `new Label { Font = new Font("Arial", 14f) }` → `테마와 다른 글꼴`.
  - `"lint: bold field caption"` — 굵은 Label(0,0,100,34) + ApcTextBox(110,0) → `이름표가 굵게`.
  - `"lint: misaligned field left edges"` — 행 두 개, 입력칸 Left 120/140 → `왼쪽 끝 어긋남`.
  - `"lint: uneven row gaps"` — 행 세 개, 간격 12/20 → `행 간격 다름`.
  - `"lint: footer button size"` — dialog 하단 [취소 100×40][저장 120×40] → `하단 버튼 크기`.
  - `"lint: empty space above footer"` — 마지막 행과 버튼 줄 사이 80px → `빈 공간`.
  - `"lint: helper dialog is clean"` — Task 2의 시험 dialog (a)를 lint → 0건.
- [ ] **Step 2:** `./build.sh` → 새 항목 FAIL 확인.
- [ ] **Step 3:** `LayoutLint`에 규칙 구현(`Walk`/`CheckSiblings` 안, 3·5는 `Inspect`에서 루트 기준 한 번).
- [ ] **Step 4:** `./build.sh` → `ALL PASSED`(템플릿 13개 점검 0건 포함. 메인 템플릿이 규칙 1·2에 걸리면 템플릿을 고친다).
- [ ] **Step 5:** 커밋 `feat: render 점검 — 글꼴·이름표 열·행 간격·하단 버튼`

### Task 5: SKILL.md·버전·재설치

**Files:**
- Modify: `skills/apc-field-ui/SKILL.md` (4b, 4-1, 7-3 점검 목록), `src/APC.UI/Properties/AssemblyInfo.cs`(1.7.0.0), `docs/README.md`(헬퍼 언급이 있으면)

- [ ] **Step 1:** SKILL.md 4b를 바꾼다: "dialog는 `ApcForm` 상속 + 헬퍼로만 만든다(`SetupDialog`→`AddFieldRow`/`AddGroup`/`AddContent`/`AddContentFill`→`SetFooter`, 위에서 아래 순서). `SetBounds`·`ClientSize`·버튼 줄 Panel을 직접 만들지 않는다." 헬퍼 서명 한 줄씩, 템플릿 표 유지. 기존 "`Columns()`·`Row()`·`S()`는 dialog에서 못 쓴다 → `AddRow`처럼 직접" 문장 삭제.
- [ ] **Step 2:** 4-1 표에 행 추가: 글꼴(테마 글꼴·크기만, 굵게는 묶음·구획 제목만, `Font` 직접 생성 금지), 단위(입력은 이름표 괄호, 표시는 `Unit`·열 제목), 간격(직접 숫자 대신 헬퍼·`ApcLayout`). 7-3 점검 목록에 새 5개 추가.
- [ ] **Step 3:** AssemblyInfo 1.7.0.0, `./build.sh` → `ALL PASSED`, `bash tools/install-skill.sh`.
- [ ] **Step 4:** 커밋 `docs: 스킬에 dialog 헬퍼·글꼴·단위 규칙 반영 (APC.UI 1.7.0.0)`
