using System;
using System.IO;
using System.Text;

namespace APC.UI.Tools
{
    internal static class Program
    {
        private const string UsageText =
            "사용법:\n" +
            "  APC.UI.Tools api <dll> <out.md>\n";

        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length == 0) return Usage();
            try
            {
                switch (args[0])
                {
                    case "api": return Api(args);
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
    }
}
