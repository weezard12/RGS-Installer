using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using static RGS_Installer.SelectApp;


namespace RGS_Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly string InstallerConsolePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "RGS\\RGS Installer\\RGS Installer Console.exe");
        public MainWindow()
        {
            InitializeComponent();
            //MainFrame.Navigate(new MainSelectionPage());

            RefreshAvilableApps();
        }

        public void RefreshAvilableApps()
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = InstallerConsolePath,
                Arguments = "releases",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false,
            };

            string consoleOutput;

            using (Process process = new Process())
            {
                process.StartInfo = processInfo;
                process.Start();

                consoleOutput = process.StandardOutput.ReadToEnd();

                process.WaitForExit();
            };
            

            AppsPanel.Children.Clear();
            
            Releases releases = JsonSerializer.Deserialize<Releases>(consoleOutput);
            foreach(ReleaseInfo release in releases.ReleasesInfos)
            AppsPanel.Children.Add(new SelectApp(release));
        }

        private SelectApp[] ConvertConsoleOutputToApp;

        // Handles mouse drag to move the window
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Start dragging the window when the user clicks anywhere on the window
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        public static string UseInstallerConsole(params string[] args)
        {
            string argsText = String.Empty;
            foreach (string arg in args)
            {
                argsText += arg + " ";
            }
            argsText.Substring(0, argsText.Length - 1);
            
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = MainWindow.InstallerConsolePath,
                Arguments = argsText,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false,
            };
            string consoleOutput;

            using (Process process = new Process())
            {
                process.StartInfo = processInfo;
                process.Start();

                consoleOutput = process.StandardOutput.ReadToEnd();

                process.WaitForExit();
            };
            return consoleOutput;
        }
    }
}