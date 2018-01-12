using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace HookAppDiscord
{
    class Utils
    {
        public static Random _random = new Random();

        public static Random GetRandom()
        {
            return _random;
        }

        public static int GetRandomInt(int a, int b)
        {
            return _random.Next(a, b);
        }

        public static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion;
        }

        public static string GetStringInBetween(string source, string first, string second)
        {
            var start = source.IndexOf(first) + first.Length;
            return source.Substring(start, source.IndexOf(second) - start);
        }

        public static string FixJsonString(string json)
        {
            json = json.TrimStart('"');
            json = json.TrimEnd('"');
            return json.Replace(@"\", "");
        }
    }
}
