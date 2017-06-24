using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCVShell
{
    public static class UltraTrim
    {
        public static bool changed(string oldval, string newval)
        {
            return !(oldval == newval);
        }

        public static string trimOnce(string input)
        {
            return input.Replace("  ", " ");
        }

        public static string ultraTrim(this string input)
        {
            var prev = "";
            var currinput = input;

            do
            {
                prev = currinput;
                currinput = trimOnce(currinput);

            } while (changed(prev, currinput));

            return currinput;
        }
    }
}
