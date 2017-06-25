using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCVShell
{
    public static class TempKeyGen
    {
        public static string Generate()
        {
            var chars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };

            var generatedKey = "";

            for (int i = 0; i < 16; i++)
            {
                generatedKey += chars[new Random(i).Next(0, chars.Length)];
            }

            return generatedKey;
        }
    }
}
