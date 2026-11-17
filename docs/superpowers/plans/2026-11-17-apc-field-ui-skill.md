# APC 현장 UI 스킬 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 새 .NET Framework 4.x WinForms 프로젝트에서 에이전트가 APC.UI 기준대로 "메인 화면 + dialog" 껍데기를 만들도록, 저장소 도구(뼈대·API 요약·렌더·csproj 연결)와 개인 스킬 `apc-field-ui`를 만든다.

**Architecture:** 반복 작업은 저장소 쪽 결정적 도구가 맡는다. 새 .NET 4.0 콘솔 `APC.UI.Tools`(명령 `api` / `render` / `attach`)와 `tools/new-project.sh` + `tools/project-template/`가 그 도구다. 스킬은 짧은 절차서이고, API는 빌드 때마다 자동 생성되는 `docs/api.md`만 가리킨다. `build.sh`가 뼈대 생성 → 빌드 → 렌더까지 점검해서, 도구가 깨지면 `ALL PASSED`가 나오지 않는다.

**Tech Stack:** C# 7.3, .NET Framework 4.0, WinForms, 구식 csproj, VS2019 MSBuild, Git Bash(sh), Node + `@resvg/resvg-js`(기존).

**Spec:** `docs/superpowers/specs/2026-11-17-apc-field-ui-skill-design.md`

## Global Constraints

- 저장소 코드: `<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>`, `<LangVersion>7.3</LangVersion>`, 구식 csproj, NuGet 추가 금지.
- 완료 기준: `./build.sh` → 마지막 줄 `ALL PASSED (n)`. 실패 상태로 끝내지 않는다.
- 점검은 기존 assert 러너(`src/APC.UI.Checks`, `Check(bool, name)`)에 추가한다. 테스트 프레임워크는 쓰지 않는다.
- `Resources/`, `references/`는 읽기 전용이다. `themes/*/assets/*.png`는 `render-assets.mjs`로만 다시 만든다. 상태색은 색상을 바꾸지 않는다.
- 생성되는 새 프로젝트는 .NET Framework 4.x WinForms만 대상으로 한다. 그 외 형식이면 도구가 0이 아닌 종료 코드와 이유를 낸다.
- 새 프로젝트 코드는 그 프로젝트의 `TargetFrameworkVersion`/`LangVersion`을 따른다. 템플릿 기본값은 `v4.0` / `7.3`이다.
- 화면은 코드로만 배치한다(디자이너 지원 없음). 구성은 메인 화면 하나 + dialog다.
- MSBuild 경로: `/c/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe`. 스크립트는 `tools/env.sh`의 `$MSBUILD`를 쓴다.
- 브랜치는 `feat/apc-field-ui-skill`이다. 커밋 메시지는 `<type>: <설명>` 형식으로 쓴다.

## Review Focus

1. **잘못된 이름 입력**: 프로젝트명이 C# 식별자가 아니거나(`1st-app`, 공백), 고객사명에 공백·한글이 들어간 경우 → `new-project.sh`가 프로젝트명은 거부하고, 고객사 폴더는 따옴표 경로로 정상 처리한다. → Task 6 점검.
2. **대상 폴더에 이미 csproj가 있는데 새로 만들기 모드로 실행** → 덮어쓰지 않고 `--into-existing`을 안내하며 중단한다. → Task 6 점검.
3. **`render` 형식명 오타 / 매개변수 있는 생성자** → 이름이 비슷한 후보 형식을 나열하는 오류, 종료 코드 2. → Task 3 점검.
4. **`render`에 틀린 테마 경로** → `ThemeException` 메시지(파일·항목 포함)를 그대로 출력하고 종료 코드 2. 빈 PNG를 만들지 않는다. → Task 3 점검.
5. **기존 프로젝트가 다른 경로(HintPath)로 이미 APC.UI를 참조** → `attach`가 참조를 중복 추가하지 않고, 기존 `HintPath`를 `lib\APC.UI.dll`로 바꾼다. → Task 4 점검.

---

### Task 1: `ApcTemplateView`를 dll로 이동 + 버전 규칙

이름은 `TemplateView`에서 `ApcTemplateView`로 바꾼다. 기존 public 컨트롤이 모두 `Apc` 접두사를 쓰고, 고객 프로젝트에서 이름이 겹치지 않게 하기 위해서다(작성자 판단).

**Files:**
- Create: `src/APC.UI/Controls/ApcTemplateView.cs`
- Delete: `src/APC.UI.Demo/Templates/TemplateView.cs`
- Modify: `src/APC.UI.Demo/Templates/*.cs` (7종의 `: TemplateView` → `: ApcTemplateView`, `using APC.UI.Controls;`)
- Modify: `src/APC.UI/Properties/AssemblyInfo.cs` (`AssemblyVersion`/`AssemblyFileVersion` → `1.1.0.0`)
- Modify: `CLAUDE.md` (Git 절 아래 한 줄: "public API가 바뀌면 `src/APC.UI/Properties/AssemblyInfo.cs`의 버전을 올린다.")
- Test: `src/APC.UI.Checks/FormChecks.cs`

**Interfaces:**
- Produces: `namespace APC.UI.Controls { public class ApcTemplateView : UserControl }`
  - `protected ApcTemplateView(string title)` (기존 생성자 본문 그대로)
  - `protected readonly ApcHeaderBar Header; protected readonly ApcAlarmBar Alarm; protected readonly Panel Body;`
  - `protected static int S(int px)`, `protected static Label Caption(string text)`, `protected static ApcButton Button(string text, ApcButtonVariant variant, int width = 110)`, `protected static FlowLayoutPanel Row(params Control[] controls)`

- [ ] **Step 1: 실패하는 점검 작성** — `FormChecks.cs`에 `private sealed class ProbeView : ApcTemplateView { public ProbeView() : base("probe") {} public Control H { get { return Header; } } public Control A { get { return Alarm; } } public Control B { get { return Body; } } }`를 추가한다. 그리고 다음을 점검한다.
  ```csharp
  using (var v = new ProbeView())
  {
      Check(v.H.Dock == DockStyle.Top && v.A.Dock == DockStyle.Bottom && v.B.Dock == DockStyle.Fill, "template view: header top, alarm bottom, body fill");
      Check(((ApcHeaderBar)v.H).Title == "probe", "template view: title goes to header");
  }
  Check(typeof(ApcTemplateView).IsPublic, "template view is public in APC.UI");
  ```
- [ ] **Step 2: 실패 확인** — `./build.sh` → `ApcTemplateView` 미정의로 BUILD FAILED.
- [ ] **Step 3: 이동** — 파일을 옮기고 `internal` → `public`, 이름과 생성자를 바꾼다. 데모 7종의 상속을 바꾸고, 버전을 `1.1.0.0`으로 올리고, `CLAUDE.md`에 한 줄 추가한다.
- [ ] **Step 4: 통과 확인** — `./build.sh` → `ALL PASSED`. `src/APC.UI.Demo/bin/Debug/APC.UI.Demo.exe`를 실행해 7개 메뉴 화면이 이동 전과 같은지 눈으로 확인한다.
- [ ] **Step 5: 커밋** — `git commit -m "refactor: TemplateView를 APC.UI의 public ApcTemplateView로 이동"`

### Task 2: `APC.UI.Tools` + `api` 명령 + `docs/api.md`

**Files:**
- Create: `src/APC.UI.Tools/APC.UI.Tools.csproj` (Exe, v4.0, 구식, `Compile Include="**\*.cs"`, 참조: System, System.Drawing, System.Windows.Forms, System.Xml, System.Xml.Linq. **APC.UI 프로젝트 참조 없음**: 대상 어셈블리를 런타임에 읽는다)
- Create: `src/APC.UI.Tools/Program.cs` (명령 분기. 사용법이 틀리면 사용법을 출력하고 종료 코드 1)
- Create: `src/APC.UI.Tools/ApiWriter.cs`
- Modify: `APC.UI.sln` (프로젝트 추가)
- Modify: `src/APC.UI/APC.UI.csproj` (두 구성에 `<DocumentationFile>bin\$(Configuration)\APC.UI.xml</DocumentationFile>`, `<NoWarn>1591</NoWarn>`)
- Modify: `src/APC.UI.Checks/APC.UI.Checks.csproj` (`APC.UI.Tools` 프로젝트 참조), `src/APC.UI.Checks/Program.cs` (`Run("tools", RunToolChecks);`)
- Create: `src/APC.UI.Checks/ToolChecks.cs`
- Modify: `build.sh` (자가점검 전에 `./src/APC.UI.Tools/bin/Debug/APC.UI.Tools.exe api src/APC.UI/bin/Debug/APC.UI.dll docs/api.md`, 실패하면 종료)
- Create: `docs/api.md` (생성물, 커밋 대상)

**Interfaces:**
- Produces: `public static class ApiWriter { public static string Write(string dllPath); }`. Markdown을 반환한다.
  - 첫 줄은 `# APC.UI API (v<AssemblyVersion>)`이다.
  - public 형식마다 `## <Name>`, 이어서 `<summary>`를 붙인다. 요약은 옆의 `<dll 이름>.xml`이 있으면 거기서 읽는다.
  - 선언된 public/protected 멤버(생성자·속성·메서드·이벤트)를 한 줄씩 적는다. 시그니처는 C# 형태로 쓰고 요약이 있으면 덧붙인다.
  - 형식과 멤버는 이름순으로 정렬한다. 같은 입력이면 같은 출력이 나와야 한다.
- Produces: 명령 `APC.UI.Tools.exe api <dll> <out.md>`. 종료 코드는 0(성공) 또는 2(파일 없음이나 로드 실패, 메시지 출력)다.
- 어셈블리는 `Assembly.LoadFrom`으로 읽는다. 의존 어셈블리는 `AppDomain.CurrentDomain.AssemblyResolve`로 대상 dll 폴더에서 찾는다(Task 3도 같은 처리를 쓴다 → `AssemblyLoader.Load(string path)`로 `Program.cs` 옆에 둔다).

- [ ] **Step 1: 실패하는 점검 작성** — `ToolChecks.cs`:
  ```csharp
  var md = ApiWriter.Write(Path.Combine(Root, @"src\APC.UI\bin\Debug\APC.UI.dll"));
  Check(md.StartsWith("# APC.UI API (v1.1.0.0)"), "api: header carries version");
  Check(md.Contains("## ApcTemplateView") && md.Contains("## ApcButton"), "api: lists public controls");
  Check(md.Contains("Load(string themeDir)"), "api: lists ApcTheme.Load signature");
  Check(md == ApiWriter.Write(Path.Combine(Root, @"src\APC.UI\bin\Debug\APC.UI.dll")), "api: deterministic");
  ```
- [ ] **Step 2: 실패 확인** — `./build.sh` → `ApiWriter` 미정의로 BUILD FAILED.
- [ ] **Step 3: 구현** — 위 Interfaces대로 구현하고, sln과 csproj를 수정하고, `build.sh`에 api 생성 단계를 넣는다.
- [ ] **Step 4: 통과 확인** — `./build.sh` → `ALL PASSED`. `docs/api.md`가 생성되고 `## ApcGrid` 등 9개 컨트롤과 `ApcTemplateView`, `ApcTheme`이 들어 있는지 확인한다.
- [ ] **Step 5: 커밋** — `git commit -m "feat: APC.UI.Tools api 명령과 docs/api.md 자동 생성"`

### Task 3: `render` 명령

**Files:**
- Create: `src/APC.UI.Tools/Renderer.cs`
- Modify: `src/APC.UI.Tools/Program.cs`
- Test: `src/APC.UI.Checks/ToolChecks.cs`

**Interfaces:**
- Consumes: `AssemblyLoader.Load(string path)` (Task 2)
- Produces: `public static class Renderer`
  - `public static Bitmap Render(string assemblyPath, string typeName, string themeDir, Size size)`
    - 어셈블리를 로드한다.
    - 대상이 참조하는 `APC.UI` 어셈블리에서 `APC.UI.Theming.ApcTheme.Load(themeDir)`를 **리플렉션으로** 호출한다. 그래야 대상과 같은 ApcTheme 정적 상태를 쓴다.
    - 형식을 찾는다(전체 이름 일치, 없으면 단순 이름 일치). 매개변수 없는 생성자(public이나 non-public)로 만든다.
    - `Form`이면 `Size`를 정하고 `CreateControl()` 후 `DrawToBitmap`한다. `UserControl`이면 `Dock=Fill`로 같은 크기의 숨은 `Form`에 올린 뒤 그린다.
  - `public static Bitmap SideBySide(Image left, Image right)`: 높이를 큰 쪽에 맞추고 비율대로 늘려 가로로 붙인다. 너비는 두 너비의 합이다.
  - `public sealed class RenderException : Exception`: 형식이 없을 때(같은 단어를 포함하는 후보를 최대 5개 나열), 매개변수 없는 생성자가 없을 때, 테마 로드 실패(내부 `ThemeException` 메시지를 그대로 담음) 때 던진다.
- Produces: 명령 `APC.UI.Tools.exe render <assembly> <type> <themeDir> <out.png> [--size 1920x1080] [--compare <원본.png>]`
  - `--compare`를 주면 `<out>.compare.png`도 쓴다.
  - 종료 코드: 0 / 1(사용법) / 2(`RenderException`, 메시지 출력, PNG 쓰지 않음).
- 도구 exe에는 dpiAware 매니페스트를 두지 않는다. 그래서 렌더는 96dpi 기준으로 결정적이다.

- [ ] **Step 1: 실패하는 점검 작성** — `demo`는 `Path.Combine(Root, @"src\APC.UI.Demo\bin\Debug\APC.UI.Demo.exe")`, `theme`은 `Path.Combine(Root, @"themes\default")`로 둔다.
  ```csharp
  using (var bmp = Renderer.Render(demo, "ProductionView", theme, new Size(1920, 1080)))
  {
      Check(bmp.Width == 1920 && bmp.Height == 1080, "render: requested size");
      Check(bmp.GetPixel(10, 10) != bmp.GetPixel(960, 1070), "render: not a blank image");
  }
  Check(Throws<RenderException>(() => Renderer.Render(demo, "ProductonView", theme, new Size(800, 600)), "ProductionView"), "render: typo lists candidates");
  Check(Throws<RenderException>(() => Renderer.Render(demo, "ProductionView", Path.Combine(Root, "nope"), new Size(800, 600)), "nope"), "render: bad theme path reported");
  using (var a = new Bitmap(100, 50)) using (var b = new Bitmap(200, 100)) using (var s = Renderer.SideBySide(a, b))
      Check(s.Width == 400 && s.Height == 100, "render: side-by-side scales to taller height");
  ```
  (Checks에 Demo 프로젝트 참조가 없으면 빌드 순서가 보장되지 않는다. `APC.UI.Checks.csproj`에 `APC.UI.Demo` 프로젝트 참조를 `<ReferenceOutputAssembly>false</ReferenceOutputAssembly>`로 추가해 빌드 순서만 잡는다.)
  (`Renderer`가 `ApcTheme.Load`로 전역 테마를 바꾸므로, `Run("tools", ...)`는 `Main`의 맨 마지막 점검으로 둔다.)
- [ ] **Step 2: 실패 확인** — `./build.sh` → `Renderer` 미정의로 BUILD FAILED.
- [ ] **Step 3: 구현** — Interfaces대로 구현한다. `ProductionView`처럼 데모 템플릿이 `internal`이면 `BindingFlags.NonPublic`으로 형식과 생성자를 찾는다.
- [ ] **Step 4: 통과 확인** — `./build.sh` → `ALL PASSED`. 명령으로도 한 번 실행한다: `APC.UI.Tools.exe render src/APC.UI.Demo/bin/Debug/APC.UI.Demo.exe ProductionView themes/default <scratch>/p.png --compare references/<아무 원본>.png` → PNG 2개가 생기는지 눈으로 확인한다.
- [ ] **Step 5: 커밋** — `git commit -m "feat: APC.UI.Tools render 명령(화면 PNG와 원본 비교본)"`

### Task 4: `attach` 명령 (기존 csproj에 연결)

**Files:**
- Create: `src/APC.UI.Tools/CsprojPatcher.cs`
- Modify: `src/APC.UI.Tools/Program.cs`
- Create: `src/APC.UI.Checks/fixtures/csproj/old-winforms.csproj`, `sdk.csproj`, `net48-console.csproj`, `old-with-hintpath.csproj`
- Test: `src/APC.UI.Checks/ToolChecks.cs`

**Interfaces:**
- Produces: `public static class CsprojPatcher`
  - `public static string Patch(string csprojXml, string customer)`: 수정된 XML을 반환한다. 입력은 바꾸지 않는다.
    - 루트에 `Sdk` 속성이 있으면 → `InvalidOperationException("SDK 형식 csproj는 지원하지 않습니다 ...")`
    - `TargetFrameworkVersion`이 `v4.`로 시작하지 않으면 → 예외(메시지에 실제 값 포함)
    - `OutputType`이 `WinExe`가 아니고 `System.Windows.Forms` 참조도 없으면 → 예외("WinForms 프로젝트가 아닙니다")
    - `Reference Include`가 `APC.UI`로 시작하는 항목이 있으면 그 `HintPath`를 `lib\APC.UI.dll`로 바꾼다. 없으면 `<Reference Include="APC.UI"><HintPath>lib\APC.UI.dll</HintPath></Reference>`를 추가한다.
    - `<None Include="themes\**\*"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>`가 없으면 추가한다.
    - 두 번 적용해도 결과가 같다.
- Produces: 명령 `APC.UI.Tools.exe attach <csproj> <customer>`. 파일을 제자리에서 고친다. 종료 코드: 0 / 2(예외 메시지 출력, 파일 변경 없음).

- [ ] **Step 1: 실패하는 점검 작성**
  ```csharp
  string old = File.ReadAllText(Fx(@"csproj\old-winforms.csproj"));
  string once = CsprojPatcher.Patch(old, "acme");
  Check(once.Contains(@"<HintPath>lib\APC.UI.dll</HintPath>") && once.Contains(@"themes\**\*"), "attach: adds reference and theme copy");
  Check(CsprojPatcher.Patch(once, "acme") == once, "attach: idempotent");
  Check(Throws<InvalidOperationException>(() => CsprojPatcher.Patch(File.ReadAllText(Fx(@"csproj\sdk.csproj")), "acme"), "SDK"), "attach: rejects SDK-style");
  Check(Throws<InvalidOperationException>(() => CsprojPatcher.Patch(File.ReadAllText(Fx(@"csproj\net48-console.csproj")), "acme"), "WinForms"), "attach: rejects non-WinForms");
  string hinted = CsprojPatcher.Patch(File.ReadAllText(Fx(@"csproj\old-with-hintpath.csproj")), "acme");
  Check(hinted.Split(new[] { "Include=\"APC.UI" }, StringSplitOptions.None).Length == 2 && hinted.Contains(@"lib\APC.UI.dll"), "attach: rewrites existing APC.UI HintPath without duplicating");
  ```
- [ ] **Step 2: 실패 확인** — `./build.sh` → BUILD FAILED.
- [ ] **Step 3: 구현** — `System.Xml.Linq.XDocument`로 구현한다. msbuild 네임스페이스(`http://schemas.microsoft.com/developer/msbuild/2003`)를 유지한다.
- [ ] **Step 4: 통과 확인** — `./build.sh` → `ALL PASSED`
- [ ] **Step 5: 커밋** — `git commit -m "feat: APC.UI.Tools attach 명령(기존 csproj에 APC.UI 연결)"`

### Task 5: `render-assets.mjs`가 override 폴더의 새 이름도 렌더

**Files:**
- Modify: `tools/render-assets.mjs`
- Modify: `build.sh` (Task 6의 연기 점검에 포함. 이 Task에서는 수동 확인만 한다)

**Interfaces:**
- Produces: 렌더 대상은 `src`의 SVG 이름과 `override`의 SVG 이름의 합집합이다. 같은 이름이면 override가 우선한다(기존 동작). override에만 있는 이름도 렌더한다.

- [ ] **Step 1: 실패 확인** — `<scratch>/ov/icon_smoke_normal.svg`(내용은 `assets-src/lamp_run.svg` 복사본)를 만들고 `node tools/render-assets.mjs --out <scratch>/out --override <scratch>/ov`를 실행한다 → `icon_smoke_normal@1x.png`가 없음을 확인한다.
- [ ] **Step 2: 구현** — 파일 목록을 `new Set([...src 목록, ...override 목록])`으로 만든다. 경로는 override에 있으면 override, 없으면 src를 쓴다.
- [ ] **Step 3: 통과 확인** — 같은 명령 → `icon_smoke_normal@{1x,1.5x,2x}.png`가 생긴다. `node tools/render-assets.mjs`(인자 없음)로 기본 테마를 다시 렌더해도 `git status themes/`에 변경이 없어야 한다.
- [ ] **Step 4: 커밋** — `git commit -m "fix: render-assets가 override 폴더에만 있는 SVG도 렌더"`

### Task 6: 새 프로젝트 뼈대 `new-project.sh` + `build.sh` 연기 점검

**Files:**
- Create: `tools/env.sh` (`MSBUILD=...` 한 줄. `build.sh`도 이 파일을 source하도록 바꾼다)
- Create: `tools/project-template/` — 자리표시자는 `__NAME__`, `__CUSTOMER__`, `__FRAMEWORK__`이다.
  - `__NAME__.sln`, `__NAME__.csproj` (WinExe, `TargetFrameworkVersion=__FRAMEWORK__`, `LangVersion=7.3`, `Compile Include="**\*.cs" Exclude="bin\**;obj\**"`, `ApplicationManifest=app.manifest`, `lib\APC.UI.dll` 참조, `themes\**\*` 출력 복사)
  - `Program.cs`: `ApcTheme.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"themes\__CUSTOMER__"))`, `ThemeException`이면 `MessageBox` 후 종료 코드 1, `Application.Run(new MainForm())`
  - `MainForm.cs`: `ApcForm` 상속, `Text="__NAME__"`, `MainView`를 Fill로 올린다.
  - `Views/MainView.cs`: `ApcTemplateView` 상속, 생성자 `public MainView() : this(new SampleMainViewSource())`와 `public MainView(IMainViewSource source) : base("__NAME__")`, 본문에 `ApcGrid` 하나(소스의 행을 채움)
  - `Data/IMainViewSource.cs`: `public interface IMainViewSource { IList<string[]> GetRows(); }`
  - `Data/SampleMainViewSource.cs`: 샘플 행 3개
  - `app.manifest`: `src/APC.UI.Demo/app.manifest` 내용
- Create: `tools/new-project.sh`
- Modify: `build.sh`
- Modify: `docs/README.md` ("새 고객사 프로젝트에서 쓰는 법" 맨 앞에 `tools/new-project.sh` 한 줄과 사용 예. 도구 세 명령 한 줄씩)

**Interfaces:**
- Consumes: `APC.UI.Tools.exe attach` (Task 4), `render` (Task 3)
- Produces: 사용법 `tools/new-project.sh <대상폴더> <프로젝트명> <고객사> [--framework v4.0] [--into-existing]`
  - 프로젝트명이 `^[A-Za-z_][A-Za-z0-9_]*$`가 아니면 → 오류, 종료 코드 1
  - 새로 만들기 모드에서 대상 폴더에 `*.csproj`가 있으면 → "`--into-existing`을 쓰세요" 안내, 종료 코드 1, 아무 파일도 쓰지 않는다.
  - 공통: `$MSBUILD src/APC.UI/APC.UI.csproj -p:Configuration=Release`를 실행하고 `lib/APC.UI.dll`(+`.xml`)로 복사한다.
  - 테마: `themes/<고객사>`가 없을 때만, 저장소의 `themes/<고객사>`가 있으면 그것을, 없으면 `themes/default`를 복사한다. 이미 있으면 그대로 둔다(재실행할 때 dll만 교체되는 경로).
  - 새로 만들기: 템플릿을 복사하고 자리표시자를 치환한다. 파일 이름의 `__NAME__`도 바꾼다.
  - `--into-existing`: 템플릿은 쓰지 않는다. 대상 폴더의 csproj(정확히 1개여야 함)에 `attach`를 실행한다.
  - 모든 경로는 따옴표로 감싼다(고객사명의 공백·한글 대응).
- Produces: `build.sh`에 자가점검 앞에 연기 점검 단계를 둔다. 실패하면 `SMOKE FAILED: <단계>`를 출력하고 종료한다.
  1. `T=$(mktemp -d)` → `tools/new-project.sh "$T/app" SmokeApp "샘플 고객"` → `$MSBUILD "$T/app/SmokeApp.sln"` 성공
  2. `APC.UI.Tools.exe render "$T/app/bin/Debug/SmokeApp.exe" MainView "$T/app/bin/Debug/themes/샘플 고객" "$T/main.png"` → 파일 존재
  3. `tools/new-project.sh "$T/app" 1bad x` → 종료 코드 ≠ 0, `tools/new-project.sh "$T/app" SmokeApp x` (csproj 존재) → 종료 코드 ≠ 0
  4. Task 5 확인: override 폴더에 `icon_smoke_normal.svg`를 두고 `render-assets.mjs --out "$T/ov-out"` → `icon_smoke_normal@1x.png` 존재
  5. `rm -rf "$T"`
  - Git Bash 경로(`/tmp/...`)를 MSBuild나 .NET exe에 넘길 때는 `cygpath -w`로 바꾼다. `new-project.sh` 안에서도 같다.

- [ ] **Step 1: 실패하는 점검 작성** — `build.sh`에 위 연기 점검을 먼저 넣는다.
- [ ] **Step 2: 실패 확인** — `./build.sh` → `SMOKE FAILED: new-project` (스크립트 없음)
- [ ] **Step 3: 구현** — `env.sh`, 템플릿, `new-project.sh`를 만들고 README를 고친다.
- [ ] **Step 4: 통과 확인** — `./build.sh` → `ALL PASSED`. 생성된 `$T/app`을 지우기 전에 한 번 `SmokeApp.exe`를 실행해 창이 뜨는지 눈으로 확인한다(수동, 1회).
- [ ] **Step 5: 커밋** — `git commit -m "feat: new-project.sh 뼈대 생성과 build.sh 연기 점검"`

### Task 7: 개인 스킬 `apc-field-ui`

`superpowers:writing-skills` 절차를 따른다. 스킬 파일은 저장소 밖(`~/.claude/skills/`)에 있으므로 커밋하지 않는다. 대신 시험 결과를 저장소 `docs/superpowers/plans/`의 이 계획 끝에 기록한다.

**Files:**
- Create: `C:\Users\rladb\.claude\skills\apc-field-ui\SKILL.md`

**Interfaces:**
- Consumes: `tools/new-project.sh`, `APC.UI.Tools.exe render`, `docs/api.md`, `tools/render-assets.mjs`, 데모 템플릿 소스
- Produces: frontmatter `name: apc-field-ui`. `description`은 "APCSOFT 현장 PC WinForms 화면(모니터링, 공정 설비 프로그램)을 새 프로젝트에서 만들 때"를 트리거로 한다. 본문은 spec §5.2의 7단계를 그대로 따르고 다음 항목을 담는다.
  - 저장소 경로 `C:\00P_PGDesignReference`
  - 필수 입력 3개와 선택 입력 3개
  - 프로젝트 판별 규칙(4.x WinForms가 아니면 중단, 프로젝트의 `LangVersion`을 따름)
  - 목적별 출발 템플릿 표(spec §5.2-4)
  - 새 그림은 프로젝트의 `assets-src/`에 두고 `render-assets.mjs`에 `--override`로 넘김
  - 데이터 자리(`IMainViewSource` 확장, dialog는 값 전달)
  - 완료 확인(빌드 → `render`로 메인과 각 dialog PNG → 캡처가 있으면 `--compare`)
  - 반목표 3개(표준 컨트롤로 그리기, 상태색 변경, `themes/default`·저장소 SVG 수정)
  - API는 직접 적지 않고 `docs/api.md`를 읽으라고만 쓴다. 본문은 150줄 이하로 한다.

- [ ] **Step 1: 기준선(스킬 없음)** — 빈 scratch 폴더에서 새 에이전트에게 "설비 모니터링 화면, 메인 기능: 라인별 가동 상태, dialog 없음"을 요청한다. 결과를 기록한다: 표준 컨트롤 사용 여부, 상태색 규칙 준수, 빌드 여부, 사용 토큰.
- [ ] **Step 2: SKILL.md 작성** — 위 Produces대로 쓴다.
- [ ] **Step 3: 스킬 시험** — 같은 요청을 새 scratch 폴더에서 다시 실행한다. 기대 결과: `new-project.sh`로 뼈대 생성, `EquipmentView` 배치 참고, 빌드 성공, `main.png` 생성, 표준 `Button`/`DataGridView` 직접 사용 없음.
- [ ] **Step 4: 추가 시험 2건**
  - 필수 입력 중 "dialog 유무"를 빼고 요청 → 그것만 묻는지 확인한다.
  - SDK 형식 net8 프로젝트 폴더에서 요청 → 중단하고 이유를 말하는지 확인한다.
- [ ] **Step 5: 기록과 정리** — 기준선 대비 결과(기준 준수, 토큰)를 이 계획 맨 끝 "시험 결과" 절에 적고 커밋한다: `git commit -m "docs: apc-field-ui 스킬 시험 결과"`. 시험에서 드러난 빈 곳은 SKILL.md를 고쳐 Step 3을 다시 실행한다.
