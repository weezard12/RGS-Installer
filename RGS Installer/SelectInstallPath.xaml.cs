using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace RGS_Installer
{
    /// <summary>
    /// Interaction logic for SelectInstallPath.xaml
    /// </summary>
    public partial class SelectInstallPath : Window
    {
        private SelectApp _appToInstall;

        private string _installPath;

        public SelectInstallPath(SelectApp appToInstall)
        {
            InitializeComponent();

            this.ShowInTaskbar = false;
            _appToInstall = appToInstall;
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            Close();
            _appToInstall.InstallApp(_installPath);
        }

        private void Path_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFileDialog = new OpenFolderDialog();

            bool? response = openFileDialog.ShowDialog();

            if (response == true)
                USetInstallPath(openFileDialog.FolderName);
            
        }
        private void USetInstallPath(string path)
        {
            _installPath = path;
            PathText.Content = path;
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
        
    }
}
