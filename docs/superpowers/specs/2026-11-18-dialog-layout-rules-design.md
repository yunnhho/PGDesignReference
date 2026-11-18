# dialog 배치 고정 규칙 (2026-11-18)

## 의도
`apc-field-ui` 스킬 첫 결과물의 수정 횟수를 줄인다. 사용자가 반복해서 고치는 것: dialog 글씨(크기·굵기·폰트), 행·요소 구분, 배치·간격 비정렬.
원인: dialog에는 공통 치수·헬퍼가 없어 템플릿 5개가 숫자를 각자 박았다(여백 14/16/20/24, 행 34/48/52, 버튼 높이 34/40/44·폭 100~150·사이 8/10/12, 이름표 보통/굵게, `FontSize*1.3f`). 에이전트는 그중 하나를 베끼거나 숫자를 지어낸다.

## 목표 / 비목표 / 반목표
- 목표: dialog의 치수·글꼴·정렬을 에이전트가 정할 여지 없이 라이브러리가 정한다. 어기면 render 점검이 잡는다.
- 비목표: 메인 화면 배치 재설계(간격 값 통일만), 새 컨트롤, 테마 색·폰트 크기 변경.
- 반목표: 헬퍼가 너무 많아 에이전트가 고르다 틀리는 것. 헬퍼를 우회한 코드가 점검을 통과하는 것.

## 접근 (사용자 선택: B)
| 안 | 내용 | 판단 |
|---|---|---|
| A | SKILL.md 4-1에 규칙 표만 추가, 템플릿 맞춤 | 글 규칙은 지금까지 넘어갔다 |
| **B** | 공통 치수 상수 + `ApcForm` dialog 헬퍼 + 템플릿 재작성 + render 점검 추가 | 채택. 숫자를 에이전트가 안 정함 |
| C | render 점검만 추가 | 잡아도 고칠 때 다시 짐작 |

## 고정값 (배율 전 px)
| 항목 | 값 |
|---|---|
| dialog 바깥 여백 | 16 |
| 입력칸 높이 | `ApcTextBox.BaseHeight`(34) |
| 입력 행 사이 | 12 |
| 묶음(`ApcGroup`) 사이 | 16 |
| 이름표 ↔ 입력칸 | 12 |
| 하단 버튼 | 110×40, 사이 8, 내용과의 간격 16 |
| 글꼴 | 테마 글꼴·크기만. 굵게는 묶음 제목(`ApcGroup`이 그림)·화면 구획 제목에만, 입력 행 이름표는 보통 |
| 이름표 열 | dialog 하나에 하나. 폭 = 가장 긴 이름표, 왼쪽 정렬 → 모든 입력칸 왼쪽 끝이 같다 |
| 단위 | 입력은 이름표 괄호 `하중 상한 (kN)`, 표시는 `ApcValueCard.Unit`·표 열 제목 |
| 메인 화면 `Row()` 줄 간격 | 10 → 12(`Columns()`와 같게) |

## 설계
1. `APC.UI.Theming.ApcLayout`(public static): 위 값을 상수로. `ApcTemplateView`·`ApcForm` 헬퍼·`LayoutLint`가 모두 이것을 쓴다.
2. `ApcForm` protected 헬퍼(dialog용, 호출 순서 = 위에서 아래):
   - `SetupDialog(string title, int width)` — TitleText, Resizable=false, MinimizeBox=false, ShowInTaskbar=false, CenterParent, 폭(배율 전 px).
   - `AddFieldRow(string caption, Control field)` — 이름표+입력칸 한 줄. 입력칸 폭 기본은 남은 폭, 지정 가능.
   - `AddGroup(string title)` → 그 안에 행을 넣는 대상(묶음 안에서도 같은 이름표 열).
   - `AddContent(Control c, int height)` — 표·그래프 등 자유 요소. height 생략 시 남은 높이 채움.
   - `SetFooter(ApcButton cancel, ApcButton confirm, params ApcButton[] left)` — 오른쪽 [취소][확정], 왼쪽 보조. `CancelButton` 자동 지정. 확정만 있는 보기용은 cancel=null.
   - 높이는 표시 직전 내용으로 계산(`AddContent`가 남은 높이 채움이면 `SetupDialog`의 높이 인자 사용). 손으로 `ClientSize`를 맞추지 않는다.
   - 도킹 순서 함정(나중 Add가 위)은 헬퍼 안에서 처리한다.
3. 템플릿 5개(`RecipeDialog`, `SettingsDialog`, `AdminPasswordDialog`, `LotInputDialog`, `EquipmentDetailDialog`)를 헬퍼로 다시 쓴다. 정보 배치는 유지, 숫자·글꼴만 고정값으로.
4. render 점검(`LayoutLint`) 추가 — dialog(`ApcForm`)에서:
   - 글꼴: 테마와 다른 글꼴명·크기의 `Label`/Apc 컨트롤. 입력 행 이름표가 굵게.
   - 이름표 열: 같은 부모 안 입력 행들의 첫 입력칸 왼쪽 끝이 다름.
   - 행 간격: 같은 부모 안 연속 입력 행의 세로 간격이 서로 다름.
   - 하단 버튼: 맨 아래 버튼 줄의 버튼 크기가 서로 다르거나 고정값과 다름.
   - 바깥 여백: 내용 가장자리(좌·우·아래)와 폼 가장자리 거리가 여백 고정값과 다름(손으로 맞춘 ClientSize로 생긴 빈 공간).
5. `APC.UI.Checks`에 헬퍼(이름표 열 정렬, 버튼 순서, 높이 자동 계산)와 새 점검 항목 검사 추가. 기존 템플릿 render 점검 0건 유지.
6. SKILL.md: 4b를 "dialog는 헬퍼로만 만든다 + 고정값 표"로 바꾸고, 4-1에 글꼴·단위 행 추가. `install-skill.sh`로 재설치.
7. public API 추가 → `AssemblyInfo` 1.7.0.0.

## 빌더 몫
- 헬퍼 내부 구현(패널 구조, 높이 계산 시점), 점검 허용 오차(±1px), 입력칸 기본 폭 규칙.
- `SettingsDialog`처럼 한 줄에 이름표+입력칸 여러 쌍이 있는 행: `AddFieldRow` 오버로드 또는 `AddContent`로 처리 — 이름표 열 점검은 행의 첫 입력칸만 본다.

## 완료 기준
`./build.sh` ALL PASSED, 템플릿 5개 render `점검 0건`, 일부러 어긋난 dialog 샘플이 새 점검에 걸림.
