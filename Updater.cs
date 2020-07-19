using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace ElectricShimmer
{
    class Updater
    {
        private static Github electricShimmer = new Github("https://api.github.com/repos/Dr-Electron/ElectricShimmer/releases");
        public static bool CheckUpdate()
        {
            try
            {
                Version Local_Version = Assembly.GetExecutingAssembly().GetName().Version;
                Version Github_Version = Version.Parse(electricShimmer.GetReleasebyVersion().tag_name.Remove(0, 1));

                if (Github_Version.CompareTo(Local_Version) > 0)
                    return true;
                return false;
            }
            catch (Exception exc)
            {
                Log.Write(exc.Message, LogLevel.EXCEPTION);
                return false;
            }
        }

        public static void Update()
        {
            try
            {
                GithubObjects.Releases.Assets app = electricShimmer.GetReleasebyVersion().assets.Where(x => x.name.Contains(".exe")).ToList()[0];
                string download_url = app.browser_download_url;

                Directory.CreateDirectory("temp");

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += ((MainWindow)App.Current.MainWindow).DownloadProgressChanged;
                    wc.DownloadFileCompleted += ((MainWindow)App.Current.MainWindow).DownloadFinished;
                    wc.DownloadFileAsync(
                        new Uri(download_url),
                        @"temp\" + app.name
                    );
                }
            }
            catch (Exception exc)
            {
                Log.Write(exc.Message, LogLevel.EXCEPTION);
            }
        }

        public static void FinishingUpdate()
        {
            try
            {
                // Change the currently running executable so it can be overwritten.
                Process thisprocess = Process.GetCurrentProcess();
                string me = thisprocess.MainModule.FileName;
                string bak = me + ".bak";
                if (File.Exists(bak))
                    File.Delete(bak);
                File.Move(me, bak);
                File.Copy(bak, me);

                var directory = new DirectoryInfo("temp");
                var files = directory.GetFiles("*.exe", SearchOption.AllDirectories);
                string destination = files[0].FullName.Replace(directory.FullName + @"\", "");
                files[0].CopyTo(destination, true);

                // Clean up.
                Directory.Delete("temp", true);

                // Restart.
                Process.Start(me);
                thisprocess.CloseMainWindow();
                thisprocess.Close();
                thisprocess.Dispose();
            }
            catch (Exception exc)
            {
                Log.Write(exc.Message, LogLevel.EXCEPTION);
            }
        }
    }
}
