using System;
using System.IO;
using Tommy;

namespace ElectricShimmer
{
    static class Config
    {
        public static void Init()
        {
            if (File.Exists(configFile))
            {
                using (StreamReader reader = new StreamReader(File.OpenRead(configFile)))
                {
                    // Parse the table
                    TomlTable table = TOML.Parse(reader);

                    if (table.HasKey("Logger"))
                        LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), table["Logger"]["LogLevel"]);

                    if (table.HasKey("AutoUpdate"))
                        IsUpdateEnabled = table["AutoUpdate"]["enabled"];

                    if (table.HasKey("cli-wallet"))
                        CliExecName = table["cli-wallet"]["Name"];
                }
            }
        }

        private static string configFile = "config.toml";

        public static LogLevel LogLevel { get; private set; } = LogLevel.ALL;
        public static bool IsUpdateEnabled { get; private set; } = true;
        public static string CliExecName { get; private set; } = "cli-wallet.exe";
    }
}
