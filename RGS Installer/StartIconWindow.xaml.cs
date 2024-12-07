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
using System.Windows.Threading;

namespace RGS_Installer
{
    /// <summary>
    /// Interaction logic for StartIconWindow.xaml
    /// </summary>
    public partial class StartIconWindow : Window
    {
        public StartIconWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
/*            new Thread(new ThreadStart(() =>
            {

                    while (LoadingProgress.Offset < 2)
                    {
                        LoadingProgress.Offset += 0.01;
                        Thread.Sleep(100);
                    }
                
            })).Start();*/
        }
    }
}
