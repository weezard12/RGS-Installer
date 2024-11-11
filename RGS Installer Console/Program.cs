
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RGS_Installer_Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine(args[0]);
            InstallCommand("C:\\Users\\User1\\AppData\\Local\\Temp\\RGS Installer", "weezard12/RGS-Manager");
            Console.ReadLine();
        }

        private static async void InstallCommand(string installetionPath, string gitRepo)
        {
            await InstallLatestReleaseAsync(installetionPath, gitRepo);
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
    }
}
