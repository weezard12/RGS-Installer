using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JsonLibrary
{
    // json for array of releases
    public class Releases
    {
        [JsonPropertyName("releases_infos")]
        public ReleaseInfo[] ReleasesInfos { get; set; }
    }
}
