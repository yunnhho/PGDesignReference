using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace APC.UI.Tools
{
    internal static class Program
    {
        private const string UsageText =
            "사용법:\n" +
            "  APC.UI.Tools api <dll> <out.md>\n" +
            "  APC.UI.Tools render <assembly> <type> <themeDir> <out.png> [--size 1920x1080] [--compare <원본.png>]\n" +
            "  APC.UI.Tools attach <csproj>\n";

        [STAThread]
        private static int Main(string[] args)
        {
            // Git Bash·에이전트는 UTF-8로 읽는다. 기본(CP949)이면 한글 오류 메시지가 깨져 원인을 알 수 없다.
            Console.OutputEncoding = new UTF8Encoding(false);
            if (args.Length == 0) return Usage();
            try
            {
                switch (args[0])
                {
                    case "api": return Api(args);
                    case "render": return RenderCmd(args);
                    case "attach": return Attach(args);
                    default: return Usage();
                }
            }
            catch (Exception ex)
            {
                // CLI 경계: 원인 메시지를 그대로 보여 주고 실패 코드로 끝낸다.
                Console.Error.WriteLine("오류: " + ex.Message);
                return 2;
            }
        }

        private static int Usage()
        {
            Console.Error.Write(UsageText);
            return 1;
        }

        private static int Api(string[] args)
        {
            if (args.Length != 3) return Usage();
            File.WriteAllText(args[2], ApiWriter.Write(args[1]), new UTF8Encoding(false));
            Console.WriteLine("api -> " + args[2]);
            return 0;
        }

        private static int Attach(string[] args)
        {
            if (args.Length != 2) return Usage();
            string patched = CsprojPatcher.Patch(File.ReadAllText(args[1]));   // 실패하면 예외 → 파일은 그대로
            File.WriteAllText(args[1], patched, new UTF8Encoding(true));
            Console.WriteLine("attach -> " + args[1]);
            return 0;
        }

        private static int RenderCmd(string[] args)
        {
            if (args.Length < 5) return Usage();
            var size = new Size(1920, 1080);
            string compare = null;
            for (int i = 5; i < args.Length; i++)
            {
                if (args[i] == "--size" && i + 1 < args.Length)
                {
                    var wh = args[++i].Split('x');
                    int w, h;
                    if (wh.Length != 2 || !int.TryParse(wh[0], out w) || !int.TryParse(wh[1], out h) || w <= 0 || h <= 0) return Usage();
                    size = new Size(w, h);
                }
                else if (args[i] == "--compare" && i + 1 < args.Length) compare = args[++i];
                else return Usage();
            }
            if (compare != null && !File.Exists(compare)) throw new RenderException("비교할 원본이 없습니다: " + compare);

            string output = Path.GetFullPath(args[4]);
            Directory.CreateDirectory(Path.GetDirectoryName(output));   // 없는 폴더면 GDI+가 원인 없는 오류를 낸다
            using (var bmp = Renderer.Render(args[1], args[2], args[3], size))
            {
                bmp.Save(output, ImageFormat.Png);
                Console.WriteLine("render -> " + output);
                if (compare == null) return 0;

                string cmpPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(output)), Path.GetFileNameWithoutExtension(output) + ".compare.png");
                using (var original = Image.FromFile(compare))
                using (var pair = Renderer.SideBySide(original, bmp))
                    pair.Save(cmpPath, ImageFormat.Png);
                Console.WriteLine("compare -> " + cmpPath);
            }
            return 0;
        }
    }
}
