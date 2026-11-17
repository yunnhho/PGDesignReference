# APC.UI — APCSOFT 현장 PC 프로그램 UI 기준

WinForms(.NET Framework 4.0~4.8) 현장 프로그램용 컨트롤·테마·이미지 묶음. 설계: `docs/superpowers/specs/2026-11-17-apcsoft-field-ui-design.md`

## 새 고객사 프로젝트에서 쓰는 법

**빠른 길**: `tools/new-project.sh <대상폴더> <프로젝트명> <고객사> [--framework v4.0]`가 아래 1~5단계를 한 번에 해 준다(뼈대 + `lib/APC.UI.dll` + `themes/<고객사>` + 샘플 데이터 메인 화면). 기존 .NET Framework 4.x WinForms 프로젝트에는 `--into-existing`을 붙인다(dll·테마·csproj 참조만 추가, 재실행하면 dll만 교체). 저장소에서 `./build.sh`를 한 번 실행한 뒤 쓴다.

도구(`src/APC.UI.Tools/bin/Debug/APC.UI.Tools.exe`):
- `api <dll> <out.md>` — public API 요약. `build.sh`가 `docs/api.md`를 매번 다시 만든다.
- `render <exe|dll> <형식명> <themeDir> <out.png> [--size 1920x1080] [--compare <원본.png>]` — 화면을 PNG로, 원본 캡처와 나란히 비교본도.
- `attach <csproj>` — 기존 csproj에 APC.UI 참조와 테마 출력 복사 추가.

직접 할 때:

1. `src/APC.UI/APC.UI.csproj`를 Release로 빌드해(VS에서 구성 Release, 또는 `MSBuild src/APC.UI/APC.UI.csproj -p:Configuration=Release`) 나온 `src/APC.UI/bin/Release/APC.UI.dll`을 참조에 추가한다. `./build.sh`는 Debug만 빌드한다.
2. `themes/default` 폴더를 `themes/<고객사>`로 복사해 프로그램 실행 폴더에 같이 배포한다.
3. `Main`에서 폼을 띄우기 전에 `ApcTheme.Load(@"...\themes\<고객사>")`를 호출한다. `ThemeException`이 나면 메시지에 파일과 항목이 적혀 있다.
4. 폼은 `ApcForm`을 상속하고, `ApcButton` / `ApcGrid` / `ApcLampColumn` / `ApcScanBox` / `ApcHeaderBar` / `ApcAlarmBar` / `ApcLamp`를 쓴다. 삭제 같은 되돌릴 수 없는 동작 앞에는 `ApcDialog.Confirm(this, "...")`를 둔다.
5. 앱 매니페스트에 `<dpiAware>true</dpiAware>`를 넣는다(`src/APC.UI.Demo/app.manifest` 참고).

## 고객사 맞춤

| 바꿀 것 | 방법 |
|---|---|
| 색·폰트·스캔 설정 | `themes/<고객사>/theme.json` 수정. 상태색(run/warn/alarm/stop/info)은 **명도만** 바꿀 수 있고 색상을 바꾸면 로드가 거부된다. |
| 로고 | `themes/<고객사>/assets/logo_apcsoft@1x.png` 교체(파일명은 `theme.json`의 `logo` 값). `@1.5x`, `@2x`도 넣으면 고배율에서 선명해진다. |
| 버튼·램프·창 버튼 이미지 | 바꿀 SVG만 `assets-src/<고객사>/`에 복사해 고친 뒤:<br>`node tools/render-assets.mjs --out themes/<고객사>/assets --override assets-src/<고객사>` |

이미지 파일 이름 규칙: `{btn|lamp|win|icon|logo}_{이름}_{normal|hover|pressed|disabled}@{1x|1.5x|2x}.png`(램프·로고는 상태 없음).
처음 한 번은 `cd tools && npm install`.

## 빌드·점검

```sh
./build.sh          # VS2019 MSBuild로 솔루션 빌드 + APC.UI.Checks 자가점검 (ALL PASSED 확인)
src/APC.UI.Demo/bin/Debug/APC.UI.Demo.exe [--theme sample]   # 갤러리
```

## 폴더

```
assets-src/   SVG 원본(수정은 여기서)      themes/       테마별 theme.json + assets/*.png
src/APC.UI/   컨트롤 라이브러리            src/APC.UI.Demo/    갤러리(카탈로그 + 기능 템플릿 5종)
src/APC.UI.Checks/  자가점검 콘솔           tools/        SVG → PNG 렌더러, new-project.sh, smoke.sh, project-template/
src/APC.UI.Tools/   api·render·attach 도구
Resources/    예전 납품 이미지(참고용, 수정하지 않음)
references/   기존 현장 프로그램 화면 캡처(UI 레퍼런스)
references/redesign/  위 화면을 APC.UI로 재구성한 결과(*.apcui.png)와 원본 비교(*.compare.png)
```
