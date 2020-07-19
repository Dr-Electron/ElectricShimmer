using System.Windows;

namespace ElectricShimmer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Write(e.Exception.Message, LogLevel.EXCEPTION);
        }
    }
}
