using RGS_Installer.SmallUiParts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JsonLibrary;

namespace RGS_Installer.Logic
{
    internal class InstalledAppControl : SelectApp
    {
        private InstalledApp _installedApp { get; set; }
        public InstalledAppControl(ReleaseInfo releaseInfo, InstalledApp installedApp) : base(releaseInfo)
        {
            _installedApp = installedApp;

            CustomButton openButton = new CustomButton("Open",Color.FromArgb(0,0,0,0),Color.FromArgb(1,1,1,1));

            openButton.OnClisckAction = OpenInstalledApp;

            ButtonsGrid.Children.Add(openButton);
            Grid.SetRow(openButton, 0);

            CustomButton unInstall = new CustomButton("Uninstall", Color.FromArgb(0, 0, 0, 0), Color.FromArgb(1, 1, 1, 1));

            unInstall.OnClisckAction = UnInstallApp;

            ButtonsGrid.Children.Add(unInstall);
            Grid.SetRow(unInstall, 1);
        }
        public void OpenInstalledApp()
        {
            string[] exePaths = Utils.GetExeFromFolder(_installedApp.Path);
            if (exePaths.Length == 0)
            {
                Process.Start("explorer.exe", _installedApp.Path);
            }
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo(exePaths[0]);
                Process.Start(processInfo);
            }
            catch
            {
                Process.Start("explorer.exe", _installedApp.Path);
            }

        }

        public void UnInstallApp()
        {
            VerifyWindow verifyUnInstall = new VerifyWindow("Are you sure you want to uninstall the app in path:\n" + _installedApp.Path);
            bool? result = verifyUnInstall.ShowDialog();
            if (result == true)
            {
                MainWindow.UseInstallerConsole("uninstall", _installedApp.Path);
                MainWindow.Instance.RefreshAvilableApps();
            }
        }

    }
}
