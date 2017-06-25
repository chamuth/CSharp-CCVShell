using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace CCVShell
{

    class Program
    {
        private static string _currentDirectory = "";
        public static string _connectedServer = "";

        public static string currentDirectory
        {
            get
            {
                return _currentDirectory;
            }
            set
            {
                _currentDirectory = value.Replace("\\", "/").Replace("//", "/");
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine();
            if (args[0].ToLower() == "add") // CCVSHELL ADD [NAME] [ENDPOINT]
            {
                if (args.Length == 3)
                {
                    AddServer(args); // Add the server to the server folder
                }
                else
                {
                    Console.WriteLine("Usage: CCVShell add [NAME] [HTTP ENDPOINT / SERVER URL]");
                    Console.WriteLine("(Adds a new server to the local configuration)");
                }
            }
            else if (args[0].ToLower() == "update")
            {
                if (args.Length == 2)
                {
                    var servername = args[1];
                    var filename = "Server\\" + servername + ".json";

                    if (File.Exists(filename))
                    {
                        var details = JsonConvert.DeserializeObject<Entities.Server>(File.ReadAllText(filename));

                        Console.Write("Server Name "); Alert.AlertUser(Alert.AlertType.Warning, "(" + details.Name + ") : ", false);
                        var name = Console.ReadLine();
                        Console.Write("Server Endpoint "); Alert.AlertUser(Alert.AlertType.Warning, "(" + details.Endpoint + ") : ", false);
                        var endpoint = Console.ReadLine();
                        Console.Write("Server Local Directory "); Alert.AlertUser(Alert.AlertType.Warning, "(" + details.LocalDirectory + ") : ", false);
                        var ldir = Console.ReadLine();
                        Console.Write("Server Password "); Alert.AlertUser(Alert.AlertType.Warning, "(" + details.Password + ") : ", false);
                        var password = Console.ReadLine();

                        if (name != "") details.Name = name;
                        if (endpoint != "") details.Endpoint = name;
                        if (ldir != "") details.LocalDirectory = name;
                        if (password != "") details.Password = name;

                        Console.WriteLine();
                        Console.WriteLine("Server Configuration updated!");
                        Console.WriteLine();

                        File.WriteAllText(filename, JsonConvert.SerializeObject(details));

                        if (name.Replace(" ", "") != "")
                            File.Move(filename, "Server\\" + name + ".json");
                        
                    }else
                    {
                        Console.WriteLine("The server name \"" + args[1] + "\" is not configured, use CCVSHELL CONNECT [SERVER]");
                    }

                }
                else
                {
                    Console.WriteLine("Usage: CCVShell update [NAME]");
                    Console.WriteLine("(Adds a new server to the local configuration)");
                }
            }
            else if (args[0].ToLower() == "list")
            {
                if (args.Length == 1)
                {
                    foreach (var file in Directory.GetFiles("Server"))
                    {
                        var details = JsonConvert.DeserializeObject<Entities.Server>(File.ReadAllText(file));

                        Alert.AlertUser(Alert.AlertType.Warning, file.Replace(".json", "").Replace("Server\\", ""), false);
                        Alert.AlertUser(Alert.AlertType.Normal, " (" + details.Endpoint + ")");
                    }
                }
                else if (args.Length == 2)
                {
                    var filename = "Server\\" + args[1] + ".json";

                    if (File.Exists(filename))
                    {
                        var details = JsonConvert.DeserializeObject<Entities.Server>(File.ReadAllText(filename));

                        Alert.AlertUser(Alert.AlertType.Warning, ("Name: "), false); Alert.AlertUser(Alert.AlertType.Normal, details.Name);
                        Alert.AlertUser(Alert.AlertType.Warning, ("Endpoint: "), false); Alert.AlertUser(Alert.AlertType.Normal, details.Endpoint);
                        Alert.AlertUser(Alert.AlertType.Warning, ("Local Directory: "), false); Alert.AlertUser(Alert.AlertType.Normal, details.LocalDirectory);
                        Alert.AlertUser(Alert.AlertType.Warning, ("Password: "), false); Alert.AlertUser(Alert.AlertType.Normal, details.Password);
                    }
                    else
                    {
                        Console.WriteLine("The server name \"" + args[1] + "\" is not configured, use CCVSHELL CONNECT [SERVER]");
                    }
                }
            }
            else if (args[0].ToLower() == "connect") // CCVSHELL CONNECT [SERVER]
            {
                if (args.Length == 2)
                {
                    //Get the server information
                    var server_name = args[1];
                    var config_file = "Server/" + server_name + ".json";

                    if (File.Exists(config_file))
                    {
                        var configuration = JsonConvert.DeserializeObject<Entities.Server>(File.ReadAllText(config_file));
                        _connectedServer = server_name;

                        //The user is connecting to a server

                        cmd(configuration.Endpoint, "serverinfo", configuration.Password); // Pull the server information
                        currentDirectory = getPWD(configuration.Endpoint, configuration.Password);

                        do
                        {
                            //Forever
                            Console.WriteLine();
                            Alert.AlertUser(Alert.AlertType.Success, Environment.UserName + "@" + configuration.Name, false);
                            Alert.AlertUser(Alert.AlertType.Information, " " + currentDirectory + " $ ", false);

                            var command = Console.ReadLine();

                            if (command != null)
                            {
                                if (command != "exit")
                                {
                                    cmd(configuration.Endpoint, command.ToLower(), configuration.Password); //Execute the command
                                }
                                else
                                {
                                    Alert.AlertUser(Alert.AlertType.Warning, "Logging out...");
                                    break;
                                }
                            }
                            else
                            {
                                Console.WriteLine();
                                break;
                            }
                        } while (true);
                    }
                    else
                    {
                        Alert.AlertUser(Alert.AlertType.Warning, "Could not find a server named \"" + server_name.ToString() + "\"!");
                        Console.WriteLine("Please make sure you have added the server to the configurations before connecting to it.");
                        Console.WriteLine("Use: CCVShell add [NAME] [HTTP ENDPOINT / SERVER URL]");
                    }
                }
                else
                {
                    Console.WriteLine("Usage: CCVShell connect [SERVER NAME]");
                    Console.WriteLine("Connects to the specified server");
                }
            }
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static Entities.Server getServerConfiguration()
        {
            var config_file = "Server/" + _connectedServer + ".json";
            var configuration = JsonConvert.DeserializeObject<Entities.Server>(File.ReadAllText(config_file));
            return configuration;
        }

        public static bool cmd(string endpoint, string command, string password)
        {
            var client = new RestClient(endpoint);
            var request_url = "CCVShell/Handle.php?p=" + password + "&cmd=" + command.ToString();

            if (currentDirectory != "")
                request_url += "&cd=" + currentDirectory;

            var request = new RestRequest(request_url, Method.GET);

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    var newcom = command.Trim(); // Trim it from left and right
                    newcom = newcom.ultraTrim();

                    var seperations = newcom.Split(' ');
                    seperations[0] = seperations[0].ToLower();


                    if (seperations[0] == "pwd")
                    {
                        var result = JsonConvert.DeserializeObject<Entities.pwd>(response.Content);
                        Alert.AlertUser(Alert.AlertType.Normal, result.cd);
                    }
                    else if (seperations[0] == "edit")
                    {
                        if (seperations.Length > 1)
                        {
                            var remote_filename = seperations[1];

                            if (!Directory.Exists("temp\\"))
                                Directory.CreateDirectory("temp\\");

                            var filename = ("temp\\" + remote_filename + ".txt");
                            File.Create(filename).Close();

                            File.WriteAllText(filename, response.Content);

                            Alert.AlertUser(Alert.AlertType.Normal, "Received bytes from \"" + remote_filename + "\"");

                            var p = new Process();
                            p.StartInfo.FileName = filename;
                            p.EnableRaisingEvents = true;
                            Alert.AlertUser(Alert.AlertType.Normal, "\"" + remote_filename + "\" started editing process...");
                            p.Start();

                            do { } while (!p.HasExited);

                            var content = File.ReadAllText(filename);
                            var cl = new RestClient(endpoint);
                            var uri = "CCVShell/Handle.php?p=" + password + "&cmd=edit";

                            var req = new RestRequest(uri, Method.POST);
                            req.AddParameter("content", content);
                            req.AddParameter("filename", remote_filename);

                            var resp = cl.Execute(req);

                            Alert.AlertUser(Alert.AlertType.Information, resp.Content);                            
                        }
                    }
                    else if(seperations[0] == "zip")
                    {
                        if (seperations.Length == 3 || seperations.Length == 4)
                            Console.WriteLine(response.Content);
                    }
                    else if (seperations[0] == "unzip")
                    {
                        if (seperations.Length == 3)
                            Console.WriteLine(response.Content);
                    }
                    else if (seperations[0] == "search")
                    {
                        var result = JsonConvert.DeserializeObject<Entities.Filenames>(response.Content);

                        if (result.files.Length == 0)
                        {
                            //Found 0 files
                            Alert.AlertUser(Alert.AlertType.Information, string.Format("Found {0} files", result.files.Length), false);
                            Console.Write(string.Format(" for the search term "));
                            Alert.AlertUser(Alert.AlertType.Information, string.Format("\"{0}\"", seperations[1]));
                        }
                        else
                        {
                            Alert.AlertUser(Alert.AlertType.Information, string.Format("Found {0} files", result.files.Length), false);
                            Console.Write(string.Format(" for the search term "));

                            var searchterm = "*";
                            if (seperations.Length > 1)
                            {
                                searchterm = seperations[1];
                                if (searchterm.StartsWith("*.")) searchterm = "*";
                            }

                            Alert.AlertUser(Alert.AlertType.Information, string.Format("\"{0}\"", searchterm));
                            Console.WriteLine();

                            foreach (var file in result.files)
                            {
                                if (seperations.Contains("-i"))
                                {
                                    // User requests more information about the searched files
                                    var t = UnixTimeStampToDateTime(double.Parse(file.mtime));

                                    if (file.isDir)
                                    {
                                        Alert.AlertUser(Alert.AlertType.Warning, file.permission + "\t" +
                                            t.ToShortDateString() + " " + t.ToShortTimeString() + "\t" +
                                            file.length + "\t" + "\t" +
                                            file.name);
                                    }
                                    else
                                    {
                                        Console.WriteLine(file.permission + "\t" +
                                            t.ToShortDateString() + " " + t.ToShortTimeString() + "\t" +
                                            file.length + "\t" + "\t" +
                                            file.name);
                                    }
                                }
                                else
                                {
                                    if (file.isDir)
                                        Alert.AlertUser(Alert.AlertType.Warning, file.name);
                                    else
                                        Alert.AlertUser(Alert.AlertType.Normal, file.name);
                                }

                            }

                            if (seperations.Contains("-copy"))
                            {
                                var contenter = "";
                                Array.ForEach(result.files, (x) =>
                                {
                                    contenter += x.name;
                                });

                                System.Windows.Forms.Clipboard.SetText(contenter);
                            }
                        }
                    }
                    else if (seperations[0] == "ls")
                    {
                        if (seperations.Length == 2)
                        {
                            if (seperations.Contains("-al"))
                            {
                                //Get files with information
                                var result = JsonConvert.DeserializeObject<Entities.ls_al>(response.Content);

                                foreach (var file in result.files)
                                {
                                    var t = UnixTimeStampToDateTime(double.Parse(file.mtime));

                                    if (file.isDir)
                                    {
                                        Alert.AlertUser(Alert.AlertType.Warning, file.permission + "\t" +
                                            t.ToShortDateString() + " " + t.ToShortTimeString() + "\t" +
                                            file.length + "\t" + "\t" +
                                            file.name);
                                    }
                                    else
                                    {
                                        Console.WriteLine(file.permission + "\t" +
                                            t.ToShortDateString() + " " + t.ToShortTimeString() + "\t" +
                                            file.length + "\t" + "\t" +
                                            file.name);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var result = JsonConvert.DeserializeObject<Entities.ls>(response.Content);

                            foreach (var file in result.files)
                            {
                                if (file.isDir)
                                    Alert.AlertUser(Alert.AlertType.Warning, file.name);
                                else
                                    Alert.AlertUser(Alert.AlertType.Normal, file.name);
                            }
                        }
                    }
                    else if (seperations[0] == "serverinfo")
                    {
                        Alert.AlertUser(Alert.AlertType.Normal, response.Content);
                    }
                    else if (seperations[0] == "cd")
                    {
                        var result = JsonConvert.DeserializeObject<Entities.pwd>(response.Content);
                        currentDirectory = result.cd; // Setup the current directory
                    }
                    else if (seperations[0] == "cd..")
                    {
                        var result = JsonConvert.DeserializeObject<Entities.pwd>(response.Content);
                        currentDirectory = result.cd; // Setup the current directory
                    }
                    else if (seperations[0] == "touch" || seperations[0] == "mkdir" || seperations[0] == "rm" || seperations[0] == "ren")
                    {
                        if (response.Content != "")
                            Alert.AlertUser(Alert.AlertType.Normal, response.Content);
                    }
                    else if (seperations[0] == "file")
                    {
                        Alert.AlertUser(Alert.AlertType.Normal, response.Content);

                        if (seperations[seperations.Length - 1] == "-copy")
                        {
                            System.Windows.Forms.Clipboard.SetText(response.Content);
                        }
                    }
                    else if (seperations[0] == "cpm")
                    {
                        if (seperations.Length > 1)
                        {
                            if (seperations[1] == "-v")
                            {
                                //Checking the version
                                Alert.AlertUser(Alert.AlertType.Normal, response.Content);
                            }
                            else if (seperations[1] == "init" || seperations[1] == "install")
                            {
                                Alert.AlertUser(Alert.AlertType.Normal, response.Content);
                            }
                        }
                        else
                        {
                            Alert.AlertUser(Alert.AlertType.Normal, response.Content);
                        }
                    }
                    else if (seperations[0] == "sync")
                    {
                        if (seperations[1] == "down") // Download / copy the files to the local directory
                        {
                            var result = JsonConvert.DeserializeObject<Entities.Filenames>(response.Content);

                            foreach (var file in result.files)
                            {
                                Alert.AlertUser(Alert.AlertType.Information, "Downloading ", false);
                                Console.WriteLine(file.name + "...");

                                var localdir = getServerConfiguration().LocalDirectory + "\\";

                                if (file.isDir == true)
                                {
                                    var savefile = localdir + file.name.Replace(endpoint, "");
                                    savefile = savefile.Replace("C:/xampp/htdocs/CCVShell/Server/", ""); // FOR TESTING

                                    Directory.CreateDirectory(savefile);
                                }
                                else
                                {
                                    using (var cs = new WebClient())
                                    {
                                        var savefile = (localdir + file.name.Replace(endpoint, ""));
                                        savefile = savefile.Replace("C:/xampp/htdocs/CCVShell/Server/", ""); // FOR TESTING
                                        var downloc = endpoint + "/CCVShell/Handle.php?p=" + password + "&cmd=sync file " + file.name;

                                        cs.Headers["Accept-Encoding"] = "application/octet-stream";

                                        try
                                        {
                                            cs.DownloadFile(downloc, savefile);

                                            Alert.AlertUser(Alert.AlertType.Information, "Saved file as ", false);
                                            Console.WriteLine(savefile);
                                        }
                                        catch (Exception ex)
                                        {
                                            Alert.AlertUser(Alert.AlertType.Error, "Download failed: ", false);
                                            Console.WriteLine(ex.Message);
                                        }

                                        Console.WriteLine();
                                    }
                                }
                            }
                        }
                        else if (seperations[1] == "up") // Upload the files back to the server
                        {
                            var localdir = getServerConfiguration().LocalDirectory + "\\";
                            var list = Scanner.GetAllFiles(localdir);

                            foreach (var listitem in list)
                            {
                                if (listitem.isDir == false)
                                {

                                    Alert.AlertUser(Alert.AlertType.Information, "Uploading ", false);
                                    Console.WriteLine(listitem.name + "...");

                                    var cl = new RestClient(endpoint);
                                    var uri = "CCVShell/Handle.php?p=" + password + "&cmd=sync upload&ld=" + listitem.name.Replace(localdir, "");

                                    var req = new RestRequest(uri, Method.POST);

                                    req.AddFile("fileData", listitem.name, "application/octet-stream");
                                    req.AlwaysMultipartFormData = true;

                                    var resp = cl.Execute(req);

                                    if (resp.StatusCode == HttpStatusCode.OK)
                                    {
                                        Console.WriteLine(resp.Content); // content of the response
                                    }
                                    else
                                    {
                                        Alert.AlertUser(Alert.AlertType.Error, "Failed uploading!");
                                    }
                                }
                            }
                        }
                    }
                    else if (seperations[0] == "http")
                    {
                        Console.WriteLine(); // Add a line ending

                        foreach (var par in response.Headers)
                        {
                            Alert.AlertUser(Alert.AlertType.Information, par.Name.ToString() + ":" + par.Value.ToString());
                        }

                        Alert.AlertUser(Alert.AlertType.Normal, response.Content);

                        if (seperations[seperations.Length - 1] == "-copy")
                        {
                            System.Windows.Forms.Clipboard.SetText(response.Content); // Copy the content of the html page to the clipboard 
                        }
                    }

                }
                catch (IOException) { Console.WriteLine(response.Content); }

                return true;
            }
            else
            {
                return false;
            }
        }

        public static string getPWD(string endpoint, string password)
        {
            var client = new RestClient(endpoint);
            var request_url = "CCVShell/Handle.php?p=" + password + "&cmd=pwd";
            var request = new RestRequest(request_url, Method.GET);

            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = JsonConvert.DeserializeObject<Entities.pwd>(response.Content);

                return result.cd;
            }
            else
            {
                return "";
            }
        }

        public static void AddServer(string[] args)
        {
            string name = args[1];
            string endpoint = args[2];

            var v = "Server/" + name.ToString() + ".json";

            if (File.Exists(v))
            {
                Console.WriteLine("You already have \"" + name.ToString() + "\" configured");
            }
            else
            {
                Console.Write("CCVShell Password (config.json): ");
                string password = Console.ReadLine();

                Console.Write("Local Directory (for Syncing): ");
                string local = Console.ReadLine();

                Console.WriteLine("Creating the local directory...");

                //Create the local directory if does not exists
                if (!Directory.Exists(local))
                    Directory.CreateDirectory(local);

                Console.WriteLine("Connecting to server \"" + name + "\" at " + endpoint + "...");

                if (verifyServer(endpoint, password))
                {
                    if (!Directory.Exists("Server"))
                        Directory.CreateDirectory("Server"); // Create the Directory

                    File.Create(v).Close(); // Create the configuration file

                    var config = new Entities.Server(name, endpoint, password, local);

                    File.WriteAllText(v, JsonConvert.SerializeObject(config)); // Save the configuration

                    Alert.AlertUser(Alert.AlertType.Success, "Connected to the server...");
                    Console.WriteLine("Saved the Server Configuration!");
                }
                else
                {
                    Console.WriteLine();
                    Alert.AlertUser(Alert.AlertType.Error, "Invalid credentials (403), Please retry");
                }
            }

        }

        public static bool verifyServer(string url, string password)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest("CCVShell/Handle.php?p=" + password, Method.GET);
                var response = client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            { return false; }
        }
    }
}

