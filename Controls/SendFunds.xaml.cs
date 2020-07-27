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

namespace ElectricShimmer.Controls
{
    /// <summary>
    /// Interaction logic for SendFunds.xaml
    /// </summary>
    public partial class SendFunds : UserControl
    {
        public static readonly DependencyProperty AmountProperty = DependencyProperty.Register("Amount", typeof(string), typeof(SendFunds),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string Amount
        {
            get => (string)GetValue(AmountProperty);
            set => SetValue(AmountProperty, value);
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(string), typeof(SendFunds),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string Color
        {
            get => (string)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public static readonly DependencyProperty AddressProperty = DependencyProperty.Register("Address", typeof(string), typeof(SendFunds), 
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string Address
        {
            get => (string)GetValue(AddressProperty);
            set => SetValue(AddressProperty, value);
        }

        public static readonly DependencyProperty IsIndicatorVisibleProperty = DependencyProperty.Register("IsIndicatorVisible", typeof(bool), typeof(SendFunds));
        public bool IsIndicatorVisible
        {
            get => (bool)GetValue(IsIndicatorVisibleProperty);
            set => SetValue(IsIndicatorVisibleProperty, value);
        }

        public static readonly DependencyProperty SendCommandProperty = DependencyProperty.Register("SendCommand", typeof(ICommand), typeof(SendFunds));
        public ICommand SendCommand
        {
            get => (ICommand)GetValue(SendCommandProperty);
            set => SetValue(SendCommandProperty, value);
        }

        public SendFunds()
        {
            InitializeComponent();
        }
    }
}
