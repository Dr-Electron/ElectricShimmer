using ElectricShimmer.ViewModel;
using MaterialDesignExtensions.Controls;
using MaterialDesignExtensions.Model;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

            if (File.Exists("config.toml"))
            {
                using (StreamReader reader = new StreamReader(File.OpenRead("config.toml")))
                {
                    // Parse the table
                    TomlTable table = TOML.Parse(reader);

                    if (table.HasKey("Logger"))
                    {
                        LogLevel.TryParse((string)table["Logger"]["LogLevel"], out Log.Level);
                    }
                }
            }
        }

        private async void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                (sender as DispatcherTimer).Stop();

                bool IsUpdateEnabled = true;
                if (File.Exists("config.toml"))
                {
                    using (StreamReader reader = new StreamReader(File.OpenRead("config.toml")))
                    {
                        // Parse the table
                        TomlTable table = TOML.Parse(reader);

                        if (table.HasKey("AutoUpdate"))
                            IsUpdateEnabled = table["AutoUpdate"]["enabled"];
                    }
                }

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
                GithubObjects.Releases.Assets app = goshimmer.GetReleasebyVersion("v0.2.1").assets.Where(x => x.name.Contains("cli-wallet_Windows")).ToList()[0];
                string download_url = app.browser_download_url;

                UpdateProgressBar.Visibility = Visibility.Visible;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += DownloadProgressChanged;
                    wc.DownloadFileCompleted += (sender, args) =>
                    {
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
                    FileName = "cli-wallet_Windows_x86_64.exe",
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
