using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGS_Installer.Logic
{
    internal class Utils
    {

        public static string[] GetExeFromFolder(string path)
        {
            // Check if the provided path is valid
            if (Directory.Exists(path))
            {
                // Get all .exe files from the folder
                return Directory.GetFiles(path, "*.exe",SearchOption.AllDirectories);
            }
            else
            {
                // Return an empty array if the directory does not exist
                return Array.Empty<string>();
            }
        }

    }
}
