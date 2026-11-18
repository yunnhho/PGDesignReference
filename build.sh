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
# x86 앱(32비트 네이티브 DLL 사용) render용 32비트 도구. APC.UI.Tools는 프로젝트 참조가 없어 따로 빌드해도 다른 출력에 영향 없다.
"$MSBUILD" src/APC.UI.Tools/APC.UI.Tools.csproj -p:Configuration=Debug -p:PlatformTarget=x86 "-p:OutputPath=bin\Debug-x86\\" "-p:IntermediateOutputPath=obj\x86\Debug\\" -v:q -nologo -p:PreferredUILang=en-US > /dev/null 2>&1 || { echo "BUILD FAILED (APC.UI.Tools x86)"; exit 1; }
./src/APC.UI.Tools/bin/Debug/APC.UI.Tools.exe api src/APC.UI/bin/Debug/APC.UI.dll docs/api.md || { echo "API DOC FAILED"; exit 1; }
[ -n "$1" ] || tools/smoke.sh || exit 1   # 솔루션 전체 빌드일 때만
./src/APC.UI.Checks/bin/Debug/APC.UI.Checks.exe
