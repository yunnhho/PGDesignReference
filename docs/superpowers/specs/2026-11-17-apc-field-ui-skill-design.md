# APC 현장 UI 스킬 — 설계 문서

작성일: 2026-11-17 · 상태: 검토 대기

## 1. 의도

새 고객사 현장 프로그램(WinForms)을 에이전트로 만들 때, 에이전트가 **처음부터 APC.UI 기준대로** 화면을 만들게 한다.

이 저장소의 `CLAUDE.md`는 다른 프로젝트 폴더에서 자동으로 읽히지 않는다. 그래서 스킬이 없으면 에이전트는 APC.UI의 존재를 모르고 표준 WinForms로 화면을 만든다. 저장소를 알려 줘도 매번 탐색 범위가 달라 규칙(상태색, default 테마 보호)을 놓친다. 스킬의 일차 효과는 **기준 준수와 결과의 일관성**이다. 스크립트로 반복 작업(뼈대·API 파악·렌더)을 넘겨 토큰과 실패율을 줄이는 것은 그다음 효과다.

호출 장면: 새 프로젝트 폴더에서 "이런 화면 만들어 줘"라고 요청한다.

## 2. 목표 / 비목표 / 반목표

**목표**
- 개인 스킬 `~/.claude/skills/apc-field-ui/SKILL.md`
- 요청을 받으면 샘플 데이터가 들어간 **화면 껍데기**와, 데이터 연결용 **인터페이스 자리**까지 만든다.
- 매번 같아야 하는 일(뼈대 생성, API 파악, 렌더 확인)은 저장소 스크립트가 맡는다. 에이전트는 화면 배치 판단에 집중한다.

**비목표**
- 실제 PLC, DB, 시리얼 연결 코드는 만들지 않는다.
- 기존 레거시 화면을 APC.UI로 옮기는 절차는 만들지 않는다. (캡처를 입력으로 받는 것은 범위 안이다. §5.2 참고)
- .NET 6/8 대상 빌드는 하지 않는다. 앞으로도 쓸 계획이 없다.
- 플러그인이나 MCP로 감싸지 않는다. 팀 배포가 필요해지면 그때 감싼다.
- VS 디자이너 지원은 하지 않는다. 화면은 템플릿과 같은 방식으로 **코드로만** 배치한다(생성자에서 배치, `S()`로 배율 처리).
- 여러 화면을 오가는 화면 전환 틀은 만들지 않는다. 새 프로그램은 **메인 화면 하나 + dialog** 구성이 기본이다.

**반목표** (동작하더라도 이렇게 되면 실패로 본다)
- 표준 `Button`, `DataGridView` 등으로 직접 그린 화면
- 상태색(run/warn/alarm/stop/info)의 색상을 바꾸는 것
- `themes/default`나 저장소의 `assets-src/*.svg`(공통 원본)를 고객사 요청으로 수정하는 것
- 스킬 문서에 컨트롤 API를 손으로 적어 두는 것. 저장소와 어긋나기 시작한다.

## 3. 제약

- 새 프로젝트는 **.NET Framework 4.x WinForms**만 받는다. 그 외(.NET 6/8, WPF, 웹)에서는 스킬이 중단하고 이유를 알린다.
- 새 프로젝트의 코드는 **새 프로젝트 자신의** `TargetFrameworkVersion`과 `LangVersion`을 따른다. C# 7.3 제약은 APC.UI 내부 코드에만 적용된다. 스킬에 7.3을 고정하지 않는다.
- 저장소 쪽 제약은 그대로 유지한다: .NET 4.0 / C# 7.3, NuGet 추가 금지, `./build.sh` → `ALL PASSED`.
- 원본은 저장소 하나다. 스킬은 저장소 경로(`C:\00P_PGDesignReference`)를 가리키기만 하고, 저장소 내용을 복사해 두지 않는다.

## 4. 검토한 접근 — **B 선택**

| | 내용 | 결론 |
|---|---|---|
| A. SKILL.md 하나 | 호출할 때마다 에이전트가 소스를 읽고, 뼈대와 렌더 스크립트를 직접 작성한다. | 기준 준수 효과는 얻는다. 하지만 반복 비용(추정 25–35k 토큰)이 크고, .NET 4.0 구형 csproj를 손으로 쓰다 깨지는 등 품질이 호출마다 다르다. |
| **B. SKILL.md + 저장소 스크립트** | 뼈대 생성, API 요약 자동 생성, 렌더를 스크립트로 처리한다. | **선택.** 반복 비용을 추정 5–8k 토큰으로 줄이고 결과를 결정적으로 만든다. 대가는 저장소 작업량이 A의 2~3배라는 점이다. |
| C. 플러그인 / MCP | B를 패키지로 감싼다. | 혼자 쓰는 지금은 효율이 B와 같고 관리할 것만 늘어난다. |
| (제외) dotnet `new` 템플릿 | | .NET Framework 4.0 구형 프로젝트와 잘 맞지 않는다. |

배포 방식으로 dll 참조를 유지하는 이유: WinForms 커스텀 컨트롤을 VS 디자이너 도구상자에서 쓰려면 컴파일된 어셈블리가 필요하다. 소스를 복사하면 프로젝트마다 갈라진다.

## 5. 설계

### 5.1 저장소 변경

| # | 변경 | 내용 |
|---|---|---|
| 1 | `TemplateView`를 dll로 이동 | `src/APC.UI.Demo/Templates/TemplateView.cs`(internal)를 `src/APC.UI`의 public 클래스로 옮긴다. 헤더바 + 본문 + 알람바 틀과 `S()`, `Caption()` 같은 도우미를 포함한다. 데모 템플릿 7종은 이 클래스를 상속하도록 바꾼다. 갤러리 모습은 이전과 같아야 한다. 디자이너 지원을 하지 않으므로, 기존 `protected TemplateView(string title)` 생성자를 그대로 유지한다. |
| 2 | dll 버전 규칙 | public API가 바뀌면 `AssemblyVersion`(현재 1.0.0.0)을 올린다. 프로젝트에 복사된 dll이 어느 시점 것인지 알 수 있게 하기 위해서다. 규칙은 저장소 `CLAUDE.md`에 한 줄 추가한다. |
| 3 | 도구 콘솔 `src/APC.UI.Tools` | .NET 4.0 콘솔 앱 하나에 명령 두 개를 둔다.<br>· `api <dll> <out.md>`: 리플렉션으로 public 형식과 멤버(속성·메서드·이벤트, XML 요약 주석 포함)를 Markdown으로 뽑는다.<br>· `render <어셈블리> <형식명> <themeDir> <out.png> [--size 1920x1080] [--compare <원본.png>]`: `ApcTheme.Load` 후 `UserControl`이나 `Form`을 오프스크린으로 그려 PNG로 저장한다. `--compare`를 주면 원본과 나란히 놓은 비교본도 만든다. |
| 4 | `docs/api.md` | `build.sh`가 빌드 후 `APC.UI.Tools api`로 매번 다시 만든다. 맨 위에 dll 버전을 적는다. 커밋 대상이다. |
| 5 | `tools/new-project.sh` | 새 프로젝트 뼈대를 만든다(§5.3). |
| 6 | `build.sh` 점검 확장 | 임시 폴더에 `new-project.sh`로 프로젝트를 만들고 → 빌드하고 → 기본 화면을 `render`해서 PNG가 나오는지 확인한다. 이 과정이 실패하면 `ALL PASSED`가 나오지 않는다. |
| 7 | `render-assets.mjs` 보완 | 현재는 기본 폴더에 있는 이름만 처리하므로, `--override` 폴더에만 있는 새 이름(`icon_*` 등)은 조용히 건너뛴다. 이런 파일도 렌더하도록 고친다. |

### 5.2 스킬 흐름 (`SKILL.md`)

1. **입력 확인**
   - 필수: **목적**(모니터링: 공정 라인 전체 / 설비 부착 센서 등, 또는 공정 설비 프로그램: Failure Analysis 등), **메인 기능**, **상세 dialog 유무**. 빠진 것만 묻고, 다 있으면 바로 진행한다.
   - 선택: 예전 프로그램 캡처, 고객사 요구서 파일, 추가 상세 요청. 요구서가 있으면 메인 기능 목록을 거기서 보완한다.
2. **프로젝트 확인과 준비**
   - csproj가 없으면 `new-project.sh`로 만든다.
   - csproj가 있으면 .NET Framework 4.x WinForms인지 확인하고, 아니면 중단한다. 맞으면 `new-project.sh --into-existing`으로 dll·테마·공통 파일만 넣는다(§5.3).
   - 새 프로젝트의 `LangVersion`을 읽어 이후 코드에 적용한다.
3. **API 파악**: `docs/api.md`를 읽는다. 배치를 참고할 템플릿 소스는 고른 것 1~2개만 읽는다.
4. **배치 결정**
   - 목적에 맞는 기존 템플릿이 있으면 그 배치를 따른다.

     | 목적 | 출발 템플릿 |
     |---|---|
     | 모니터링(라인 전체, 설비 상태) | `EquipmentView` |
     | 모니터링(센서 값 추이) | `TempHumiView` |
     | 공정 설비 프로그램(분석·판정) | `FailureAnalysisView` |
     | 작업 입력 위주(시작·실적·불량) | `WorkStartView`, `ProductionView`, `DefectView` |
     | 조회 | `HistoryView` |

   - 목적이 애매하거나 추가 요청이 있으면 `TemplateView` 틀 위에 **기존 컨트롤로 새 배치**를 짠다.
   - 캡처가 있으면 원본의 정보 배치(무엇이 어디 있는지)는 유지하고, 모양만 APC.UI 기준으로 바꾼다.
   - 상세 dialog는 `ApcDialog` 기반 별도 폼으로 만들고, 되돌릴 수 없는 동작 앞에는 `ApcDialog.Confirm`을 둔다.
5. **새 그림이 필요할 때만**
   - 요청에 맞는 SVG를 **새 프로젝트의** `assets-src/`에 기존 파일명 규칙(`{btn|lamp|win|icon|logo}_{이름}_{상태}`)대로 만든다. 저장소는 건드리지 않는다. 따라서 저장소 쪽 git·브랜치 처리도 필요 없다.
   - `node <저장소>/tools/render-assets.mjs --out <새 프로젝트>/themes/<고객사>/assets --override <새 프로젝트>/assets-src`로 PNG를 만든다.
   - 기존 이름을 대체하는 그림은 컨트롤이 자동으로 사용한다. 새 이름의 그림은 `ApcTheme.GetImage`로 불러 쓴다.
6. **데이터 자리**: 메인 화면용 `IMainViewSource` 인터페이스 하나와 `SampleMainViewSource` 샘플 구현 하나를 둔다. 화면 코드는 인터페이스에만 의존한다. dialog는 메인 화면에서 필요한 값을 넘겨받고, 자체 데이터 인터페이스를 두지 않는다.
7. **완료 확인**: 새 프로젝트 빌드 성공 → 메인 화면과 각 dialog를 `render`로 PNG 생성 → 사용자에게 보여 준다. 캡처를 받았다면 `--compare` 비교본도 보여 준다.

### 5.3 `tools/new-project.sh`

```
tools/new-project.sh <대상폴더> <프로젝트명> <고객사> [--framework v4.0] [--into-existing]
```

새로 만들 때 생성하는 것:
- `<프로젝트명>.sln`, `<프로젝트명>.csproj` (구형 형식, WinExe, 지정한 `TargetFrameworkVersion`, `lib\APC.UI.dll` 참조)
- `Program.cs`: `ApcTheme.Load(@"themes\<고객사>")`, `ThemeException` 처리, `MainForm` 실행
- `MainForm.cs` (`ApcForm` 상속), 기본 화면 `Views/MainView.cs` (`TemplateView` 상속)
- `Data/IMainViewSource.cs`, `Data/SampleMainViewSource.cs`
- `app.manifest` (`dpiAware`)
- `lib/APC.UI.dll`: 저장소에서 Release로 빌드해 복사한다.
- `themes/<고객사>/`: 저장소에 `themes/<고객사>`가 있으면 그것을, 없으면 `themes/default`를 복사한다. 복사한 뒤 고객사 테마는 **프로젝트 쪽에서** 관리한다. 테마 폴더는 빌드 출력에 복사되도록 csproj에 등록한다.

`--into-existing`일 때:
- **구형 csproj만** 지원한다. SDK 형식이면 중단하고 이유를 알린다.
- 처음 실행할 때는 `lib/`와 `themes/`를 넣고, 기존 csproj에 참조와 테마 복사 항목을 추가한다(이미 있으면 중복 추가하지 않는다). 나머지 파일은 만들지 않는다.
- 이미 APC.UI가 들어간 프로젝트에서 다시 실행하면 `lib/APC.UI.dll`만 새 버전으로 교체한다. 프로젝트에서 고쳤을 수 있으므로 테마는 덮어쓰지 않는다. 이것이 §5.1-2 버전 규칙의 갱신 경로다.

## 6. 구현자 판단에 맡기는 것

- `api.md`의 서식 세부(멤버 정렬, 상속 멤버 생략 범위)
- `render`가 `DrawToBitmap`으로 그리지 못하는 컨트롤을 처리하는 방법
- SKILL.md 문구와 description 트리거 문장. 단, 본문은 짧게 쓰고 API는 `docs/api.md`만 가리킨다.
- 스크립트 세부 옵션 이름

## 7. 확인 방법

- `./build.sh` → `ALL PASSED`. 기존 점검, `api.md` 생성, new-project→빌드→render 점검이 모두 포함된다.
- 갤러리 실행: `TemplateView` 이동 전후에 템플릿 7종의 모습이 같다.
- 스킬 시험: 빈 폴더에서 "설비 모니터링 화면, 메인 기능: 라인별 가동 상태, dialog 없음"으로 호출해 빌드와 PNG까지 나오는지 확인한다. 같은 요청을 스킬 없이도 한 번 실행해 결과(기준 준수 여부, 토큰)를 비교한다.
