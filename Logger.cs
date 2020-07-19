using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ElectricShimmer
{
    public enum LogLevel
    {
        ALL,
        INFO,
        ERROR,
        EXCEPTION,
        OFF
    }
    class Log
    {
        public static LogLevel Level = LogLevel.ALL;

        private static Mutex mutex = new Mutex(false, "Logger");

        public static void Write(string message, LogLevel logLevel, [CallerMemberName] string callerMemberName = "")
        {
            try
            {
                if (!String.IsNullOrEmpty(message) && logLevel >= Level)
                {
                    mutex.WaitOne(1000);
                    DateTime DTNow = DateTime.Now;

                    string Date = DTNow.ToString("yyyy-MM-dd");
                    bool FileExists = false;

                    if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Logger\"))
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"Logger\");

                    FileExists = File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Logger\ElectricShimmer.log");

                    StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"Logger\ElectricShimmer.log", FileExists);
                    sw.WriteLine(DTNow.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + logLevel.ToString() + "\t" + callerMemberName + "\t" + message);
                    sw.Close();
                    mutex.ReleaseMutex();
                }
            }
            catch (Exception exc)
            {
                Write(exc.Message, LogLevel.EXCEPTION);
            }
        }
    }
}
