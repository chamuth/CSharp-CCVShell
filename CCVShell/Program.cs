﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using System.IO;
using System.Net;

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
                                    cmd(configuration.Endpoint, command, configuration.Password); //Execute the command
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

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    var seperations = command.Split(' ');


                    if (seperations[0] == "pwd")
                    {
                        var result = JsonConvert.DeserializeObject<Entities.pwd>(response.Content);
                        Alert.AlertUser(Alert.AlertType.Normal, result.cd);
                    }
                    else if (seperations[0] == "ls")
                    {
                        if (seperations.Length == 2)
                        {
                            if (seperations[0] == "ls" && seperations[1] == "-al")
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

                }
                catch (IndexOutOfRangeException) { Console.WriteLine(response.Content); }

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
