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
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        public ErrorWindow(string ErrorContext) : this()
        {
            InitializeComponent();
            ErrorDescription.Content = ErrorContext;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
