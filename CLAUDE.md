# APC.UI — 에이전트 작업 지침

APCSOFT 현장 PC 프로그램(WinForms)용 UI 기준 저장소. 사용법·폴더 구조는 `docs/README.md`, 설계 기준은 `docs/superpowers/specs/2026-11-17-apcsoft-field-ui-design.md`.

## 제약
- .NET Framework 4.0 / C# 7.3. 최신 문법(`record`, `init`, nullable 참조형 등)과 NuGet 패키지 추가 금지.
- 작업 완료 기준: `./build.sh` 실행 결과 `ALL PASSED`. 실패 상태로 끝내지 않는다.

## 건드리지 말 것
- `Resources/`, `references/` — 원본 자료. 읽기만 한다.
- `themes/*/assets/*.png` — 생성물. `assets-src/`의 SVG를 고친 뒤 `node tools/render-assets.mjs`로 다시 만든다(처음 한 번 `cd tools && npm install`).
- `theme.json` 상태색(run/warn/alarm/stop/info)은 명도만 조정 가능. 색상을 바꾸면 `ThemeLoader`가 로드를 거부한다.
- 고객사 맞춤은 `themes/<고객사>/`에서만. `themes/default`는 모든 고객사 공통 기준이다.

## 화면·컨트롤 추가
- 새로 만들기 전에 `src/APC.UI/Controls/`의 기존 컨트롤과 `src/APC.UI.Demo/Templates/`의 템플릿을 먼저 확인하고 재사용한다.
- 새 컨트롤은 `APC.UI`에 두고, 갤러리(`src/APC.UI.Demo/GalleryForm.cs`)에 예시를 추가하고, 로직이 있으면 `src/APC.UI.Checks/`에 점검을 추가한다.

## 스킬
- `skills/apc-field-ui/SKILL.md`는 `tools/new-project.sh`, `APC.UI.Tools`(api/render/attach), `render-assets.mjs`의 인자와 데모 템플릿 이름을 그대로 적어 쓴다. 이것들을 바꾸면 SKILL.md도 고치고 `bash tools/install-skill.sh`로 다시 설치한다.
- 설치본(`~/.claude/skills/apc-field-ui`)을 직접 고치지 않는다. 원본은 저장소다.

## Git
- `main`에 직접 커밋하지 않는다. 작업마다 브랜치를 만든다.
- 커밋 메시지: `<type>: <설명>` (feat, fix, refactor, docs, chore …).
- `APC.UI`의 public API가 바뀌면 `src/APC.UI/Properties/AssemblyInfo.cs`의 버전을 올린다(고객 프로젝트에 복사된 dll 구분용).
