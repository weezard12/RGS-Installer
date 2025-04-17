#define ENCHANTED
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Environment;

namespace RGS_Installer_Console
{
    internal class Program
    {
        //Build
        public const string BUILD_DATE = "17/04/2025";
#if ENCHANTED
        public const string BUILD_VERSION_NAME = "ENCHANTED";
#else
        public const string BUILD_VERSION_NAME = "LIGHT";
#endif
        // GitHub
        private static readonly string GITHUB_TOKEN = "";
        private const string INSTALLER_TAG = "rgs_installer";
        private static bool CLOSE_AFTER_COMMAND = true;

        // Logging
        private const bool LoggingEnabled = true;
        private static readonly string LogFilePath = Path.Combine(Path.GetTempPath(), "RGS Installer\\rgs_installer_log.txt");

        // commands
        // install {installationPath} {release} {asset name} {install_actions[] <create_desktop_shortcut> <save_in_apps.json>}
        // installicon {name} {url} 
        // update {repository_name}
        // releases {username} - prints all of the urls to of the latest releases that have rgs_installer tag
        // desktop_shortcut {path}
        public static void Main(string[] args)
        {

            // creates and clears the logging file
            if (LoggingEnabled)
            {
                try
                {
                    File.WriteAllLines(LogFilePath, []);
                }
                catch (Exception ex)
                {
                }
            }

            // Commands
            if (args.Length == 0)
            {
                Console.Title = "RGS Installer Console";
                StartBasicSetup();
            }
#if ENCHANTED
            else switch(args[0])
            {
                case "install":
                    InstallCommand(args[1], args[2], args[3]);
                    break;

                case "releases":
                    if (args.Length > 1)
                        GetReleases(args[1]);
                    else
                        GetReleases();
                    break;

                case "installicon":
                    InstallReleaseIcon(args[1], args[2]);
                    break;

                case "desktop_shortcut":
                    if (args.Length == 2)
                        CreateShortcutOnDesktop(args[1], Path.GetFileNameWithoutExtension(args[1]));
                    else if (args.Length == 3)
                        CreateShortcutOnDesktop(args[1], args[2]);
                    break;

                case "uninstall":
                    if (args.Length > 1)
                        UnInstall(args[1]);
                    break;

                default:
                    Console.WriteLine("Error - Invalid args");
                    break;
            }
#endif

        commandsLoop:
            string input = Console.ReadLine();
            if (input == "")
                return;
            SimpleCommand.RunCommand(input);
            goto commandsLoop;

        }
#if ENCHANTED
        #region GetReleases
        public static async void GetReleases(string userName = "weezard12", string tag = "")
        {
            ReleaseInfo[] allReleases = await GetAllReleasedRepos(userName, tag);

            Releases releases = new Releases() { ReleasesInfos = allReleases };

            Console.WriteLine(JsonSerializer.Serialize<Releases>(releases));

            Environment.Exit(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns>Array of of a releases. (one for each repo with release).</returns>
        private static async Task<ReleaseInfo[]> GetAllReleasedRepos(string username, string releaseTag)
        {
            HttpClient _httpClient = new HttpClient();
            var releaseInfos = new List<ReleaseInfo>();

            try
            {
                // Load the GitHub personal access token from environment variable
                string token = GITHUB_TOKEN;
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("GitHub token not found. Please set the GITHUB_TOKEN environment variable.");
                    return Array.Empty<ReleaseInfo>();
                }

                // Configure HttpClient to use the GitHub token for authorization
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0"); // Required User-Agent header
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Step 1: Get the list of repositories for the specified user
                string reposUrl = $"https://api.github.com/users/{username}/repos";
                var reposResponse = await _httpClient.GetAsync(reposUrl);

                if (!reposResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch repositories for user {username}. Status code: {reposResponse.StatusCode}");
                    return Array.Empty<ReleaseInfo>();
                }
                    
                var reposJson = await reposResponse.Content.ReadAsStringAsync();
                var repos = JArray.Parse(reposJson);
                Log("Log Fetched repos from github api:");

                // Step 2: Loop through each repo and get the correct release
                foreach (var repo in repos)
                {
                    // Gets the repo name
                    string repoName = repo["name"]?.ToString();

                    string releaseJson = String.Empty;

                    if (releaseTag == "")
                    {
                        // If no tag specified then it will get the latest release
                        string releasesUrl = $"https://api.github.com/repos/{username}/{repoName}/releases/latest";
                        releaseJson = await GetJsonStringFromUrl(releasesUrl, _httpClient);
                    }
                    else
                    {
                        // If a tag is specified then it will get the release of that tag
                        string releasesUrl = $"https://api.github.com/repos/{username}/{repoName}/releases";

                        JArray releases = JArray.Parse(await GetJsonStringFromUrl(releasesUrl, _httpClient));
                        //Console.WriteLine(releases);
                        foreach (var release in releases)
                        {
                            if (release["tag_name"]?.ToString() == releaseTag)
                            {
                                //releasesUrl = release["url"]?.ToString();
                                releaseJson = release.ToString();
                                //Console.WriteLine(releaseJson);
                            }
                        }
                    }
                    try
                    {
                        releaseInfos.Add(GetReleaseFromJson(releaseJson, repoName));
                        Log(releaseInfos.Last().ToString().Replace('\n','.'));
                    }
                    catch
                    {

                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return Array.Empty<ReleaseInfo>();
            }
            return releaseInfos.ToArray();

        }
        private async static Task<string> GetJsonStringFromUrl(string url,HttpClient httpClient)
        {
            var releaseResponse = await httpClient.GetAsync(url);

            if (releaseResponse.IsSuccessStatusCode)
            {
                return await releaseResponse.Content.ReadAsStringAsync();
            }
            else if (releaseResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Failed to fetch url {url}. Status code: {releaseResponse.StatusCode}");
            }
            return null;
        }
        private static ReleaseInfo GetReleaseFromJson(string releaseJson, string repoName = "No Name Loaded")
        {
            var release = JObject.Parse(releaseJson);

            // Create and populate a ReleaseInfo object with release details
            var releaseInfo = new ReleaseInfo
            {
                Name = release["name"]?.ToString(),
                RepoName = repoName,
                Description = release["body"]?.ToString(),
                URL = release["html_url"]?.ToString(),
                Tag = release["tag_name"]?.ToString(),
                Date = release["published_at"]?.ToString()
            };

            return releaseInfo;
        }
        #endregion
#endif
        #region InstallLogic

        private static async void InstallCommand(string installationPath, string releaseUrl, string assetName, Action doAfterInstall = null)
        {
            string tempInstallPath = Path.Combine(Path.GetTempPath(), "RGS Installer\\Download");
            CreateFolderIfDoesntExist(tempInstallPath, true);

            await InstallReleaseInFolder(releaseUrl, assetName, tempInstallPath, false);
            string assetFilePath = Path.Combine(tempInstallPath, assetName);
            string finalInstallPath = installationPath;

            // Check if the file is extractable (zip)
            bool isExtractable = IsExtractableFile(assetName);

            if (isExtractable)
            {
                try
                {
                    // Extract directly to the installation path
                    ZipFile.ExtractToDirectory(assetFilePath, installationPath);

                    // Check if there's only one file in the installation directory
                    string? singleFileName = GetSingleFileNameInZip(assetFilePath);
                    if (singleFileName != null)
                    {
                        // Only one file, update the final path to point to this file
                        finalInstallPath = Path.Combine(installationPath, singleFileName);
                    }
                }
                catch (Exception ex)
                {
                    Log("Error " + ex.Message);

                    // If failed to unzip the file then just try to copy it as it is
                    try
                    {
                        File.Copy(assetFilePath, Path.Combine(installationPath, assetName), true);
                        finalInstallPath = Path.Combine(installationPath, assetName);
                    }
                    catch (Exception copyEx)
                    {
                        Log("Error " + copyEx.Message);
                    }
                }
            }
            else
            {
                // Not extractable, just copy the file directly
                try
                {
                    File.Copy(assetFilePath, Path.Combine(installationPath, assetName), true);
                    finalInstallPath = Path.Combine(installationPath, assetName);
                }
                catch (Exception ex)
                {
                    Log("Error " + ex.Message);
                }
            }

#if ENCHANTED
            InstalledApp installedApp = InstalledApp.FromUrl(releaseUrl);
            installedApp.LastUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            installedApp.Path = finalInstallPath;  // Use the updated path (single file or folder)

            Apps.AddInstalledApp(installedApp);
#endif

            // Invokes optional code before closing the console
            try
            {
                doAfterInstall?.Invoke();
            }
            catch (Exception ex)
            {
                Log($"Error Failed to invoke action after installation. Exception: {ex}");
            }

            if (CLOSE_AFTER_COMMAND)
                Environment.Exit(0);
        }


#if ENCHANTED
        private static async void InstallReleaseIcon(ReleaseInfo releaseInfo)
        {
            InstallReleaseIcon(releaseInfo.Name, releaseInfo.URL, releaseInfo.Tag);
        }
        private static async void InstallReleaseIcon(string releaseName, string releaseUrl, string installerTag = INSTALLER_TAG)
        {
            string iconsPath = Path.Combine(Path.GetTempPath(), $"RGS Installer\\Icons\\{releaseName}.png");

            CreateFolderIfDoesntExist(iconsPath);
            DeleteFileIfExist(iconsPath);
            await InstallReleaseInFolder(releaseUrl, "icon.png", iconsPath, true);

            Environment.Exit(0);
        }

        private static async Task<List<string>> InstallReleaseInFolder(ReleaseInfo releaseInfo, string assetName, string installPath, bool usePathAsFileName = false)
        {
            return await InstallReleaseInFolder(releaseInfo.URL, assetName, installPath, usePathAsFileName);
        }
#endif

        /// <summary>
        /// installs a release asset in a folder. takes in the release Ulease URL with the tag!
        /// </summary>
        /// <param name="releaseURL"></param>
        /// <param name="assetName"></param>
        /// <param name="installPath"></param>
        /// <param name="usePathAsFileName"></param>
        /// <returns></returns>
        private static async Task<List<string>> InstallReleaseInFolder(string releaseURL, string assetName, string installPath, bool usePathAsFileName = false)
        {
            HttpClient _httpClient = new HttpClient();

            var savedFiles = new List<string>();

            Log($"Log Installing Release in path: {installPath} . from url: {releaseURL}, asset name: {assetName}");
            try
            {
                // Load the GitHub token from environment variable
                string token = GITHUB_TOKEN;
                if (string.IsNullOrEmpty(token))
                {
                    Log("Error GitHub token not found. Please set the GITHUB_TOKEN environment variable.");
                    return savedFiles;
                }

                // Configure HttpClient for authentication and User-Agent
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0"); // Required User-Agent header
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string downloadUrl = releaseURL.Replace("/tag/", "/download/") + $"/{assetName}";

                // Download the asset file
                var assetResponse = await _httpClient.GetAsync(downloadUrl);
                if (!assetResponse.IsSuccessStatusCode)
                    Console.WriteLine($"Error Failed to download asset {assetName}. Status code: {assetResponse.StatusCode}. Download URL: {downloadUrl}");


                string filePath = usePathAsFileName ? installPath : Path.Combine(installPath, assetName);
                await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await assetResponse.Content.CopyToAsync(fileStream);
                }

                Log($"Log Downloaded and saved {assetName} to {filePath}");
                savedFiles.Add(filePath);

            }
            catch (Exception ex)
            {
                Log($"Error An error occurred when installing a release: {ex.Message}");
            }

            return savedFiles;
        }
        #endregion
#if ENCHANTED
        #region UnInstallLogic
        private static void UnInstall(string path)
        {
            Log("Log Uninstalling in path: " + path);
            
            Apps.RemoveInstalledAppByPath(path);
            SafeDeleteApp(path);
            Environment.Exit(0);
        }
        #endregion
#endif
#region BasicSetup
        private static void StartBasicSetup()
        {
            Console.ResetColor();
            Console.Write("Welcome to ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("RGS Installer Console");
            Console.ResetColor();
            Console.WriteLine("!");

            Console.WriteLine("This is the non UI application for installing RGS apps easily.");
            if (!IsRunningAsAdministrator())
            {
                Console.Write("Please ");
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.Write("open this app as an Administrator");
                Console.ResetColor();
                Console.WriteLine(" for Installing / Updating the UI installer");
                return;
            }
            Console.WriteLine("Starting Setup...");

            MonitorFile(LogFilePath);

            CreateRGSFolder();
        }
        private static async void CreateRGSFolder()
        {
            string userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            string newFolderPath = Path.Combine(userDirectory, "RGS\\RGS Installer");

            CreateFolderIfDoesntExist(newFolderPath, true);

            string consolePath = Path.Combine(newFolderPath, "RGS Installer Console.exe");


            string installedAppsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RGS\\RGS Installer\\apps.json");
            CreateFileIfDoesntExist(installedAppsPath, @"
{
    ""installed_apps"": [
    ]
}");

            // app will be open after installing
            CLOSE_AFTER_COMMAND = false;

#if ENCHANTED
            if (!File.Exists(consolePath))
                File.Copy(Process.GetCurrentProcess().MainModule.FileName, consolePath);
#else
            await InstallReleaseInFolder("https://github.com/weezard12/RGS-Installer/releases/tag/rgs_installer", "RGS.Installer.Console.exe", consolePath, true);
#endif

            string InstallerExePath = Path.Combine(newFolderPath, "RGS Installer\\RGS Installer.exe");
            InstallCommand(newFolderPath, "https://github.com/weezard12/RGS-Installer/releases/tag/rgs_installer", "publish.zip",
                new Action(() =>
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = InstallerExePath,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    Process.Start(psi);
                }));
            CreateShortcutOnDesktop(InstallerExePath, "RGS Installer");
            CreateShortcutOnDesktop(consolePath, "RGS Installer");
        }

        private static void CreateFolderIfDoesntExist(string path, bool clearDirectory = false)
        {
            //just in case
            if (path.Equals("Program Files") || Path.GetDirectoryName(path).Equals("C:\\") || path.Length < 15)
                return;
            
            Log("Log Creating path " + path);
            try
            {
                //if the path is to a file it will call the method again but with the file folder as path
                if (Path.HasExtension(path))
                {
                    CreateFolderIfDoesntExist(Path.GetDirectoryName(path), clearDirectory);
                    return;
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    return;
                }
                if (clearDirectory)
                {
                    Directory.Delete(path, true);
                    Directory.CreateDirectory(path);
                }
            }
            catch
            {
                Log($@"Error The path ""{path}"" does not exists and returned an error");
                Log($@"Log Trying again...");
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    File.Delete(path);
                    CreateFolderIfDoesntExist(path, clearDirectory);
                }
            }
        }
        private static void CreateFileIfDoesntExist(string path, string content = "")
        {
            try
            {
                // If the path contains a directory structure, ensure it exists
                string directoryPath = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    CreateFolderIfDoesntExist(directoryPath);
                }

                // Check if the file already exists
                if (!File.Exists(path))
                {
                    Log("overwriting file:"+ path);
                    // Create the file and close the stream immediately
                    File.WriteAllText(path,content);
                }
            }
            catch (Exception ex)
            {
                Log($@"Error The file ""{path}"" could not be created. Error: {ex.Message}");
                throw; // Optionally rethrow the exception if needed
            }
        }
        #endregion

        #region Checks
        public static bool IsRunningAsAdministrator()
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }

        public static bool IsGitInstalled()
            {
                try
                {
                    // Run git --version to see if Git is installed
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = Process.Start(psi))
                    {
                        process.WaitForExit();
                        string output = process.StandardOutput.ReadToEnd();
                        if (!string.IsNullOrEmpty(output) && output.Contains("git version"))
                        {
                            Console.WriteLine("Git is installed.");
                            return true;
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Git is not installed.");
                    return false;
                }

                return false;
            }
        #endregion

        #region Utils
        private static void Log(string message)
        {
            if (!LoggingEnabled)
                return;

            try
            {
                // Write the log message to the file
                File.AppendAllText(LogFilePath, $"{DateTime.Now} {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                // Handle potential exceptions (e.g., lack of write permission)
                if (ex is FileNotFoundException)
                    try
                    {
                        File.Create(LogFilePath);
                    }
                    catch { }
            }
        }


        private static void MonitorFile(string filePath)
        {
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Log file not found.");
                return;
            }

            // Create a FileSystemWatcher to monitor the file for changes
            FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath))
            {
                Filter = Path.GetFileName(filePath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            watcher.Changed += (sender, e) =>
            {
                try
                {
                    // Read the new line added to the file
                    string[] lines = File.ReadAllLines(filePath);
                    DebugWriteLineWithColors(lines[lines.Length - 1]);
                }
                catch
                {

                }

            };

            watcher.EnableRaisingEvents = true;
        }

        public static void WriteLineWithColors(string message)
        {
            string[] parts = message.Split(new[] { ' ' }, StringSplitOptions.None);
            
            foreach (var part in parts)
                if(part.ToUpper().Equals(part) && Enum.TryParse(typeof(ConsoleColor), part, true, out object? result))
                    Console.ForegroundColor = (ConsoleColor) result;
                else 
                    Console.Write(part+" ");
            Console.WriteLine();
        }

        private static void DebugWriteLineWithColors(string message)
        {
            // Split the message into parts based on spaces to handle words
            string[] parts = message.Split(new[] { ' ' }, StringSplitOptions.None);

            foreach (var part in parts)
            {
                // Check each character in the part
                foreach (char c in part)
                {
                    if (c == '/')
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write(c);
                    }
                    else if (part.Equals("Log", StringComparison.Ordinal))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(part);
                        break;
                    }
                    else if (part.Equals("Error", StringComparison.Ordinal))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(part);
                        break;
                    }
                    else
                    {
                        // Reset to default color for other text
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(c);
                    }

                }
                // Reset color to default after each part
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" "); // Add space after each part
            }

            // Reset color to default after the entire message
            Console.ResetColor();
            Console.WriteLine();
        }

        private static string GetRepoNameFromURL(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));

            Uri uri = new Uri(url);
            var segments = uri.Segments;

            if (segments.Length < 3)
                throw new FormatException("Invalid GitHub release URL format.");

            string repoName = segments[1].TrimEnd('/');
            return repoName;
        }

        public static void CreateShortcutOnDesktop(string path, string shortcutName)
        {
            string command = String.Format("$Shortcut = (New-Object -COM WScript.Shell).CreateShortcut([System.IO.Path]::Combine($Env:USERPROFILE, 'Desktop', '{1}.lnk')); " +
                             "$Shortcut.TargetPath = '{0}'; " +
                             "$Shortcut.Save()", path, shortcutName);

            // Start the process
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-Command \"{command}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            Process.Start(startInfo);
        }

        public static void SafeDeleteApp(string path)
        {
            //just in case
            foreach (SpecialFolder folder in Enum.GetValues(typeof(Environment.SpecialFolder)))
                if (path.Equals(Environment.GetFolderPath(folder)))
                    return;

            if (Path.GetDirectoryName(path).Equals("C:\\") || path.Length < 10)
                return;

            try
            {
                if(Directory.Exists(path))
                    Directory.Delete(path, true);
                else if (File.Exists(path))
                    File.Delete(path);
                else
                    Log("Warn Path to delete does not exists: " + path);
            }
            catch
            {
                Log("Error Faild to Safe Delete path: " + path);
            }
            
        }

        public static bool ArePathsTheSame(string path1, string path2)
        {
            if(path1.Equals(path2)) return true;

            StringBuilder sb = new StringBuilder();
            foreach(char c in path1)
            {
                if(c == '\\')
                    sb.Append("\\");
                sb.Append(c);
            }
            if (sb.ToString().Equals(path2)) return true;

            sb.Clear();
            foreach (char c in path2)
            {
                if (c == '\\')
                    sb.Append("\\");
                sb.Append(c);
            }
            if (sb.ToString().Equals(path1)) return true;

            return false;
        }

        public static void DeleteFileIfExist(string path)
        {
            if (Directory.Exists(path)) return;
            if (!File.Exists(path)) return;

            try
            {
                File.Delete(path);
                Log("Log Deleted File: " + path);
            }
            catch
            {
                Log("Error Failed to Deleted File: " + path);
            }

        }

        // Helper method to check if a file is extractable
        private static bool IsExtractableFile(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension == ".zip" || extension == ".rar" || extension == ".7z";
        }

        // Helper method to copy a directory and its contents
        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            // Create destination directory if it doesn't exist
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            // Copy all files
            foreach (string filePath in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(filePath);
                string destFile = Path.Combine(destinationDir, fileName);
                File.Copy(filePath, destFile, true);
            }

            // Copy all subdirectories
            foreach (string dirPath in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(dirPath);
                string destDir = Path.Combine(destinationDir, dirName);
                CopyDirectory(dirPath, destDir);
            }
        }

        public static bool IsSingleFileInZip(string zipFilePath)
        {
            if (!File.Exists(zipFilePath))
                throw new FileNotFoundException("Zip file not found.", zipFilePath);

            using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
            {
                int fileCount = 0;

                foreach (var entry in archive.Entries)
                {
                    if (!string.IsNullOrEmpty(entry.Name)) // Only count actual files
                    {
                        fileCount++;
                        if (fileCount > 1)
                            return false;
                    }
                }

                return fileCount == 1;
            }
        }
        // if there is just one file in the zip it will return the file name, else it will return null
        public static string? GetSingleFileNameInZip(string zipFilePath)
        {
            if (!File.Exists(zipFilePath))
                throw new FileNotFoundException("Zip file not found.", zipFilePath);

            using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
            {
                string? singleFileName = null;

                foreach (var entry in archive.Entries)
                {
                    if (!string.IsNullOrEmpty(entry.Name)) // It's a file, not a folder
                    {
                        if (singleFileName != null)
                            return null; // Already found a file, so more than one
                        singleFileName = entry.FullName;
                    }
                }

                return singleFileName;
            }
        }

        #endregion
#if ENCHANTED
        #region jsons
        // json for array of releases
        private class Releases
        {
            [JsonPropertyName("releases_infos")]
            public ReleaseInfo[] ReleasesInfos { get; set; }
        }

        // json of a release
        private class ReleaseInfo
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("repo_name")]
            public string RepoName { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }

            [JsonPropertyName("url")]
            public string URL { get; set; }

            [JsonPropertyName("tag")]
            public string Tag { get; set; }

            [JsonPropertyName("date")]
            public string Date { get; set; }

            public override string ToString()
            {
                return string.Format("Name: {0}\n Repo Name: {1} \n URL: {2}\n Tag: {3}\n Date: {4}\n Description: {5}", Name, RepoName,URL,Tag,Date,Description);
            }
        }


        // json for arry of installed apps
        private class Apps
        {
            [JsonPropertyName("apps")]
            public InstalledApp[] InstalledApps { get; set; }

            public static Apps GetInstalledApps()
            {
                string installedAppsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RGS\\RGS Installer\\apps.json");
                CreateFileIfDoesntExist(installedAppsPath, @"
                {
                    ""installed_apps"": [
                    ]
                }");

                Apps? installedApps = JsonSerializer.Deserialize<Apps>(File.ReadAllText(installedAppsPath));
                if(installedApps == null)
                    installedApps = new Apps();
                if (installedApps.InstalledApps == null)
                    installedApps.InstalledApps = [];
                return installedApps;
            }
            public static void SetInstalledApps(Apps apps)
            {
                string installedAppsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RGS\\RGS Installer\\apps.json");
                CreateFileIfDoesntExist(installedAppsPath, @"
                {
                    ""installed_apps"": [
                    ]
                }");
                File.WriteAllText(installedAppsPath, JsonSerializer.Serialize(apps));
            }
            public static void RemoveInstalledApp(InstalledApp appToRemove)
            {
                Log("Log Uninstalling App:" + appToRemove);
                Apps installedApps = GetInstalledApps();
                List<InstalledApp> currentInstalledApps = installedApps.InstalledApps.ToList();
                currentInstalledApps.Remove(appToRemove);
                installedApps.InstalledApps = currentInstalledApps.ToArray();

                SetInstalledApps(installedApps);
            }
            public static void RemoveInstalledAppByPath(string path)
            {
                bool uninstalled = false;
                List<InstalledApp> currentInstalledApps = Apps.GetInstalledApps().InstalledApps.ToList();
                for(int i = 0; i < currentInstalledApps.Count;)
                {
                    if (!uninstalled && ArePathsTheSame(currentInstalledApps[i].Path, path))
                    {
                        currentInstalledApps.RemoveAt(i);
                        uninstalled = true;
                    }
                    else
                        i++;
                }
                Apps installedApps = new Apps() { InstalledApps = currentInstalledApps.ToArray() };

                SetInstalledApps(installedApps);
            }

            public static void AddInstalledApp(InstalledApp appToAdd)
            {
                Apps installedApps = GetInstalledApps();
                List<InstalledApp> currentInstalledApps = installedApps.InstalledApps.ToList();
                currentInstalledApps.Add(appToAdd);
                installedApps.InstalledApps = currentInstalledApps.ToArray();

                SetInstalledApps(installedApps);
            }
        }

        // json of an installed app
        private class InstalledApp
        {
            [JsonPropertyName("publisher_name")]
            public string PublisherName { get; set; }
            [JsonPropertyName("app_name")]
            public string AppName { get; set; }
            [JsonPropertyName("app_tag")]
            public string AppTag { get; set; }
            [JsonPropertyName("path")]
            public string Path { get; set; }
            [JsonPropertyName("last_update")]
            public string LastUpdate { get; set; }
            [JsonPropertyName("repo")]
            public string RepoURL { get; set; }

            public static InstalledApp FromUrl(string url)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(url))
                        Log("Error URL cannot be null or empty");

                    Uri uri = new Uri(url);
                    if (!uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
                        Log("Error URL must be from github.com");

                    // Split the path into segments: "/weezard12/PlayFiles/releases/tag/test"
                    string[] segments = uri.AbsolutePath.Trim('/').Split('/');
                    if (segments.Length < 5 || segments[2] != "releases" || segments[3] != "tag")
                        throw new ArgumentException("URL format is not valid for a GitHub release");

                    // Extract information
                    string publisher = segments[0];
                    string appName = segments[1];
                    string appTag = segments[4];

                    // Construct the repository URL
                    string repoURL = $"https://github.com/{publisher}/{appName}";

                    // Return a populated InstalledApp object
                    return new InstalledApp
                    {
                        PublisherName = publisher,
                        AppName = appName,
                        AppTag = appTag,
                        RepoURL = repoURL,
                        LastUpdate = DateTime.UtcNow.ToString("o"), // ISO 8601 format for last update
                        Path = null // Path can be assigned later if applicable
                    };
                }
                catch (Exception ex)
                {
                    Log($"Error Failed to parse URL: {url}. Error: {ex.Message}. "+ ex);
                }
                return null;
            }

            public override string ToString()
            {
                return $"InstalledApp:\n" +
                       $"- PublisherName: {PublisherName ?? "N/A"}\n" +
                       $"- AppName: {AppName ?? "N/A"}\n" +
                       $"- AppTag: {AppTag ?? "N/A"}\n" +
                       $"- Path: {Path ?? "N/A"}\n" +
                       $"- LastUpdate: {LastUpdate ?? "N/A"}\n" +
                       $"- RepoURL: {RepoURL ?? "N/A"}";
            }
        }

        #endregion

#endif
        private class SimpleCommand
        {
            public static SimpleCommand[]? Commands;
            public static void SetupCommands()
            {
                if (Commands != null)
                    return;
                Commands =
                [
                    new SimpleCommand("debug", "db", "Shows the installer log file at runtime.", new Action(() => { MonitorFile(LogFilePath); })),
                    new SimpleCommand("clear", "clr", "Clears the console.", new Action(Console.Clear)),
                    new SimpleCommand("buildnum", "bn", "Shows the 'Build Num' of the app.\n(The build date and additional info about the current version)", new Action(() =>
                    {
                        Console.Write("Version ");
                        Console.BackgroundColor = ConsoleColor.Magenta;
                        Console.Write($"{BUILD_VERSION_NAME}");
                        Console.ResetColor();
                        Console.WriteLine($" Build Date: {BUILD_DATE}");
                    })),
                    new SimpleCommand("help", "help", "", new Action(DisplayHelpOfAll)),
                ];
            }
            public static void RunCommand(string commandLine)
            {
                SetupCommands();
                string[] args = commandLine.Split(' ');
                foreach (SimpleCommand command in Commands)
                    if(command.UsedCommand(args))
                        return;
            }
            public static void DisplayHelpOfAll()
            {
                Console.WriteLine("All of the RGS Installer Console Simple Commands:");
                foreach (SimpleCommand command in Commands)
                    command.DisplayHelp();
            }

            private readonly string _name;
            private readonly string _shortcutName;
            private readonly string _description;
            private readonly Action _action;
            public SimpleCommand(string name, string shortcutName, string description, Action action)
            {
                _name = name;
                _shortcutName = shortcutName;
                _description = description;
                _action = action;
            }
            public bool UsedCommand(string[] args)
            {
                if (args[0].ToLower() != _name && args[0].ToLower() != _shortcutName)
                    return false;
                _action.Invoke();
                return true;
            }
            public void DisplayHelp()
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                WriteLineWithColors($"GREEN - DARKGRAY Command: WHITE {_name} DARKGRAY / WHITE {_shortcutName}");
            }

        }
    }
}
