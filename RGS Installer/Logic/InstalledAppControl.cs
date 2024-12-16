using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace RGS_Installer.Logic
{
    internal class InstalledAppControl : SelectApp
    {
        private InstalledApp _installedApp { get; set; }
        public InstalledAppControl(ReleaseInfo releaseInfo, InstalledApp installedApp) : base(releaseInfo)
        {
            _installedApp = installedApp;

            CustomButton openButton = new CustomButton("Open",Color.FromArgb(0,0,0,0),Color.FromArgb(1,1,1,1));

            openButton.OnClisckAction = OpenInstalledApp;

            ButtonsGrid.Children.Add(openButton);
            Grid.SetRow(openButton, 1);
        }
        public void OpenInstalledApp()
        {
            string[] exePaths = Utils.GetExeFromFolder(_installedApp.Path);
            if (exePaths.Length == 0)
            {
                Process.Start("explorer.exe", _installedApp.Path);
            }
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo(exePaths[0]);
                Process.Start(processInfo);
            }
            catch
            {
                Process.Start("explorer.exe", _installedApp.Path);
            }

        }


        #region Jsons
        // json for arry of installed apps
        public class Apps
        {
            [JsonPropertyName("apps")]
            public InstalledApp[] InstalledApps { get; set; }
        }

        // json of an installed app
        public class InstalledApp
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

                    Uri uri = new Uri(url);

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
                    //Log($"Error Failed to parse URL: {url}. Error: {ex.Message}. " + ex);
                }
                return null;
            }
        }
        #endregion
    }
}
