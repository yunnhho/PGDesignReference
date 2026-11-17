using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace APC.UI.Tools
{
    /// <summary>기존 .NET Framework 4.x WinForms csproj(구형 형식)에 APC.UI 참조와 테마 출력 복사를 넣는다.
    /// 원래 줄·들여쓰기·인코딩은 그대로 두고 필요한 항목만 끼워 넣는다.</summary>
    public static class CsprojPatcher
    {
        private static readonly XNamespace Ns = "http://schemas.microsoft.com/developer/msbuild/2003";
        private const string HintPath = @"lib\APC.UI.dll";
        private const string ThemeGlob = @"themes\**\*";
        private const string IndentUnit = "  ";

        /// <summary>연결할 수 있는 프로젝트인지 검사만 한다.</summary>
        /// <exception cref="InvalidOperationException">SDK 형식, 4.x 아님, WinForms 아님.</exception>
        public static void Validate(string csprojXml)
        {
            Validate(XDocument.Parse(csprojXml).Root);
        }

        /// <summary>수정된 XML을 돌려준다. 여러 번 적용해도 결과가 같다.</summary>
        /// <exception cref="InvalidOperationException">SDK 형식, 4.x 아님, WinForms 아님.</exception>
        public static string Patch(string csprojXml)
        {
            // XML 파서는 줄바꿈을 \n으로 정규화한다. 안에서는 \n으로만 다루고, 원본이 CRLF면 마지막에 되돌린다.
            var doc = XDocument.Parse(csprojXml, LoadOptions.PreserveWhitespace);
            Validate(doc.Root);
            const string nl = "\n";

            SetReference(doc.Root, nl);
            AddThemeCopy(doc.Root, nl);

            var sb = new StringBuilder();
            if (doc.Declaration != null) sb.Append(doc.Declaration).Append(nl);
            sb.Append(doc.Root.ToString(SaveOptions.DisableFormatting));
            if (csprojXml.EndsWith("\n", StringComparison.Ordinal)) sb.Append(nl);
            string result = sb.ToString().Replace("\r\n", "\n");
            return csprojXml.Contains("\r\n") ? result.Replace("\n", "\r\n") : result;
        }

        /// <summary>파일을 제자리에서 고친다. 원래 인코딩(BOM 유무 포함)을 유지한다. 실패하면 파일은 그대로다.</summary>
        public static void PatchFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            Encoding enc;
            string text;
            using (var reader = new StreamReader(new MemoryStream(bytes), new UTF8Encoding(false), true))
            {
                text = reader.ReadToEnd();
                enc = reader.CurrentEncoding;
            }
            bool hasBom = bytes.Length >= 2 && ((bytes[0] == 0xEF && bytes[1] == 0xBB) || bytes[0] == 0xFF || bytes[0] == 0xFE);
            if (enc is UTF8Encoding) enc = new UTF8Encoding(hasBom);
            File.WriteAllText(path, Patch(text), enc);
        }

        private static void Validate(XElement root)
        {
            if (root.Attribute("Sdk") != null)
                throw new InvalidOperationException("SDK 형식 csproj는 지원하지 않습니다. .NET Framework 4.x 구형 csproj만 연결할 수 있습니다.");

            string fw = Values(root, "TargetFrameworkVersion").FirstOrDefault();
            if (fw == null || !fw.StartsWith("v4.", StringComparison.Ordinal))
                throw new InvalidOperationException("대상 프레임워크가 .NET Framework 4.x가 아닙니다: " + (fw ?? "(없음)"));

            bool winExe = Values(root, "OutputType").Any(v => v == "WinExe");
            bool forms = root.Descendants(Ns + "Reference").Any(r => IncludeName(r) == "System.Windows.Forms");
            if (!winExe && !forms) throw new InvalidOperationException("WinForms 프로젝트가 아닙니다(OutputType WinExe 또는 System.Windows.Forms 참조 없음).");
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

        private static void SetReference(XElement root, string nl)
        {
            // 소스 프로젝트로 이미 연결돼 있으면 dll 참조를 겹쳐 넣지 않는다.
            bool viaProject = root.Descendants(Ns + "ProjectReference")
                .Any(p => Path.GetFileNameWithoutExtension(((string)p.Attribute("Include") ?? "").Replace('\\', '/').Split('/').Last()) == "APC.UI");
            if (viaProject) return;

            var existing = root.Descendants(Ns + "Reference").Where(r => IncludeName(r) == "APC.UI").ToList();
            foreach (var r in existing)   // 조건부 ItemGroup에 여러 개 있어도 모두 lib로
            {
                r.SetAttributeValue("Include", "APC.UI");   // 옛 버전 고정 문자열 제거: 교체한 dll을 그대로 쓴다
                var hint = r.Element(Ns + "HintPath");
                if (hint != null) hint.Value = HintPath;
                else AddIndented(r, new XElement(Ns + "HintPath", HintPath), nl);
            }
            if (existing.Count > 0) return;

            var reference = new XElement(Ns + "Reference", new XAttribute("Include", "APC.UI"));
            var group = ItemGroupWith(root, "Reference", nl);
            AddIndented(group, reference, nl);
            AddIndented(reference, new XElement(Ns + "HintPath", HintPath), nl);
        }

        private static void AddThemeCopy(XElement root, string nl)
        {
            bool present = root.Descendants().Any(e => e.Name.Namespace == Ns && (string)e.Attribute("Include") == ThemeGlob);
            if (present) return;
            var none = new XElement(Ns + "None", new XAttribute("Include", ThemeGlob));
            AddIndented(ItemGroupWith(root, "None", nl), none, nl);
            AddIndented(none, new XElement(Ns + "CopyToOutputDirectory", "PreserveNewest"), nl);
        }

        // 같은 종류 항목이 있는 (조건 없는) ItemGroup에 넣는다. 없으면 새 ItemGroup을 마지막 Import 앞에 만든다.
        private static XElement ItemGroupWith(XElement root, string itemName, string nl)
        {
            var group = root.Elements(Ns + "ItemGroup")
                .FirstOrDefault(g => g.Attribute("Condition") == null && g.Elements(Ns + itemName).Any());
            if (group != null) return group;

            group = new XElement(Ns + "ItemGroup");
            var import = root.Elements(Ns + "Import").LastOrDefault();
            if (import != null) import.AddBeforeSelf(group, new XText(nl + Indent(import)));
            else AddIndented(root, group, nl);
            return group;
        }

        /// <summary>child를 parent의 마지막 자식으로, 형제와 같은 들여쓰기로 넣는다.</summary>
        private static void AddIndented(XElement parent, XElement child, string nl)
        {
            string parentIndent = Indent(parent);
            var last = parent.Elements().LastOrDefault();
            string childIndent = last != null ? Indent(last) : parentIndent + IndentUnit;

            if (last != null) last.AddAfterSelf(new XText(nl + childIndent), child);
            else
            {
                parent.RemoveNodes();   // 빈 요소의 공백 정리
                parent.Add(new XText(nl + childIndent), child, new XText(nl + parentIndent));
            }
        }

        // 요소 바로 앞 공백 텍스트의 마지막 줄 = 그 요소의 들여쓰기.
        private static string Indent(XElement e)
        {
            var text = e.PreviousNode as XText;
            if (text == null) return "";
            string s = text.Value;
            int i = s.LastIndexOf('\n');
            return i < 0 ? "" : s.Substring(i + 1);
        }
    }
}
