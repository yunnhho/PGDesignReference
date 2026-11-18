#!/bin/bash
# 저장소의 skills/apc-field-ui를 Claude Code 개인 스킬 폴더에 설치한다(덮어씀).
#   bash tools/install-skill.sh [대상 skills 폴더]   (기본: ~/.claude/skills)
# SKILL.md 안의 저장소 경로(C:\apc-field-ui, /c/apc-field-ui)는 이 저장소의 실제 위치로 바꿔 넣는다.
REPO="$(cd "$(dirname "$0")/.." && pwd)"
DEST="${1:-$HOME/.claude/skills}/apc-field-ui"
SRC="$REPO/skills/apc-field-ui/SKILL.md"
[ -f "$SRC" ] || { echo "오류: $SRC 없음" >&2; exit 1; }

text="$(cat "$SRC")" || exit 1
# 따옴표 친 패턴 = 글자 그대로 치환(백슬래시 이스케이프 불필요)
text="${text//"/c/apc-field-ui"/"$REPO"}"
text="${text//"C:\\apc-field-ui"/"$(cygpath -w "$REPO")"}"

mkdir -p "$DEST" || { echo "오류: $DEST 를 만들 수 없습니다" >&2; exit 1; }
printf '%s\n' "$text" > "$DEST/SKILL.md" || { echo "오류: 설치 실패" >&2; exit 1; }
echo "설치: $DEST/SKILL.md"
