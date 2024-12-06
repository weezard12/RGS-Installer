using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls.Primitives;
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

        public static Stack<Window> DialogWindows { get; set; } = new Stack<Window>();

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            RefreshAvilableApps();
        }

        public void RefreshAvilableApps()
        {   
            AppsPanel.Children.Clear();

            string jsonFile = UseInstallerConsole("releases");
            if (jsonFile == "")
                return;
            
            Releases releases = JsonSerializer.Deserialize<Releases>(jsonFile);
            foreach(ReleaseInfo release in releases.ReleasesInfos)
            AppsPanel.Children.Add(new SelectApp(release));
        }

        private SelectApp[] ConvertConsoleOutputToApp;

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
                    CreateNoWindow = false,
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