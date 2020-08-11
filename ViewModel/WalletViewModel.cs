using ElectricShimmer.Controls;
using ElectricShimmer.ViewModel.Base;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ElectricShimmer.ViewModel
{
    class WalletViewModel : ViewModel
    {
        #region Properties
        private int _selectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                SetProperty(ref _selectedTabIndex, value);
                if (_selectedTabIndex == 0)
                    UpdateBalance();
                else if (_selectedTabIndex == 1)
                    UpdateAddresses();
            }
        }

        private BalanceInfo _selectedBalance;
        public BalanceInfo SelectedBalance
        {
            get => _selectedBalance;
            set
            {
                SetProperty(ref _selectedBalance, value);
                SendColor = _selectedBalance == null ? "IOTA" : _selectedBalance.Color;
            }
        }

        private long _balance;
        public long Balance 
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        #region Assets
        public int AssetAmount { get; set; }
        public string AssetName { get; set; }
        public string AssetSymbol { get; set; }

        private bool _isCreateIndicatorVisible; 
        public bool IsCreateIndicatorVisible 
        {
            get => _isCreateIndicatorVisible;
            set => SetProperty(ref _isCreateIndicatorVisible, value);
        }
        #endregion

        #region Send
        public int SendAmount { get; set; }

        private string _sendColor = "IOTA";
        public string SendColor 
        {
            get => _sendColor;
            set => SetProperty(ref _sendColor, value);
        }
        public string SendAddress { get; set; }

        private bool _isSendIndicatorVisible;
        public bool IsSendIndicatorVisible
        {
            get => _isSendIndicatorVisible;
            set => SetProperty(ref _isSendIndicatorVisible, value);
        }
        #endregion

        private List<BalanceInfo> _balanceList;
        public List<BalanceInfo> BalanceList
        {
            get => _balanceList;
            set => SetProperty(ref _balanceList, value);
        }

        private List<AddressInfo> _addressList;
        public List<AddressInfo> AddressList
        {
            get => _addressList;
            set => SetProperty(ref _addressList, value);
        }
#endregion

        #region Commands & Methods
        public ICommand CreateAssetCommand { get; set; }
        public void CreateAsset(object obj)
        {
            try
            {
                IsCreateIndicatorVisible = true;

                ProcessStartInfo startinfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = cliWallet,
                    Arguments = "create-asset -amount " + AssetAmount + " -name " + AssetName + " -symbol " + AssetSymbol,
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
                    IsCreateIndicatorVisible = false;
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
                                CreateAsset(null);
                        });
                    }
                    else if (error.Contains("synchronized"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Do you want to try again?", "Node not synchronized", MessageBoxButton.YesNo), "MainDialogHost");
                            if (result == MessageBoxResult.Yes)
                                CreateAsset(null);
                        });
                    }
                    else if (error.Length <= 2)
                    {
                        UpdateBalance();
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Creation of Assets was succesful", "Created Assets", MessageBoxButton.OK), "MainDialogHost");
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

        public ICommand ReceiveCommand { get; set; }
        public void Receive(object obj)
        {
            try
            {
                ProcessStartInfo startinfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = cliWallet,
                    Arguments = "address -new",
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
                            DialogHost.CloseDialogCommand.Execute(null, null);
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Please generate a seed in the settings tab first", "No seed found", MessageBoxButton.OK), "MainDialogHost");
                        });
                    }
                    else if (error.Contains("no such host"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            DialogHost.CloseDialogCommand.Execute(null, null);
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Please check your node address.", "Host not found", MessageBoxButton.OK), "MainDialogHost");
                        });
                    }
                    else if (error.Contains("refused"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            DialogHost.CloseDialogCommand.Execute(null, null);
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Please check your node address and if the node ports are configured correctly." + Environment.NewLine + "Do you want to try again?", "Host refused connection", MessageBoxButton.YesNo), "MainDialogHost");
                            if (result == MessageBoxResult.Yes)
                                Receive(null);
                        });
                    }
                    else if (error.Contains("synchronized"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            DialogHost.CloseDialogCommand.Execute(null, null);
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Do you want to try again?", "Node not synchronized", MessageBoxButton.YesNo), "MainDialogHost");
                            if (result == MessageBoxResult.Yes)
                                Receive(null);
                        });
                    }
                    else if (error.Length <= 2)
                    {
                        string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        string address = lines[1].Split(' ')[3];
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            await DialogHost.Show(new ReceiveAddress(address), "MainDialogHost");
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

        public ICommand SendCommand { get; set; }
        public void Send(object obj)
        {
            try
            {

                IsSendIndicatorVisible = true;

                ProcessStartInfo startinfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = cliWallet,
                    Arguments = "send-funds -dest-addr " + SendAddress + " -color " + SendColor + " -amount " + SendAmount,
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
                    IsSendIndicatorVisible = false;
                    process.Dispose();
                    if (error.Contains("wallet"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            DialogHost.CloseDialogCommand.Execute(null, null);
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Please generate a seed in the settings tab first", "No seed found", MessageBoxButton.OK), "MainDialogHost");
                        });
                    }
                    else if (error.Contains("no such host"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            DialogHost.CloseDialogCommand.Execute(null, null);
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Please check your node address.", "Host not found", MessageBoxButton.OK), "MainDialogHost");
                        });
                    }
                    else if (error.Contains("refused"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            DialogHost.CloseDialogCommand.Execute(null, null);
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Please check your node address and if the node ports are configured correctly." + Environment.NewLine + "Do you want to try again?", "Host refused connection", MessageBoxButton.YesNo), "MainDialogHost");
                            if (result == MessageBoxResult.Yes)
                                Send(null);
                        });
                    }
                    else if (error.Contains("synchronized"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            DialogHost.CloseDialogCommand.Execute(null, null);
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Do you want to try again?", "Node not synchronized", MessageBoxButton.YesNo), "MainDialogHost");
                            if (result == MessageBoxResult.Yes)
                                Send(null);
                        });
                    }
                    else if (error.Length <= 2)
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            DialogHost.CloseDialogCommand.Execute(null, null);
                            UpdateBalance();
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Funds where send successfully", "Sending successful", MessageBoxButton.OK), "MainDialogHost");
                        });
                    }
                    else //Unspecified error
                {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            DialogHost.CloseDialogCommand.Execute(null, null);
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

        public void UpdateAddresses()
        {
            try
            {
                ProcessStartInfo startinfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = cliWallet,
                    Arguments = "address -list",
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
                                UpdateAddresses();
                        });
                    }
                    else if (error.Contains("synchronized"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Do you want to try again?", "Node not synchronized", MessageBoxButton.YesNo), "MainDialogHost");
                            if (result == MessageBoxResult.Yes)
                                UpdateAddresses();
                        });
                    }
                    else if (error.Length <= 2)
                    {
                        string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        List<AddressInfo> addressList = new List<AddressInfo>();
                        for (int i = 3; i < lines.Length; i++)
                        {
                            string[] cellContent = lines[i].Split('\t');
                            addressList.Add(new AddressInfo(
                                Int64.Parse(cellContent[0]),
                                cellContent[1],
                                Boolean.Parse(cellContent[2]))
                            );
                        }
                        AddressList = addressList;
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

        public void UpdateBalance()
        {
            try
            {
                ProcessStartInfo startinfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = cliWallet,
                    Arguments = "balance",
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
                                UpdateBalance();
                        });
                    }
                    else if (error.Contains("synchronized"))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            MessageBoxResult result = (MessageBoxResult)await DialogHost.Show(new Controls.MaterialMessageBox("Do you want to try again?", "Node not synchronized", MessageBoxButton.YesNo), "MainDialogHost");
                            if (result == MessageBoxResult.Yes)
                                UpdateBalance();
                        });
                    }
                    else if (error.Length <= 2)
                    {
                        string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        List<BalanceInfo> balanceList = new List<BalanceInfo>();
                        Balance = 0;
                        for (int i = 3; i < lines.Length; i++)
                        {
                            string[] cellContent = lines[i].Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                            if (cellContent[0] == "<EMPTY>")
                            {
                                BalanceList = null;
                                break;
                            }
                            long tokenBalance = Int64.Parse(cellContent[1]);
                            Balance += tokenBalance;
                            balanceList.Add(new BalanceInfo(
                                cellContent[0] == "[ OK ]" ? true : false,
                                tokenBalance,
                                cellContent[2],
                                cellContent[3])
                            );
                        }
                        BalanceList = balanceList;
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
        #endregion

        public WalletViewModel() : base()
        {
            ReceiveCommand = new RelayCommand(Receive);
            CreateAssetCommand = new RelayCommand(CreateAsset);
            SendCommand = new RelayCommand(Send);
            UpdateBalance();
        }
    }

    public class AddressInfo
    {
        public long Index { get; set; }
        public string Address { get; set; }
        public bool Spent { get; set; }

        public AddressInfo(long index, string address, bool spent)
        {
            Index = index;
            Address = address;
            Spent = spent;
        }
    }

    public class BalanceInfo
    {
        public bool Status { get; set; }
        public long Balance { get; set; }
        public string Color { get; set; }
        public string Token { get; set; }

        public BalanceInfo(bool status, long balance, string color, string token )
        {
            Status = status;
            Balance = balance;
            Color = color;
            Token = token;
        }
    }
}
