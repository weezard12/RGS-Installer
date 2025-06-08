using RGS_Installer.SmallUiParts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static RGS_Installer.SelectApp;
using JsonLibrary;

namespace RGS_Installer.Logic
{
    internal class WebAppControl : SelectApp
    {
        CustomButton installButton;
        public WebAppControl(ReleaseInfo releaseInfo) : base(releaseInfo)
        {
            installButton = new CustomButton("Install", Install_Click);

            ButtonsGrid.Children.Add(installButton);
            Grid.SetRow(installButton, 0);
        }
        private void Install_Click()
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
        public override void InstallApp(string installPath)
        {
            ButtonsGrid.Children.Remove(installButton);
            
            InstallLoading installLoading = new InstallLoading();
            ButtonsGrid.Children.Add(installLoading);
            Grid.SetRow(installLoading, 0);

            MainWindow.UseInstallerConsoleOnOtherThread(FinishedInstalling, "install", installPath, _releaseInfo.URL, "publish.zip");

        }
        public void FinishedInstalling()
        {
            MainWindow.Instance.RefreshAvilableApps();
        }

    }
}
