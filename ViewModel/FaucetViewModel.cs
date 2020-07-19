using ElectricShimmer.ViewModel.Base;
using MaterialDesignThemes.Wpf;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ElectricShimmer.ViewModel
{
    public class FaucetViewModel : ViewModel
    {
        private bool _isIndicatorVisible;
        public bool IsIndicatorVisible
        {
            get => _isIndicatorVisible;
            set => SetProperty(ref _isIndicatorVisible, value);
        }

        public ICommand RequestFaucetCommand { get; set; }
        public void RequestFaucet(object obj)
        {
            try
            {
                IsIndicatorVisible = true;

                ProcessStartInfo startinfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = "cli-wallet_Windows_x86_64.exe",
                    Arguments = "request-funds ",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                Process process = new Process() { StartInfo = startinfo };
                process.EnableRaisingEvents = true;
                string output = "";
                string error = "";
                process.OutputDataReceived += (sender, args) =>
                {
                    output += args.Data + Environment.NewLine;
                    Log.Write(args.Data, LogLevel.INFO);
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    error += args.Data + Environment.NewLine;
                    Log.Write(args.Data, LogLevel.ERROR);
                };
                process.Exited += (sender, args) =>
                {
                    IsIndicatorVisible = false;
                    process.Dispose();
                    if (error.Contains("wallet"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Please generate a seed in the settings tab first", "No seed found", MessageBoxButton.OK), "MainDialogHost");
                        });
                    }
                    else if (error.Contains("no such host"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Please check your node address.", "Host not found", MessageBoxButton.OK), "MainDialogHost");
                        });
                    }
                    else if (error.Contains("refused"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Please check your node address and if the node ports are configured correctly." + Environment.NewLine + "Do you want to try again?", "Host refused connection", MessageBoxButton.YesNo), "MainDialogHost");
                            if (result == MessageBoxResult.Yes)
                                RequestFaucet(null);
                        });
                    }
                    else if (error.Contains("synchronized"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Do you want to try again?", "Node not synchronized", MessageBoxButton.YesNo), "MainDialogHost");
                            if (result == MessageBoxResult.Yes)
                                RequestFaucet(null);
                        });
                    }
                    else if (error.Length <= 2) 
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Your faucet request was succesful." + Environment.NewLine + "It could take some time for your IOTAs to show up in your balance", "Request succesful", MessageBoxButton.OK), "MainDialogHost");
                        });
                    }
                    else //Unspecified error
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox(error, "Error", MessageBoxButton.OK), "MainDialogHost");
                        });
                    }
                };

                Log.Write(process.StartInfo.FileName + " " + process.StartInfo.Arguments, LogLevel.INFO);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception exc)
            {
                Log.Write(exc.Message, LogLevel.EXCEPTION);
            }
        }

        public FaucetViewModel() : base() 
        {
            RequestFaucetCommand = new RelayCommand(RequestFaucet);
        }
    }
}
