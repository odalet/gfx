using System;
using System.IO;

namespace ScanPlayerWpf
{
    internal static class Constants
    {
        public static string AppName => "ScanPlayer";

        public static string CompanyName => "AddUp";

        public static string AppDataFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), CompanyName, AppName);

        public static string UserDataFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), CompanyName, AppName);
    }
}
