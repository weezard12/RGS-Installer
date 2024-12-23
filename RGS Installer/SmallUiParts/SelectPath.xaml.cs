using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RGS_Installer.SmallUiParts
{
    /// <summary>
    /// Interaction logic for SelectPath.xaml
    /// </summary>
    public partial class SelectPath : UserControl
    {
        public string SelectedPath { get => FilePath.Text; private set => FilePath.Text = value; }
        public SelectPath()
        {
            InitializeComponent();
        }

        private void SelectPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFileDialog = new OpenFolderDialog();

            bool? response = openFileDialog.ShowDialog();

            if (response == true)
                SelectedPath = openFileDialog.FolderName;
        }
    }
}
