using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCVShell
{
    public static class Scanner
    {
        public static List<Entities.File> GetAllFiles(string Directory)
        {
            var returner = new List<Entities.File>();

            foreach (var file in System.IO.Directory.GetFiles(Directory))
            {
                // Get the files in the local directory and add them
                returner.Add(new Entities.File()
                {
                    name = file,
                    isDir = false
                });
            }

            foreach (var directory in System.IO.Directory.GetDirectories(Directory))
            {
                // Get the directories and the files inside them and add them to the list
                returner.Add(new Entities.File()
                {
                    name = directory,
                    isDir = true
                });

                returner.AddRange(GetAllFiles(directory)); // Add the files from that directory to the same list
            }

            return returner;
        }
    }
}
