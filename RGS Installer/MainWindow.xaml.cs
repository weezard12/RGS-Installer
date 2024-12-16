using RGS_Installer.Logic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using static RGS_Installer.Logic.InstalledAppControl;
using static RGS_Installer.SelectApp;


namespace RGS_Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly string InstallerConsolePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "RGS\\RGS Installer\\RGS Installer Console.exe");

        public static MainWindow Instance;

        private static readonly StartIconWindow _startIconWindow = new StartIconWindow();

        public static Stack<Window> DialogWindows { get; set; } = new Stack<Window>();

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Instance = this;
            Hide();

            _startIconWindow.Show();
            _startIconWindow.Focus();

            Thread setupThread = new Thread(SetupApp);
            setupThread.Start();
        }
        public void SetupApp()
        {
            string jsonFile = UseInstallerConsole("releases");
            Dispatcher.Invoke(() =>
            {
                RefreshAvilableApps(jsonFile);
                _startIconWindow.Close();

                Show();
                Focus();
            });
        }

        public void RefreshAvilableApps()
        {   
            string jsonFile = UseInstallerConsole("releases");
            if (jsonFile == "")
                return;

            RefreshAvilableApps(jsonFile);
        }
        public void RefreshAvilableApps(string jsonFile)
        {
            Apps installedApps = null;
            try
            {
                installedApps = JsonSerializer.Deserialize<Apps>(File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RGS\\RGS Installer\\apps.json")));
            }
            catch (Exception ex)
            {
            }

            Dispatcher.Invoke(() =>
            {
                AppsPanel.Children.Clear();
            
                Releases releases = JsonSerializer.Deserialize<Releases>(jsonFile);
                foreach (ReleaseInfo release in releases.ReleasesInfos)
                {
                    InstalledApp matchedApp = installedApps?.InstalledApps.FirstOrDefault(app => app.AppName == release.RepoName);
                    if (matchedApp != null)
                        AppsPanel.Children.Add(new InstalledAppControl(release, matchedApp));
                    else
                        AppsPanel.Children.Add(new WebAppControl(release));
                }
            });
        }

        private SelectApp[] ConvertConsoleOutputToApp;

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public static void UseInstallerConsoleOnOtherThread(params string[] args)
        {
            Thread thread = new Thread(() =>
            {
                UseInstallerConsole(args);
            });
            thread.Start();
        }
        public static void UseInstallerConsoleOnOtherThread(Action actionAfterThreadFinished, params string[] args)
        {
            Thread thread = new Thread(() =>
            {
                UseInstallerConsole(args);
                actionAfterThreadFinished.Invoke();
            });
            thread.Start();
        }
        public static string UseInstallerConsole(params string[] args)
        {
            string argsText = String.Empty;
            foreach (string arg in args)
            {
                bool hasSpace = false;
                // if a parameter contains a space like in a path for example it will add "" to make the path valid as one argument
                if(arg.Contains(' '))
                {
                    argsText += @"""";
                    hasSpace = true;
                }   
                argsText += arg + (hasSpace ? @""" " : " ");
            }
            argsText.Substring(0, argsText.Length - 1);


            string consoleOutput = String.Empty;
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = MainWindow.InstallerConsolePath,
                    Arguments = argsText,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                

                using (Process process = new Process())
                {
                    process.StartInfo = processInfo;
                    process.Start();

                    consoleOutput = process.StandardOutput.ReadToEnd();

                    process.WaitForExit();
                };
            }
            catch (Exception ex)
            {
                if(ex is Win32Exception)
                {

                }

                MessageBox.Show(ex.ToString());
            }

            return consoleOutput;
        }


        // Handles mouse drag to move the window
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Start dragging the window when the user clicks anywhere on the window
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Handles mouse down for canceling dialogs
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            while(DialogWindows.Count != 0)
            {
                DialogWindows.Pop().Close();
            }
        }
    }
}