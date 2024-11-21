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
        public SelectInstallPath(SelectApp appToInstall)
        {
            InitializeComponent();

            _appToInstall = appToInstall;
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.UseInstallerConsole("install","");
        }

        private void Path_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
        }
    }
}
