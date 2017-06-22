using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCVShell.Entities
{
    public class pwd
    {
        public string cd { get; set; }
    }

    public class ls
    {
        public File[] files { get; set; }
    }

    public class ls_al
    {
        public File[] files { get; set; }
    }

    public class File
    {
        public string name { get; set; }
        public string permission { get; set; }
        public string mtime { get; set; }
        public string length { get; set; }
        public bool isDir { get; set; }
    }

    public class Filenames
    {
        public File[] files { get; set; }
    }
}
