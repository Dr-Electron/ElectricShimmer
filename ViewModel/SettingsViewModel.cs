using ElectricShimmer.ViewModel.Base;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ElectricShimmer.ViewModel
{
    public class SettingsViewModel : ViewModel
    {
        #region Properties
        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        public string Seed
        {
            get
            {
                try
                {
                    if (!IsPasswordVisible)
                        return null;
                    byte[] bytes = File.ReadAllBytes("wallet.dat");

                    List<byte> byteList = new List<byte>(bytes);
                    bytes = byteList.Take(32).ToArray();

                    return Base58.Encode(bytes);
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
            }
            set
            {
                byte[] bytes = Base58.Decode(value);
                File.WriteAllBytes("wallet.dat", bytes);
            }
        }

        public string NodeAddress
        {
            get
            {
                string text = File.ReadAllText("config.json");
                Config config = JsonConvert.DeserializeObject<Config>(text);
                return config.WebAPI;
            }
            set
            {
                Config config = new Config() { WebAPI = value };
                string text = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText("config.json", text);
            }
        }

        private bool _isIndicatorVisible;
        public bool IsIndicatorVisible
        {
            get => _isIndicatorVisible;
            set => SetProperty(ref _isIndicatorVisible, value);
        }
        #endregion

        public ICommand GenerateSeedCommand { get; set; }
        public void GenerateSeed(object obj)
        {
            try
            {
                IsPasswordVisible = false;
                IsIndicatorVisible = true;
                File.Delete("wallet.dat");

                ProcessStartInfo startinfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = cliWallet,
                    Arguments = "init",
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
                    if (error.Contains("wallet")) //should never happen, as wallet.dat gets deleted
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("wallet.dat was not deleted correctly." + Environment.NewLine + "Please delete it manually.", "wallet.dat not deleted correctly", MessageBoxButton.OK), "MainDialogHost");
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
                                GenerateSeed(null);
                        });
                    }
                    else if (error.Contains("synchronized"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Do you want to try again?", "Node not synchronized", MessageBoxButton.YesNo), "MainDialogHost");
                            if (result == MessageBoxResult.Yes)
                                GenerateSeed(null);
                        });
                    }
                    else if (error.Length <= 2)
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Your Seed was created without errors.", "Seed creation successful", MessageBoxButton.OK), "MainDialogHost");
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

        public ICommand GetServerStatusCommand { get; set; }
        public void GetServerStatus(object obj)
        {
            try
            {
                ProcessStartInfo startinfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = cliWallet,
                    Arguments = "server-status",
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
                                GenerateSeed(null);
                        });
                    }
                    else if (error.Contains("synchronized"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Do you want to try again?", "Node not synchronized", MessageBoxButton.YesNo), "MainDialogHost");
                            if (result == MessageBoxResult.Yes)
                                GenerateSeed(null);
                        });
                    }
                    else if (error.Length <= 2)
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            string[] lines = output.Split(new char[] {'\n','\r'}, StringSplitOptions.RemoveEmptyEntries);
                            string str = string.Join(Environment.NewLine, lines.Skip(1).ToArray());
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox(str, "Server Status", MessageBoxButton.OK), "MainDialogHost");
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

        public SettingsViewModel() : base() 
        {
            GenerateSeedCommand = new RelayCommand(GenerateSeed);
            GetServerStatusCommand = new RelayCommand(GetServerStatus);
        }
    }

    public class Config
    {
        public string WebAPI;
    }
}
