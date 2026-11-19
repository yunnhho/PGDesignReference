cd /c/00P_apc-field-ui/skill-test/B-cellleak/project
T=/c/00P_apc-field-ui/src/APC.UI.Tools/bin/Debug/APC.UI.Tools.exe
for f in MainForm LotInputDialog AdminPasswordDialog; do echo "== $f"; $T render "$(cygpath -w bin/Debug/CellLeakTester.exe)" $f "$(cygpath -w bin/Debug/themes/default)" "$(cygpath -w ../out/$f.png)"; done
