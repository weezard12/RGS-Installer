using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using static GlobalUtils.LoggingUtils;
using static GlobalUtils.FileUtils;

namespace JsonLibrary
{
    // json for arry of installed apps
    public class Apps
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
            if (installedApps == null)
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
            for (int i = 0; i < currentInstalledApps.Count;)
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
}
