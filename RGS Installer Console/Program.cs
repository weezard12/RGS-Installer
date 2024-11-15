
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace RGS_Installer_Console
{
    internal class Program
    {
        private static readonly string GITHUB_TOKEN = "";
        private static readonly string INSTALLER_TAG = "rgs_installer";
        private static readonly string USERNAME = "weezard12";
        // commands
        // install {repository_name}
        // update {repository_name}
        // releases {username} - prints all of the urls to of the latest releases that have rgs_installer tag
        public static void Main(string[] args)
        {
            //Console.WriteLine(args[0]);
            //InstallCommand("C:\\Users\\User1\\AppData\\Local\\Temp\\RGS Installer", "weezard12/RGS-Manager");
            //if(args.Length == 0)
            //StartBasicSetup();

            //CreateRGSFolder();

            GetReleases();

            Console.ReadLine();
        }

        public static async void GetReleases()
        {
            ReleaseInfo[] allReleases = await GetAllReleasedRepos("weezard12");
            foreach (var release in allReleases)
            {
                if(release.Tag == INSTALLER_TAG)
                {
                    Console.WriteLine(release.Name);
                    Console.WriteLine(release.Description);
                    Console.WriteLine(release.Date);
                    Console.WriteLine(release.URL);
                    
                }
                
            }
                
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
            await InstallReleaseInFolder(releaseInfo,"icon.png","/RGS Installer/Icons");
            File.Move("icon.png", releaseInfo.Name);
        }

        private static async Task<List<string>> InstallReleaseInFolder(ReleaseInfo releaseInfo, string assetName, string installPath)
        {
            return await InstallReleaseInFolder(releaseInfo.URL,releaseInfo.Tag, assetName, installPath);
        }
        private static async Task<List<string>> InstallReleaseInFolder(string releaseURL, string releaseTag, string assetName, string installPath)
        {
            HttpClient _httpClient = new HttpClient();

            var savedFiles = new List<string>();
            string tempFolder = Path.Combine(Path.GetTempPath(), installPath);

            try
            {
                // Load the GitHub token from environment variable
                string token = GITHUB_TOKEN;
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Error GitHub token not found. Please set the GITHUB_TOKEN environment variable.");
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


                // Save the file to the Temp folder
                string filePath = Path.Combine(tempFolder, assetName);
                await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await assetResponse.Content.CopyToAsync(fileStream);
                }

                Console.WriteLine($"Downloaded and saved {assetName} to {filePath}");
                savedFiles.Add(filePath);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error An error occurred: {ex.Message}");
            }

            return savedFiles;
        }

        #region BasicSetup
        private static async void StartBasicSetup()
        {
            Console.WriteLine("Welcome to RGS Installer Console!");
            Console.WriteLine("This is the non UI application for installing RGS apps easelly.");
            if (!IsRunningAsAdministrator())
            {
                Console.WriteLine("Please open this app as an Administrator for installing apps on this computer.");
                return;
            }
            Console.WriteLine("Starting Setup...");
            if (!IsGitInstalled())
            {
                await InstallLatestGit();
            }
        }
        private static void CreateRGSFolder()
        {
            string userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            string newFolderName = "RGS Studios";
            string newFolderPath = Path.Combine(userDirectory, newFolderName);

            
            Directory.CreateDirectory(newFolderPath);
            
            Console.WriteLine($"Folder created at: {newFolderPath}");
        }

        #endregion

        #region Install

        

        private static async void InstallCommand(string installetionPath, string downloadUrl)
        {
            //await InstallReleaseInFolder(downloadUrl,INSTALLER_TAG,);
            Console.ReadKey();
        }
        public static async Task InstallLatestReleaseAsync(string path, string repoName)
        {
            Console.WriteLine("Starting Intallesion");
            try
            {
                // Ensure the target path exists
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                Console.WriteLine("Directory Is Valid");

                // Get the latest release tag from GitHub API
                string latestTag = await GetLatestReleaseTagAsync(repoName);
                Console.WriteLine("latest tag: "+ latestTag);
                if (string.IsNullOrEmpty(latestTag))
                {
                    Console.WriteLine("Could not find the latest release tag.");
                    return;
                }

                // Clone the specific release using Git
                string gitCloneCommand = $"clone --branch {latestTag} https://github.com/{repoName}.git \"{path}\"";
                RunGitCommand(gitCloneCommand);
                Console.WriteLine($"Latest release '{latestTag}' of '{repoName}' has been installed to '{path}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static async Task<string> GetLatestReleaseTagAsync(string repoName)
        {
            try
            {
                using HttpClient client = new HttpClient();

                // Set the User-Agent header required by GitHub API
                client.DefaultRequestHeaders.UserAgent.ParseAdd("CSharp-App");

                string apiUrl = $"https://api.github.com/repos/{repoName}/releases/latest";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch the latest release information. Status Code: {response.StatusCode}");
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Error message: " + errorMessage);
                    return null;
                }

                // Parse the JSON response to retrieve the tag name
                string jsonResponse = await response.Content.ReadAsStringAsync();
                JsonDocument jsonDoc = JsonDocument.Parse(jsonResponse);
                string tagName = jsonDoc.RootElement.GetProperty("tag_name").GetString();
                return tagName;
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine("HTTP request error: " + httpEx.Message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return null;
            }
        }

        static async Task InstallLatestGit()
        {
            string gitInstallerUrl = "https://github.com/git-for-windows/git/releases/latest/download/Git-x64.exe";
            string installerPath = Path.Combine(Path.GetTempPath(), "GitInstaller.exe");

            // Download Git installer
            Console.WriteLine("Downloading Git installer...");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(gitInstallerUrl);
                response.EnsureSuccessStatusCode();

                await using var fs = new FileStream(installerPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs);
            }

            // Run the installer in silent mode
            Console.WriteLine("Running Git installer...");
            Process installProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = "/SILENT",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            installProcess.Start();
            installProcess.WaitForExit();

            if (installProcess.ExitCode == 0)
            {
                Console.WriteLine("Git installation completed successfully.");

                // Verify Git installation
                Process verifyProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                verifyProcess.Start();
                string output = await verifyProcess.StandardOutput.ReadToEndAsync();
                verifyProcess.WaitForExit();

                Console.WriteLine("Git Version: " + output);
            }
            else
            {
                Console.WriteLine("Git installation failed.");
                Console.WriteLine(await installProcess.StandardError.ReadToEndAsync());
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

        public static async Task<string[]> FilterReleasesByTag(string[] releaseUrls, string tagName)
        {
            HttpClient _httpClient = new HttpClient();

            var filteredReleaseUrls = new List<string>();

            foreach (var releaseUrl in releaseUrls)
            {
                try
                {
                    // GitHub API for release metadata using the release URL
                    var releaseResponse = await _httpClient.GetAsync(releaseUrl);

                    if (releaseResponse.IsSuccessStatusCode)
                    {
                        var releaseJson = await releaseResponse.Content.ReadAsStringAsync();
                        var release = JObject.Parse(releaseJson);

                        // Check if the release's tag_name matches the specified tag
                        string tag = release["tag_name"]?.ToString();

                        if (tag == tagName)
                        {
                            filteredReleaseUrls.Add(releaseUrl);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error failed to fetch or parse release at {releaseUrl}: {ex.Message}");
                }
            }

            return filteredReleaseUrls.ToArray();
        }
        private static void RunGitCommand(string arguments)
        {
            Console.WriteLine("Git command: " + arguments);
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = Process.Start(processInfo);
            process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception("Git command failed to execute.");
            }
        }

        // installer info.txt
        // name
        // version
        // release date
        // grediant bkg start
        // grediant bkg end
        // grediant image start
        // grediant image end

        private class ReleaseInfo
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string URL { get; set; }
            public string Tag { get; set; }
            public string Date { get; set; }

            public override string ToString()
            {
                return string.Format("Name: {0}\n URL: {1}\n Tag: {2}\n Date: {3}\n Description: {4}", Name,URL,Tag,Date,Description);

            }
        }
    }
}
