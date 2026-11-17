#!/bin/sh
# APC.UI 기반 새 현장 프로그램 뼈대를 만들거나, 기존 .NET Framework 4.x WinForms 프로젝트에 APC.UI를 넣는다.
#
#   tools/new-project.sh <대상폴더> <프로젝트명> <고객사> [--framework v4.0] [--into-existing]
#
# 공통: APC.UI를 Release로 빌드해 <대상>/lib/에 복사(재실행하면 dll만 교체).
#       <대상>/themes/<고객사>가 없을 때만 저장소 themes/<고객사>(없으면 default)를 복사.
# 새로:  tools/project-template/을 복사해 이름·고객사·프레임워크를 채운다. 대상에 csproj가 있으면 중단.
# --into-existing: 대상 폴더의 csproj(1개)에 APC.UI 참조와 테마 출력 복사를 추가(APC.UI.Tools attach).
. "$(dirname "$0")/env.sh"
REPO="$(cd "$(dirname "$0")/.." && pwd)"

die() { echo "오류: $1" >&2; exit 1; }
win() { cygpath -w "$1"; }

[ $# -ge 3 ] || die "사용법: tools/new-project.sh <대상폴더> <프로젝트명> <고객사> [--framework v4.0] [--into-existing]"
DEST="$1"; NAME="$2"; CUSTOMER="$3"; shift 3
FRAMEWORK=v4.0
MODE=new
while [ $# -gt 0 ]; do
  case "$1" in
    --framework) [ $# -ge 2 ] || die "--framework 값이 없습니다"; FRAMEWORK="$2"; shift 2 ;;
    --into-existing) MODE=existing; shift ;;
    *) die "알 수 없는 옵션: $1" ;;
  esac
done

echo "$NAME" | grep -Eq '^[A-Za-z_][A-Za-z0-9_]*$' || die "프로젝트명은 C# 식별자여야 합니다(영문·숫자·_, 숫자로 시작 불가): $NAME"
case "$CUSTOMER" in
  "" | "." | ".." | " "* | *" " | *. | *[\\/:*?\"\<\>\|\&]*) die "고객사명이 비었거나 폴더 이름으로 쓸 수 없습니다(. .. 앞뒤 공백 끝의 . \\ / : * ? \" < > | & 불가): $CUSTOMER" ;;
esac
# C# 키워드는 식별자 정규식을 통과하지만 namespace·클래스 이름으로 쓸 수 없다.
CS_KEYWORDS=" abstract as base bool break byte case catch char checked class const continue decimal default delegate do double else enum event explicit extern false finally fixed float for foreach goto if implicit in int interface internal is lock long namespace new null object operator out override params private protected public readonly ref return sbyte sealed short sizeof stackalloc static string struct switch this throw true try typeof uint ulong unchecked unsafe ushort using virtual void volatile while "
case "$CS_KEYWORDS" in *" $NAME "*) die "프로젝트명으로 C# 키워드는 쓸 수 없습니다: $NAME" ;; esac
case "$FRAMEWORK" in
  v4.0 | v4.5 | v4.5.1 | v4.5.2 | v4.6 | v4.6.1 | v4.6.2 | v4.7 | v4.7.1 | v4.7.2 | v4.8 | v4.8.1) ;;
  *) die "--framework는 실제 .NET Framework 4.x 버전이어야 합니다(v4.0 v4.5 … v4.8 v4.8.1): $FRAMEWORK" ;;
esac

mkdir -p "$DEST" || die "대상 폴더를 만들 수 없습니다: $DEST"
CSPROJ_COUNT=$(find "$DEST" -maxdepth 1 -name '*.csproj' | wc -l)
if [ "$MODE" = new ]; then
  [ "$CSPROJ_COUNT" -eq 0 ] || die "대상 폴더에 이미 csproj가 있습니다. 기존 프로젝트에 넣으려면 --into-existing을 쓰세요."
else
  [ "$CSPROJ_COUNT" -eq 1 ] || die "--into-existing은 대상 폴더에 csproj가 정확히 1개 있어야 합니다(현재 $CSPROJ_COUNT개)."
  CSPROJ="$(find "$DEST" -maxdepth 1 -name '*.csproj')"
  # 연결할 수 없는 프로젝트면 lib/·themes/를 쓰기 전에 멈춘다.
  "$REPO/src/APC.UI.Tools/bin/Debug/APC.UI.Tools.exe" attach "$(win "$CSPROJ")" --check \
    || die "이 프로젝트에는 APC.UI를 연결할 수 없습니다(위 메시지 참고). 아무 파일도 바꾸지 않았습니다."
fi

# 1. APC.UI Release → lib/
LOG="$(mktemp)"
"$MSBUILD" "$(win "$REPO/src/APC.UI/APC.UI.csproj")" -p:Configuration=Release -v:q -nologo -p:PreferredUILang=en-US > "$LOG" 2>&1 \
  || { tail -20 "$LOG"; rm -f "$LOG"; die "APC.UI Release 빌드 실패"; }
rm -f "$LOG"
mkdir -p "$DEST/lib"
cp "$REPO/src/APC.UI/bin/Release/APC.UI.dll" "$REPO/src/APC.UI/bin/Release/APC.UI.xml" "$DEST/lib/" || die "dll 복사 실패"

# 2. 테마: 이미 있으면 프로젝트 쪽 수정을 지키기 위해 건드리지 않는다.
if [ ! -d "$DEST/themes/$CUSTOMER" ]; then
  SRC="$REPO/themes/$CUSTOMER"
  if [ ! -d "$SRC" ]; then
    SRC="$REPO/themes/default"
    echo "알림: 저장소에 themes/$CUSTOMER 이 없어 themes/default 를 복사했습니다(고객사 색·로고는 $DEST/themes/$CUSTOMER 에서 바꾼다)."
  fi
  mkdir -p "$DEST/themes"
  cp -r "$SRC" "$DEST/themes/$CUSTOMER" || die "테마 복사 실패"
fi

# 3. 프로젝트
if [ "$MODE" = existing ]; then
  "$REPO/src/APC.UI.Tools/bin/Debug/APC.UI.Tools.exe" attach "$(win "$CSPROJ")" \
    || die "csproj 연결 실패(위 메시지 참고)."
else
  GUID=$(powershell -NoProfile -Command "[guid]::NewGuid().ToString().ToUpper()" | tr -d '\r')
  TPL="$REPO/tools/project-template"
  LIST="$(mktemp)"
  (cd "$TPL" && find . -type f) > "$LIST"
  # 파이프 대신 리다이렉트: 루프가 현재 셸에서 돌아야 die가 스크립트 전체를 끝낸다.
  while read -r rel; do
    out="$DEST/$(echo "${rel#./}" | sed "s/__NAME__/$NAME/g")"
    mkdir -p "$(dirname "$out")" \
      && sed -e "s/__NAME__/$NAME/g" -e "s/__GUID__/$GUID/g" -e "s/__FRAMEWORK__/$FRAMEWORK/g" -e "s|__CUSTOMER__|$CUSTOMER|g" "$TPL/$rel" > "$out" \
      || { rm -f "$LIST"; die "템플릿 복사 실패: $rel"; }
  done < "$LIST"
  rm -f "$LIST"
fi

echo "완료: $DEST ($MODE, $NAME, 고객사 $CUSTOMER, $FRAMEWORK)"
