using RGS_Installer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RGS_Installer.Main
{
    /// <summary>
    /// Interaction logic for InstallPage.xaml
    /// </summary>
    public partial class MainSelectionPage : Page
    {
        public MainSelectionPage()
        {
            InitializeComponent();
            MainContent.Content = new SetupPage();
        }

        private void ToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            // Ensure only one ToggleButton is checked at a time
            if (sender is ToggleButton clickedButton)
            {
                // Uncheck all other buttons
                if (clickedButton != UninstallButton) UninstallButton.IsChecked = false;
                if (clickedButton != InstallButton) InstallButton.IsChecked = false;
                if (clickedButton != UpdateButton) UpdateButton.IsChecked = false;

                // Update ContentGrid based on the selected button
/*                ContentGrid.Children.Clear();
                if (clickedButton == UninstallButton)
                    ContentGrid.Children.Add(new UninstallPage());
                else if (clickedButton == InstallButton)
                    ContentGrid.Children.Add(new MainSelectionPage());
                else if (clickedButton == UpdateButton)
                    ContentGrid.Children.Add(new UpdatePage());*/
            }
        }

        private void ToggleButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            // Prevent all buttons from being unchecked by keeping at least one selected
            if (!(UninstallButton.IsChecked == true || InstallButton.IsChecked == true || UpdateButton.IsChecked == true))
            {
                // Recheck the last selected button, defaulting to Install
                InstallButton.IsChecked = true;
                //ContentGrid.Children.Clear();
                //ContentGrid.Children.Add(new MainSelectionPage());
            }
        }

    }
}
