---
name: apc-field-ui
description: Use when building a new APCSOFT field PC program screen in WinForms (.NET Framework 4.x) — 현장 PC 프로그램, 설비 모니터링, 라인별/센서 모니터링, 공정 설비 프로그램(Failure Analysis 등), 새 화면·새 프로젝트, APC.UI, ApcForm, ApcGrid. Not for WPF, web, or .NET 6/8 projects.
---

# APC 현장 UI 화면 만들기

APCSOFT 현장 프로그램 화면은 **APC.UI 라이브러리**로 만든다. 원본 저장소: `C:\00P_PGDesignReference` (이하 `$REPO`, Git Bash 경로 `/c/00P_PGDesignReference`).
결과물: 샘플 데이터가 들어간 **메인 화면 하나 + (필요하면) dialog** 껍데기와, 데이터 연결용 **인터페이스 자리**. 실제 PLC/DB/시리얼 연결 코드는 만들지 않는다.

## 1. 입력 확인

| 필수 | 선택 |
|---|---|
| **목적**: 모니터링(라인 전체 / 설비 부착 센서 등) 또는 공정 설비 프로그램(Failure Analysis 등) | 예전 프로그램 캡처 |
| **메인 기능** | 고객사 요구서 파일 |
| **상세 dialog 유무** | 추가 상세 요청 |

필수 중 빠진 것만 한 번에 묻는다. 셋 다 있으면 바로 진행한다. 요구서가 있으면 메인 기능 목록을 거기서 보완한다. 고객사명이 없으면 `default`로 하고 그렇게 했다고 말한다.

## 2. 프로젝트 판별과 준비

먼저 저장소 도구가 빌드돼 있어야 한다: `$REPO/src/APC.UI.Tools/bin/Debug/APC.UI.Tools.exe`가 없으면 `bash /c/00P_PGDesignReference/build.sh`를 실행한다(`ALL PASSED` 확인).

| 작업 폴더 상태 | 할 일 |
|---|---|
| csproj 없음 | `bash /c/00P_PGDesignReference/tools/new-project.sh <폴더> <프로젝트명> <고객사> [--framework v4.x]` |
| 구형 csproj, `TargetFrameworkVersion` v4.x, WinForms | 같은 명령에 `--into-existing` (dll·테마·참조만 추가, 재실행 시 dll만 교체) |
| SDK 형식(`<Project Sdk=`), net6/8, WPF, 웹 | **중단.** APC.UI는 .NET Framework 4.x WinForms 전용이라고 알린다 |

`--into-existing`일 때 스크립트가 하지 않는 두 가지를 직접 한다:
- `Main`에서 첫 폼을 띄우기 전에 `ApcTheme.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"themes\<고객사>"))`를 호출하고 `ThemeException`이면 메시지를 보여 주고 끝낸다. 없으면 앱이 시작하자마자 죽는다(render는 테마를 직접 로드하므로 이 누락을 잡지 못한다).
- csproj가 `<Compile Include="파일.cs" />`를 하나씩 나열하면, 새로 만든 `.cs`마다 항목을 추가한다(`**\*.cs` 와일드카드면 불필요).

코드는 **그 프로젝트의** `LangVersion`/`TargetFrameworkVersion`을 따른다(v4.x면 C# 7.3까지: `record`, `init`, `string?`, switch 식, `new()` 금지).

## 3. API 파악

`$REPO/docs/api.md`(자동 생성 API 요약)를 읽는다. 컨트롤 소스 전체를 읽지 않는다. 배치 참고용 템플릿은 아래에서 고른 1~2개만 `$REPO/src/APC.UI.Demo/Templates/`에서 읽는다.

## 4. 배치

| 목적 | 출발 템플릿 |
|---|---|
| 모니터링: 라인 전체, 설비 상태 | `EquipmentView` |
| 모니터링: 센서 값 카드·추이 | `TempHumiView` |
| 공정 설비 프로그램: 분석·판정 | `FailureAnalysisView` |
| 작업 입력: 시작·실적·불량 | `WorkStartView`, `ProductionView`, `DefectView` |
| 조회 | `HistoryView` |

- 맞는 템플릿이 없거나 추가 요청이 있으면 `ApcTemplateView`(헤더 + Body + 알람바) 위에 **기존 Apc 컨트롤로 새 배치**를 짠다.
- 배치는 생성자 코드로만 한다(디자이너 파일 없음). 크기는 `S(px)`로 배율 적용.
- 캡처가 있으면 원본의 정보 배치(무엇이 어디)는 유지하고 모양만 APC.UI로 바꾼다.
- dialog는 `ApcDialog` 기반 별도 폼. 되돌릴 수 없는 동작 앞에는 `ApcDialog.Confirm(this, "...")`.
- 상태 표시는 `StatusKind`(Run/Warn/Alarm/Stop/Info) + `ApcLamp`/`ApcLampColumn`. 색은 테마가 정한다.

## 5. 새 그림이 필요할 때만

1. 프로젝트의 `assets-src/`에 SVG를 만든다. 이름 규칙 `{btn|lamp|win|icon|logo}_{이름}_{normal|hover|pressed|disabled}.svg`(램프·로고는 상태 없음), `<svg width="..">` 필수. 기존 SVG(`$REPO/assets-src/`)를 본떠 그린다.
2. `$REPO/tools/node_modules`가 없으면 먼저 `cd /c/00P_PGDesignReference/tools && npm install`(한 번만). 그다음
   `node /c/00P_PGDesignReference/tools/render-assets.mjs --out <프로젝트>/themes/<고객사>/assets --override <프로젝트>/assets-src`
3. 기존 이름을 대체한 그림은 컨트롤이 자동으로 쓴다. 새 이름은 `ApcTheme.GetImage("icon_xxx_normal")`로 쓴다.

## 6. 데이터 자리

메인 화면은 `Data/IMainViewSource`(인터페이스)에만 의존하고, `Data/SampleMainViewSource`가 샘플을 준다. 화면에 맞게 인터페이스 메서드와 행 모델을 바꾼다. 화면에는 **매개변수 없는 생성자**(샘플 사용)를 남긴다 — render가 쓴다. dialog는 메인 화면에서 값을 넘겨받고 자체 인터페이스를 두지 않는다. dialog에도 **샘플 값을 넣는 매개변수 없는 생성자**를 둔다 — 없으면 render할 수 없다.

## 7. 완료 확인

1. MSBuild로 빌드: `"/c/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe" <sln> -v:m`
2. 메인 폼과 각 dialog를 PNG로:
   `/c/00P_PGDesignReference/src/APC.UI.Tools/bin/Debug/APC.UI.Tools.exe render <bin/Debug/앱.exe> MainForm <bin/Debug/themes/고객사> <out.png>` (Windows 경로는 `cygpath -w`)
   캡처를 받았다면 `--compare <캡처.png>` → `<out>.compare.png`
3. PNG를 직접 열어 보고 사용자에게 경로와 함께 보여 준다.

render 제한: 도구는 64비트로 돈다. `PlatformTarget`이 x86인 앱이나 생성자에서 32비트 네이티브 DLL을 부르는 화면은 "어셈블리를 읽을 수 없습니다"로 실패한다. 그때는 빌드 성공까지만 확인하고 render를 못 한 이유를 사용자에게 말한다.

## 하지 않는 것

| 하지 않음 | 대신 |
|---|---|
| 표준 `Button`/`DataGridView`/`Form`으로 직접 그리기 | `ApcButton`/`ApcGrid`/`ApcForm` |
| 상태색 색상 변경(run=초록, warn=노랑, alarm=빨강, stop=회색, info=파랑) | 명도만 고객사 `theme.json`에서 |
| `$REPO/themes/default`, `$REPO/assets-src/*.svg` 수정 | 프로젝트의 `themes/<고객사>`, `assets-src/` |
| 컨트롤 API 추측 | `docs/api.md` 확인 |
