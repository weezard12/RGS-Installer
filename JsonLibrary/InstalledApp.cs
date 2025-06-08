using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static GlobalUtils.LoggingUtils;

namespace JsonLibrary
{
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
                Log($"Error Failed to parse URL: {url}. Error: {ex.Message}. " + ex);
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
}
