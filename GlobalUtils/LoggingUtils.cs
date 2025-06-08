using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalUtils
{
    public static class LoggingUtils
    {
        public static bool LoggingEnabled = true;
        public static string LogFilePath = Path.Combine(Path.GetTempPath(), "RGS Installer\\rgs_installer_log.txt");

        public static void Log(string message)
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
    }
}
