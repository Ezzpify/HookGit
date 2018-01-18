using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace HookAppDiscord
{
    class Utils
    {
        public static Random _random = new Random();

        public static string GetIncreasement(long newVal, long oldVal)
        {
            long val = newVal - oldVal;

            if (val > 0)
                return $"+{val}";
            else if (val < 0)
                return $"{val}";
            else
                return $"+-{val}";
        }

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

        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            value = Regex.Replace(value, @"\t|\n|\r", " ");
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + " ...";
        }
    }
}
