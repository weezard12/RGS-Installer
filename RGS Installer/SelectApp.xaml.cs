
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;


namespace RGS_Installer
{
    /// <summary>
    /// Interaction logic for SelectApp.xaml
    /// </summary>
    public partial class SelectApp : UserControl
    {
        ReleaseInfo _releaseInfo;
        public SelectApp(ReleaseInfo releaseInfo)
        {
            InitializeComponent();
            this._releaseInfo = releaseInfo;

            SetupXamlByApp();
        }
        private void SetupXamlByApp()
        {
            AppName.Content = _releaseInfo.Name;

            Thread loadImageThread = new Thread(SetAppImage);
            //loadImageThread.IsBackground = true;
            loadImageThread.Start();
        }
        private void SetAppImage()
        {
            if (_releaseInfo.Tag != "rgs_installer")
                return;


            MainWindow.UseInstallerConsole("installicon", $@"""{_releaseInfo.Name}""", $@"""{_releaseInfo.URL}""");
            Dispatcher.Invoke(() =>
            {
                BitmapImage appImage = GetImageSource(Path.Combine(Path.GetTempPath(), $"RGS Installer\\Icons\\{_releaseInfo.Name}.png"));

                if (appImage != null)
                {
                    // sets the app image
                    AppImage.Source = appImage;
                    AppImage.Margin = new Thickness(10);

                    //sets the image background gradient
                    GradientColor gradientColor = GradientColor.GetBestGradient(GradientColor.GetDominantColors(appImage));
                    FirstImageColor.Color = gradientColor.FirstColor;
                    SecondImageColor.Color = gradientColor.SecondColor;
                }


            });


        }

        private BitmapImage GetImageSource(string filePath)
        {
            try
            {
                // Create a new BitmapImage
                BitmapImage bitmap = new BitmapImage();

                    // Initialize the BitmapImage
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Ensures the file is not locked
                    bitmap.EndInit();
                
                return bitmap;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void Install_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //MainWindow.UseInstallerConsole("install","");
            SelectInstallPath selectPathDialog = new SelectInstallPath(this)
            {
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                Owner = MainWindow.Instance,
            };
            MainWindow.DialogWindows.Push(selectPathDialog);
            selectPathDialog.Show();
            selectPathDialog.Focus();
            
        }

        public void InstallApp(string installPath)
        {
            MainWindow.UseInstallerConsole("install", installPath, _releaseInfo.URL);
        }

        public class Releases
        {
            [JsonPropertyName("releases_infos")]
            public ReleaseInfo[] ReleasesInfos { get; set; }
        }

        public class ReleaseInfo
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("repo_name")]
            public string RepoName { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }

            [JsonPropertyName("url")]
            public string URL { get; set; }

            [JsonPropertyName("tag")]
            public string Tag { get; set; }

            [JsonPropertyName("date")]
            public string Date { get; set; }

            public override string ToString()
            {
                return string.Format("Name: {0}\n Repo Name: {1} \n URL: {2}\n Tag: {3}\n Date: {4}\n Description: {5}", Name, RepoName, URL, Tag, Date, Description);
            }
        }


    }
}
