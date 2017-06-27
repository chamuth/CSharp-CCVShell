# CCVShell Client

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CCVShell it's a virtual SSH developed for Linux and Windows Servers. CCVShell comes with two editions client version and the server version. Client edition is stored in the client computer, and is used to access a CCVShell server.

## CONFIGURATION OF CCVSHELL CLIENT
1. First add CCVShell.exe to your `PATH` environment variable if it's not added previously. Use `PATH` in command prompt to learn more.
2. Use `ccvshell add [SERVER_NAME] [SERVER_URL]` to add and save a server configuration to your system. (A trailing slash at the end of the `SERVER_URL` is mandatory)
	- Provide the Password that you stored in the `config.json` in your [CCVShell server](http://www.github.com/Chamuth/php-ccvshell)
	- Provide the local directory for your server. (Essential in syncing files between the remote server and your computer)
3. Use `ccvshell connect [SERVER_NAME]` to connect to the configured server. No password entry is required. 
4. Use commands given below for navigation, I/O and other processes

## COMMANDS FOR CCVSHELL
- Adding and configuring a CCVShell server
	- add : Add a server
- List all the servers configured
	- list : List all the servers
	- list [SERVER_NAME] : List information about a specific server
- Update configured servers
	- update [SERVER_NAME] : Update a specific server

## COMMANDS FOR CONNECTED CCVSHELL
- Navigation through files in the server
	- pwd : See the current directory
	- cd : Change the current directory
- Copy, move, rename, and delete files
	- move : Move files
	- ren : Rename files in your server
	- rm : Remove files from your server (delete files)
	- copy : Copy files in your server
- Get Server information easily
	- serverinfo : Get server information
- Zipping and Unzipping files
	- zip : Zip files, add directories, and files to a specified zip file
	- unzip : Extract files to a specified location
- CPM (SSH Package Manager)
	- cpm install : Easily install packages like Twitter Bootstrap, jQuery, Materialize, Tether and etc.
- File Searching
	- search : Search files with case insensitivity and with extensions specified
- Syncing files between local and remote directories
	- sync up : upload files to the remote server
	- sync down : download remote files from the remote server to the local directory
- Using the web server as a HTML proxy
	- http : used to send HTTP GET requests to a specified server and obtain raw html data from it. Use -copy attribute to copy that to the clipboard
- Editing files without downloading and uploading.
	- edit : edit files in notepad or specified program without downloading or uploading, let ccvshell client do it's task
