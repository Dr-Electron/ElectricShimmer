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
    /// Interaction logic for MaterialMessageBox.xaml
    /// </summary>
    public partial class MaterialMessageBox : UserControl
    {
        public MaterialMessageBox(string messageBoxText, string caption, MessageBoxButton button)
        {
            InitializeComponent();

            lHeader.Content = caption;
            tNotification.Text = messageBoxText;

            ShowButttons(button);
        }

        private void ShowButttons(MessageBoxButton button)
        {
            if (button == MessageBoxButton.OK)
            {
                bOK.Visibility = Visibility.Visible;
                bYES.Visibility = Visibility.Collapsed;
                bNO.Visibility = Visibility.Collapsed;
                bCANCEL.Visibility = Visibility.Collapsed;
            }
            else if (button == MessageBoxButton.OKCancel)
            {
                bOK.Visibility = Visibility.Visible;
                bYES.Visibility = Visibility.Collapsed;
                bNO.Visibility = Visibility.Collapsed;
                bCANCEL.Visibility = Visibility.Visible;
            }
            else if (button == MessageBoxButton.YesNo)
            {
                bOK.Visibility = Visibility.Collapsed;
                bYES.Visibility = Visibility.Visible;
                bNO.Visibility = Visibility.Visible;
                bCANCEL.Visibility = Visibility.Collapsed;
            }
            else
            { //MessageBoxButton.YesNoCancel
                bOK.Visibility = Visibility.Collapsed;
                bYES.Visibility = Visibility.Visible;
                bNO.Visibility = Visibility.Visible;
                bCANCEL.Visibility = Visibility.Visible;
            }
        }
    }
}
