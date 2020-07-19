﻿using System;
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

namespace ElectricShimmer.Controls
{
    /// <summary>
    /// Interaction logic for ReceiveAddress.xaml
    /// </summary>
    public partial class ReceiveAddress : UserControl
    {
        public ReceiveAddress(string address)
        {
            InitializeComponent();
            tAddress.Text = address;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tAddress.Text);
        }
    }
}
