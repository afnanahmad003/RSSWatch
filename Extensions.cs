using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESSWATCH
{

    public static class UtilExtensions
    {
        public static bool EqualsCaseInsensitive(this string a, params string[] possibilities)
        {
            if (possibilities == null)
                return a == null;

            foreach (string b in possibilities)
            {
                string c = b;
                if (a == null)
                    a = string.Empty;
                if (c == null)
                    c = string.Empty;

                if (string.Compare(a, c, true) == 0)
                    return true;
            }

            return false;
        }

        public static List<string> SplitToStringList(string value)
        {
            if (string.IsNullOrEmpty(value))
                return new List<string>();

            return value
                .Split(',')
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToList();
        }

    }
}
