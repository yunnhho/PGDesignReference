# 공용 환경: 다른 스크립트가 source 한다.
# MSBUILD: 환경 변수로 지정했으면 그대로, 아니면 vswhere로 설치된 최신 Visual Studio/Build Tools에서 찾는다.
if [ -z "$MSBUILD" ]; then
  VSWHERE="/c/Program Files (x86)/Microsoft Visual Studio/Installer/vswhere.exe"
  [ -x "$VSWHERE" ] && MSBUILD="$("$VSWHERE" -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | head -1 | tr -d '\r')"
  [ -n "$MSBUILD" ] && MSBUILD="$(cygpath -u "$MSBUILD")"
fi
[ -n "$MSBUILD" ] && [ -x "$MSBUILD" ] || { echo "MSBuild.exe를 찾을 수 없습니다. Visual Studio(또는 Build Tools)를 설치하거나 MSBUILD 환경 변수에 경로를 지정하세요." >&2; exit 1; }
