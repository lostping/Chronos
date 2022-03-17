using System;
using System.IO;
using System.Reflection;
using System.Windows;
using MahApps.Metro.Controls;
using Salaros.Configuration;
using Chronos.libs;
using Chronos.Classes;
using NLog;


namespace Chronos.libs
{
    class Configuration
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public static Objects.ChronosSettings LoadSettings(string portableConfFile)
        {
            Objects.ChronosSettings tts_ret = new Objects.ChronosSettings();

            Logger.Info(Properties.Resources.SettingsRead);
            if (Helper.IsPortable())
            {
                Logger.Info(Properties.Resources.PortableMode);
                string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (File.Exists(Path.Combine(exePath, portableConfFile)))
                {
                    try
                    {
                        var conf = new ConfigParser(portableConfFile);
                        tts_ret.SaveInterval = int.Parse(conf.GetValue("Work", "SaveInterval"));
                        tts_ret.DailyWork = float.Parse(conf.GetValue("Work", "DailyWork"));
                        tts_ret.MaxDailyWork = float.Parse(conf.GetValue("Work", "MaxDailyWork"));
                        tts_ret.StartWithWindows = bool.Parse(conf.GetValue("App", "StartWithWindows"));
                        tts_ret.StartHidden = bool.Parse(conf.GetValue("App", "StartHidden"));
                        tts_ret.Reminder = bool.Parse(conf.GetValue("App", "Reminder"));
                        tts_ret.ReminderThreshold = double.Parse(conf.GetValue("App", "ReminderThreshold"));
                        tts_ret.ReminderInterval = int.Parse(conf.GetValue("App", "EndWorkReminderInterval"));
                        tts_ret.ReminderSound = bool.Parse(conf.GetValue("App", "ReminderSound"));
                        tts_ret.MinimizeOnClose = bool.Parse(conf.GetValue("App", "MinimizeOnClose"));
                        tts_ret.CustomExport = bool.Parse(conf.GetValue("App", "CustomExport"));
                        tts_ret.CustomExportFolder = conf.GetValue("App", "CustomExportFolder");
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal("Failed to parse portable configuration: " + ex.Message);
                    }
                }
            }
            else
            {
                Logger.Info(Properties.Resources.PortableModeOff);
                try
                {
                    tts_ret.SaveInterval = Properties.Settings.Default.SaveInterval;
                    tts_ret.DailyWork = Properties.Settings.Default.DailyWork;
                    tts_ret.MaxDailyWork = Properties.Settings.Default.MaxDailyWork;
                    tts_ret.StartWithWindows = Properties.Settings.Default.StartWithWindows;
                    tts_ret.StartHidden = Properties.Settings.Default.StartHidden;
                    tts_ret.Reminder = Properties.Settings.Default.Reminder;
                    tts_ret.ReminderThreshold = Properties.Settings.Default.ReminderThreshold;
                    tts_ret.ReminderInterval = Properties.Settings.Default.EndWorkReminderInterval;
                    tts_ret.ReminderSound = Properties.Settings.Default.ReminderSound;
                    tts_ret.MinimizeOnClose = Properties.Settings.Default.MinimizeOnClose;
                    tts_ret.CustomExport = Properties.Settings.Default.CustomExport;
                    tts_ret.CustomExportFolder = Properties.Settings.Default.CustomExportFolder;
                }
                catch (Exception ex)
                {
                    Logger.Fatal(Properties.Resources.SettingsReadFail + ex.Message);
                }
            }
            return tts_ret;
        }
    }
}
