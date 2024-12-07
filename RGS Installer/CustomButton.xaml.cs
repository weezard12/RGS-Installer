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

namespace RGS_Installer
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class CustomButton : UserControl
    {
        public Action OnClisckAction { get; set; }
        public CustomButton(string text, Color leftColor, Color rightColor)
        {
            InitializeComponent();
            XmlCustomButton.Content = text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnClisckAction?.Invoke();
        }
    }
}
