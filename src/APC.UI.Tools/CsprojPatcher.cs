using System;
using System.Linq;
using System.Xml.Linq;

namespace APC.UI.Tools
{
    /// <summary>기존 .NET Framework 4.x WinForms csproj(구형 형식)에 APC.UI 참조와 테마 출력 복사를 넣는다.</summary>
    public static class CsprojPatcher
    {
        private static readonly XNamespace Ns = "http://schemas.microsoft.com/developer/msbuild/2003";
        private const string HintPath = @"lib\APC.UI.dll";
        private const string ThemeGlob = @"themes\**\*";

        /// <summary>수정된 XML을 돌려준다. 여러 번 적용해도 결과가 같다.</summary>
        /// <exception cref="InvalidOperationException">SDK 형식, 4.x 아님, WinForms 아님.</exception>
        public static string Patch(string csprojXml)
        {
            var doc = XDocument.Parse(csprojXml);
            var root = doc.Root;
            if (root.Attribute("Sdk") != null)
                throw new InvalidOperationException("SDK 형식 csproj는 지원하지 않습니다. .NET Framework 4.x 구형 csproj만 연결할 수 있습니다.");

            string fw = Values(root, "TargetFrameworkVersion").FirstOrDefault();
            if (fw == null || !fw.StartsWith("v4.", StringComparison.Ordinal))
                throw new InvalidOperationException("대상 프레임워크가 .NET Framework 4.x가 아닙니다: " + (fw ?? "(없음)"));

            bool winExe = Values(root, "OutputType").Any(v => v == "WinExe");
            bool forms = root.Descendants(Ns + "Reference").Any(r => IncludeName(r) == "System.Windows.Forms");
            if (!winExe && !forms) throw new InvalidOperationException("WinForms 프로젝트가 아닙니다(OutputType WinExe 또는 System.Windows.Forms 참조 없음).");

            SetReference(root);
            AddThemeCopy(root);
            return doc.Declaration + Environment.NewLine + doc.Root;
        }

        private static string[] Values(XElement root, string name)
        {
            return root.Descendants(Ns + name).Select(e => e.Value.Trim()).ToArray();
        }

        private static string IncludeName(XElement reference)
        {
            var include = (string)reference.Attribute("Include") ?? "";
            int comma = include.IndexOf(',');
            return (comma < 0 ? include : include.Substring(0, comma)).Trim();
        }

        private static void SetReference(XElement root)
        {
            var existing = root.Descendants(Ns + "Reference").FirstOrDefault(r => IncludeName(r) == "APC.UI");
            if (existing != null)
            {
                existing.SetAttributeValue("Include", "APC.UI");   // 옛 버전 고정 문자열 제거: 교체한 dll을 그대로 쓴다
                existing.SetElementValue(Ns + "HintPath", HintPath);
                return;
            }
            ItemGroupWith(root, "Reference").Add(new XElement(Ns + "Reference",
                new XAttribute("Include", "APC.UI"),
                new XElement(Ns + "HintPath", HintPath)));
        }

        private static void AddThemeCopy(XElement root)
        {
            bool present = root.Descendants().Any(e => e.Name.Namespace == Ns && (string)e.Attribute("Include") == ThemeGlob);
            if (present) return;
            ItemGroupWith(root, "None").Add(new XElement(Ns + "None",
                new XAttribute("Include", ThemeGlob),
                new XElement(Ns + "CopyToOutputDirectory", "PreserveNewest")));
        }

        // 같은 종류 항목이 있는 ItemGroup에 넣는다. 없으면 새 ItemGroup을 Import 앞에 만든다.
        private static XElement ItemGroupWith(XElement root, string itemName)
        {
            var group = root.Elements(Ns + "ItemGroup").FirstOrDefault(g => g.Elements(Ns + itemName).Any());
            if (group != null) return group;
            group = new XElement(Ns + "ItemGroup");
            var import = root.Elements(Ns + "Import").LastOrDefault();
            if (import != null) import.AddBeforeSelf(group);
            else root.Add(group);
            return group;
        }
    }
}
