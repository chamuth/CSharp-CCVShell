using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCVShell.Entities
{
    /// <summary>
    /// Represents a single server in the configurations
    /// </summary>
    public class Server
    {
        public Server(string Name, string Endpoint, string Password, string LocalDirectory)
        {
            this.Name = Name;
            this.Endpoint = Endpoint;
            this.Password = Password;
            this.LocalDirectory = LocalDirectory;
        }

        public string Name { get; set; }
        public string Endpoint { get; set; }
        public string Password { get; set; }
        public string LocalDirectory { get; set; }
    }

}
