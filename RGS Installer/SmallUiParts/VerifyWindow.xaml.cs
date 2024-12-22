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

namespace RGS_Installer.SmallUiParts
{
    /// <summary>
    /// Interaction logic for VerifyWindow.xaml
    /// </summary>
    public partial class VerifyWindow : Window
    {
        public VerifyWindow(string title)
        {
            InitializeComponent();

            Title.Content = title;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; // Sets the result to true
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Sets the result to false
            Close();
        }
    }
}
