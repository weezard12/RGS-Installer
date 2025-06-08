using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JsonLibrary
{
    // json of a release
    public class ReleaseInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("repo_name")]
        public string RepoName { get; set; }

        [JsonPropertyName("body")]
        public string Description { get; set; }

        [JsonPropertyName("html_url")]
        public string URL { get; set; }

        [JsonPropertyName("tag_name")]
        public string Tag { get; set; }

        [JsonPropertyName("published_at")]
        public string Date { get; set; }

        [JsonPropertyName("assets")]
        public Asset[] Assets { get; set; }

        public override string ToString()
        {
            return string.Format("Name: {0}\n Repo Name: {1} \n URL: {2}\n Tag: {3}\n Date: {4}\n Description: {5}",
                Name, RepoName, URL, Tag, Date, Description);
        }
    }
}
