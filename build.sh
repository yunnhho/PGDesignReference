#!/bin/sh
# Build solution (or one project) and run self-checks. Usage: ./build.sh [project]
cd "$(dirname "$0")" || exit 1
. tools/env.sh
LOG="$(mktemp)"
"$MSBUILD" "${1:-APC.UI.sln}" -p:Configuration=Debug -v:m -nologo -p:PreferredUILang=en-US > "$LOG" 2>&1
CODE=$?
grep -E "error|warning|->" "$LOG"
rm -f "$LOG"
[ $CODE -eq 0 ] || { echo "BUILD FAILED ($CODE)"; exit $CODE; }
./src/APC.UI.Tools/bin/Debug/APC.UI.Tools.exe api src/APC.UI/bin/Debug/APC.UI.dll docs/api.md || { echo "API DOC FAILED"; exit 1; }
[ -n "$1" ] || tools/smoke.sh || exit 1   # 솔루션 전체 빌드일 때만
./src/APC.UI.Checks/bin/Debug/APC.UI.Checks.exe
