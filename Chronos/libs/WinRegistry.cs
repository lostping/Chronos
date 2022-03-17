using System;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using NLog;

namespace Chronos.libs
{
    class WinRegistry
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        private string appName = Assembly.GetExecutingAssembly().GetName().Name;
        private string appPath = Assembly.GetExecutingAssembly().Location + " -startup";

        public bool RegisterApp()
        {
            Logger.Info(Properties.Resources.RegisterInfo);
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.SetValue(appName, "\"" + appPath + "\"");
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal(Properties.Resources.RegisterFail + ": " + ex.Message);
                return false;
            }
        }

        public bool DeRegisterApp()
        {
            Logger.Info(Properties.Resources.RegisterRemove);
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.DeleteValue(appName, false);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal(Properties.Resources.RegisterRemoveFail + ": " + ex.Message);
                return false;
            }
        }
    }
}
