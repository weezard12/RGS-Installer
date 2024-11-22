﻿
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace RGS_Installer_Console
{
    internal class Program
    {
        private static readonly string GITHUB_TOKEN = "";
        private const string INSTALLER_TAG = "rgs_installer";
        private static readonly string USERNAME = "weezard12";

        private const bool LoggingEnabled = false;
        // commands
        // install {repository_name}
        // installicon {url} {name}
        // update {repository_name}
        // releases {username} - prints all of the urls to of the latest releases that have rgs_installer tag
        public static void Main(string[] args)
        {
            Console.Title = "RGS Installer Console";
            if (args.Length == 0)
            {
                StartBasicSetup();
                Console.ReadLine();
            }
            else if (args[0] == "install")
                InstallCommand(args[1], args[2]);

            else if (args[0] == "releases")
                GetReleases();

            else if (args[0] == "installicon")
            {
                Console.WriteLine(args[0] + " " + args[1] + " " + args[2]);
                InstallReleaseIcon(args[1], args[2]);
            }
                

            Console.ReadLine();
        }

        private static async void InstallCommand(string installationPath, string downloadUrl)
        {
            CreateFolderIfDoesntExist(installationPath, true);
            await InstallReleaseInFolder(downloadUrl,"rgs_installer", "publish.zip", installationPath, true);

            //CreateFolderIfDoesntExist($"{installationPath}", true);
            if(Directory.Exists("C:\\Program Files\\RGS\\Count Playtime"))
                Directory.Delete("C:\\Program Files\\RGS\\Count Playtime");
            ZipFile.ExtractToDirectory(installationPath, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"RGS"));
        }

        public static async void GetReleases()
        {
            ReleaseInfo[] allReleases = await GetAllReleasedRepos("weezard12");

            Releases releases = new Releases() { ReleasesInfos = allReleases };

            Console.WriteLine(JsonSerializer.Serialize<Releases>(releases));
            Environment.Exit(0);
                
        }


        private static async Task<ReleaseInfo[]> GetAllReleasedRepos(string username)
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

                // Step 2: Loop through each repo and get the latest release information if it exists
                foreach (var repo in repos)
                {
                    string repoName = repo["name"]?.ToString();
                    string releasesUrl = $"https://api.github.com/repos/{username}/{repoName}/releases/latest";

                    // Fetch the latest release info for each repository
                    var releaseResponse = await _httpClient.GetAsync(releasesUrl);

                    if (releaseResponse.IsSuccessStatusCode)
                    {
                        var releaseJson = await releaseResponse.Content.ReadAsStringAsync();
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

                        releaseInfos.Add(releaseInfo);
                    }
                    else if (releaseResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine($"Failed to fetch release for repo {repoName}. Status code: {releaseResponse.StatusCode}");
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

        private static async void InstallReleaseIcon(ReleaseInfo releaseInfo)
        {
            InstallReleaseIcon(releaseInfo.Name, releaseInfo.URL, releaseInfo.Tag);
        }
        private static async void InstallReleaseIcon(string releaseName, string releaseUrl, string installerTag = INSTALLER_TAG)
        {
            string iconsPath = Path.Combine(Path.GetTempPath(), $"RGS Installer\\Icons\\{releaseName}.png");

            CreateFolderIfDoesntExist(iconsPath, true);
            await InstallReleaseInFolder(releaseUrl, installerTag, "icon.png", iconsPath, true);

            Environment.Exit(0);
        }


        private static async Task<List<string>> InstallReleaseInFolder(ReleaseInfo releaseInfo, string assetName, string installPath, bool usePathAsFileName = false)
        {
            return await InstallReleaseInFolder(releaseInfo.URL,releaseInfo.Tag, assetName, installPath, usePathAsFileName);
        }
        private static async Task<List<string>> InstallReleaseInFolder(string releaseURL, string releaseTag, string assetName, string installPath, bool usePathAsFileName = false)
        {
            HttpClient _httpClient = new HttpClient();

            var savedFiles = new List<string>();

            Log("Log Installing in path:" + installPath);
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
                    Console.WriteLine($"Error Failed to download asset {assetName}. Status code: {assetResponse.StatusCode}");


                string filePath = usePathAsFileName ? installPath :  Path.Combine(installPath, assetName);
                await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await assetResponse.Content.CopyToAsync(fileStream);
                }

                Log($"Log Downloaded and saved {assetName} to {filePath}");
                savedFiles.Add(filePath);

            }
            catch (Exception ex)
            {
                Log($"Error An error occurred: {ex.Message}");
            }

            return savedFiles;
        }

        #region BasicSetup
        private static void StartBasicSetup()
        {
            Console.WriteLine("Welcome to RGS Installer Console!");
            Console.WriteLine("This is the non UI application for installing RGS apps easelly.");
            if (!IsRunningAsAdministrator())
            {
                Console.WriteLine("Please open this app as an Administrator for installing apps on this computer.");
                return;
            }
            Console.WriteLine("Starting Setup...");

            CreateRGSFolder();
        }
        private static void CreateRGSFolder()
        {
            string userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            string newFolderPath = Path.Combine(userDirectory, "RGS\\Installer");

            CreateFolderIfDoesntExist(newFolderPath);

            string consolePath = Path.Combine(newFolderPath, "RGS Installer Console.exe");

            if (!File.Exists(consolePath))
                File.Copy(Path.Combine(AppContext.BaseDirectory, "RGS Installer Console.exe"), consolePath);

            string jsonPath = Path.Combine(newFolderPath, "Apps.json");
            if (!File.Exists(jsonPath))
            {
                File.Create(jsonPath);
                //File.WriteAllText(jsonPath, "apps{}");
            }
            InstallReleaseInFolder("","rgs_installer","publish.zip",newFolderPath);
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

        private static void Log(string message)
        {
            if (!LoggingEnabled)
                return;
            Console.WriteLine(message);
        }
        private class Releases
        {
            [JsonPropertyName("releases_infos")]
            public ReleaseInfo[] ReleasesInfos { get; set; }
        }

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
    }
}
