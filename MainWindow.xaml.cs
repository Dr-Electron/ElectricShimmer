using ElectricShimmer.ViewModel;
using MaterialDesignExtensions.Controls;
using MaterialDesignExtensions.Model;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using Tommy;

namespace ElectricShimmer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MaterialWindow
    {
        public List<INavigationItem> NavigationItems { get; }

        public MainWindow()
        {
            InitializeComponent();

            Config.Init();
            Log.Level = Config.LogLevel;

            NavigationItems = new List<INavigationItem>()
            {
                new FirstLevelNavigationItem() { Label = "Wallet", Icon = PackIconKind.Wallet, NavigationItemSelectedCallback = item => new WalletViewModel() },
                new FirstLevelNavigationItem() { Label = "Faucet", Icon = PackIconKind.Magic, NavigationItemSelectedCallback = item => new FaucetViewModel() },
                new FirstLevelNavigationItem() { Label = "Settings", Icon = PackIconKind.Settings, NavigationItemSelectedCallback = item => new SettingsViewModel() },
            };

            new DispatcherTimer(
                       TimeSpan.Zero,
                       DispatcherPriority.Loaded,
                       dispatcherTimer_Tick,
                       Application.Current.Dispatcher);
        }

        private async void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                (sender as DispatcherTimer).Stop();

                bool IsUpdateEnabled = Config.IsUpdateEnabled;

                if (IsUpdateEnabled && Updater.CheckUpdate())
                {
                    MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("There is an Update available." + Environment.NewLine + "Do you want to update?", "Update", MessageBoxButton.YesNo), "MainDialogHost");
                    if (result == MessageBoxResult.Yes)
                    {
                        UpdateProgressBar.Visibility = Visibility.Visible;
                        Updater.Update();
                    }
                    else
                    {
                        if (!Check_CLI_Wallet())
                            Install_CLI_Wallet();
                        else
                            CloseSplashView(2);
                    }

                }
                else
                {
                    if (!Check_CLI_Wallet())
                        Install_CLI_Wallet();
                    else
                        CloseSplashView(2);
                }
            }
            catch (Exception exc)
            {
                Log.Write(exc.Message, LogLevel.EXCEPTION);
            }
        }

        public void Install_CLI_Wallet()
        {
            try
            {
                Github goshimmer = new Github("https://api.github.com/repos/iotaledger/goshimmer/releases");
                GithubObjects.Releases.Assets app = goshimmer.GetReleasebyAssetName("cli-wallet.*_Windows").assets.Where(x => Regex.Match(x.name, "cli-wallet.*_Windows").Success).ToList()[0];
                string download_url = app.browser_download_url;

                UpdateProgressBar.Visibility = Visibility.Visible;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += DownloadProgressChanged;
                    wc.DownloadFileCompleted += (sender, args) =>
                    {
                        //string t = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        ZipFile.ExtractToDirectory(app.name, @".\");
                        File.Delete(app.name);
                        UpdateProgressBar.Visibility = Visibility.Collapsed;
                        UpdateProgressBar.Value = 0;
                        CloseSplashView(1);
                    };
                    wc.DownloadFileAsync(
                            new Uri(download_url),
                            app.name
                    );
                }
            }
            catch (Exception exc)
            {
                Log.Write(exc.Message, LogLevel.EXCEPTION);
            }
        }

        public bool Check_CLI_Wallet()
        {
            try
            {
                ProcessStartInfo startinfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = Config.CliExecName,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                Process process = new Process() { StartInfo = startinfo };
                try { process.Start(); }
                catch (Win32Exception)
                {
                    return false;
                }
                process.WaitForExit();
                StreamReader sr = process.StandardOutput;
                return sr.ReadToEnd().Contains("IOTA Pollen CLI-Wallet");
            }
            catch (Exception e)
            {
                Log.Write(e.Message, LogLevel.EXCEPTION);
                return false;
            }
        }

        public void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UpdateProgressBar.Value = e.ProgressPercentage;
        }

        public void DownloadFinished(object sender, AsyncCompletedEventArgs e)
        {
            UpdateProgressBar.Visibility = Visibility.Collapsed;
            UpdateProgressBar.Value = 0;
            Updater.FinishingUpdate();
        }

        private void CloseSplashView(int seconds)
        {
            try
            {
                DispatcherTimer dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += (_sender, _e) =>
                {
                    SplashGrid.Visibility = Visibility.Collapsed;
                    navRail.SelectedItem = NavigationItems[0];
                    NavigationItems[0].IsSelected = true;
                    navRail.DataContext = this;
                    (_sender as DispatcherTimer).Stop();
                };
                dispatcherTimer.Interval = new TimeSpan(0, 0, seconds);
                dispatcherTimer.Start();
            }
            catch (Exception exc)
            {
                Log.Write(exc.Message, LogLevel.EXCEPTION);
            }
        }
        private void NavigationItemSelectedHandler(object sender, NavigationItemSelectedEventArgs args)
        {
            SelectNavigationItem(args.NavigationItem);
        }

        private void SelectNavigationItem(INavigationItem navigationItem)
        {
            if (navigationItem != null)
            {
                contentControl.Content = navigationItem.NavigationItemSelectedCallback(navigationItem);
            }
            else
            {
                contentControl.Content = null;
            }
        }

        private void GoToElectricShimmerGitHub(object sender, RoutedEventArgs args)
        {
            OpenLink("https://github.com/Dr-Electron/ElectricShimmer");
        }

        private void GoToGoShimmerGitHub(object sender, RoutedEventArgs args)
        {
            OpenLink("https://github.com/iotaledger/goshimmer");
        }

        private void OpenLink(string url)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };

                Process.Start(psi);
            }
            catch (Exception exc)
            {
                Log.Write(exc.Message, LogLevel.EXCEPTION);
            }
        }
    }
}
