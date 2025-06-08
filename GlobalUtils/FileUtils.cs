namespace GlobalUtils
{
    using System.Text;
    using static GlobalUtils.LoggingUtils;

    public class FileUtils
    {
        public static void CreateFolderIfDoesntExist(string path, bool clearDirectory = false)
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
        public static void CreateFileIfDoesntExist(string path, string content = "")
        {
            try
            {
                // If the path contains a directory structure, ensure it exists
                string directoryPath = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    CreateFolderIfDoesntExist(directoryPath);
                }

                // Check if the file already exists
                if (!File.Exists(path))
                {
                    Log("overwriting file:" + path);
                    // Create the file and close the stream immediately
                    File.WriteAllText(path, content);
                }
            }
            catch (Exception ex)
            {
                Log($@"Error The file ""{path}"" could not be created. Error: {ex.Message}");
                throw; // Optionally rethrow the exception if needed
            }
        }
        public static bool ArePathsTheSame(string path1, string path2)
        {
            if (path1.Equals(path2)) return true;

            StringBuilder sb = new StringBuilder();
            foreach (char c in path1)
            {
                if (c == '\\')
                    sb.Append("\\");
                sb.Append(c);
            }
            if (sb.ToString().Equals(path2)) return true;

            sb.Clear();
            foreach (char c in path2)
            {
                if (c == '\\')
                    sb.Append("\\");
                sb.Append(c);
            }
            if (sb.ToString().Equals(path1)) return true;

            return false;
        }
    }
}
