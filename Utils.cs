using System.Collections.Generic;
using System.IO;

namespace LogsNotIncluded
{
    public static class Utils
    {
        public static string GetLogDirectory()
        {
            return Path.Combine(Util.RootFolder(), "logs/");
        }

        internal static void MakeLogDirectory()
        {
            if (!Directory.Exists(GetLogDirectory()))
            {
                Directory.CreateDirectory(GetLogDirectory());
            }
        }

        public static IEnumerable<string> ToString<T>(IEnumerable<T> objects)
        {
            foreach (T obj in objects)
            {
                yield return obj.ToString();
            }
            yield break;
        }

        public static IEnumerable<string> ToStringArray<T>(this List<T> list) {
            return ToString(list);
        }
    }
}
