using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace APC.UI.Tools
{
    /// <summary>대상 어셈블리를 경로로 읽고, 그 의존 어셈블리를 같은 폴더에서 찾게 한다.</summary>
    internal static class AssemblyLoader
    {
        private static readonly List<string> ProbeDirs = new List<string>();
        private static bool _hooked;

        public static Assembly Load(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("파일이 없습니다: " + path, path);
            string full = Path.GetFullPath(path);
            string dir = Path.GetDirectoryName(full);
            if (!ProbeDirs.Contains(dir)) ProbeDirs.Add(dir);
            if (!_hooked)
            {
                AppDomain.CurrentDomain.AssemblyResolve += Resolve;
                _hooked = true;
            }
            return Assembly.LoadFrom(full);
        }

        private static Assembly Resolve(object sender, ResolveEventArgs e)
        {
            string name = new AssemblyName(e.Name).Name;
            foreach (var dir in ProbeDirs)
            {
                foreach (var ext in new[] { ".dll", ".exe" })
                {
                    string p = Path.Combine(dir, name + ext);
                    if (File.Exists(p)) return Assembly.LoadFrom(p);
                }
            }
            return null;
        }
    }
}
