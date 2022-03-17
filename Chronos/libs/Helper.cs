using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;
using Ookii.Dialogs.Wpf;
using IWshRuntimeLibrary;
using System.Reflection;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NLog;

namespace Chronos.libs
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime StartOfLastWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 14;
            return dt.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Get all chosen days of a month
        /// </summary>
        /// <param name="startDate">date to start from</param>
        /// <param name="desiredDayOfWeek">day of week to get from month</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> GetDaysOfWeek(DateTime startDate, DayOfWeek desiredDayOfWeek)
        {
            var daysOfWeek = new List<DateTime>();
            var workingDate = new DateTime(startDate.Year, startDate.Month, 1);
            var offset = ((int)desiredDayOfWeek - (int)workingDate.DayOfWeek + 7) % 7;

            // Jump to the first desired day of week.
            workingDate = workingDate.AddDays(offset);

            do
            {
                daysOfWeek.Add(workingDate);

                // Jump forward seven days to get the next desired day of week.
                workingDate = workingDate.AddDays(7);
            } while (workingDate.Month == startDate.Month);

            return daysOfWeek;
        }

        public static DateTime FirstDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        public static int DaysInMonth(this DateTime value)
        {
            return DateTime.DaysInMonth(value.Year, value.Month);
        }

        public static DateTime LastDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.DaysInMonth());
        }

        /// <summary>
        /// Get all days between two datetimes
        /// </summary>
        /// <param name="startDate">DateTime to start</param>
        /// <param name="endDate">DateTime to end</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> GetDatesBetween(DateTime startDate, DateTime endDate)
        {
            IEnumerable<DateTime> allDates = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                      .Select(offset => startDate.AddDays(offset))
                      .ToArray();
            return allDates;
        }

        /// <summary>
        /// Get all days between two datetimes - only business days without weekend
        /// </summary>
        /// <param name="startDate">DateTime to start</param>
        /// <param name="endDate">DateTime to end</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> GetWorkdatesBetween(DateTime startDate, DateTime endDate)
        {
            IEnumerable<DateTime> allDates = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                      .Select(offset => startDate.AddDays(offset))
                      .Where(date => date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                      .ToArray();
            return allDates;
        }

        /// <summary>
        /// Get all days between two datetimes with specific format
        /// </summary>
        /// <param name="startDate">DateTime to start</param>
        /// <param name="endDate">DateTime to end</param>
        /// <param name="format">Date format to output</param>
        /// <returns></returns>
        public static List<string> GetDatesBetweenAsString(DateTime startDate, DateTime endDate, String format)
        {
            IEnumerable<DateTime> allDates = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                      .Select(offset => startDate.AddDays(offset))
                      .ToArray();

            List<string> daysOutput = new List<string>();

            foreach (DateTime day in allDates)
            {
                string str = day.ToString(format);
                daysOutput.Add(str);
            }

            return daysOutput;
        }
    }

    class Helper
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public static string GetDbPath()
        {
            string dbPath;
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            string localAppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (Helper.IsPortable())
            {
                dbPath = Path.Combine(exePath, "db", appName + ".sqlite");
            }
            else
            {
                dbPath = Path.Combine(localAppdataPath, appName, appName + ".sqlite");
            }
            return dbPath;
        }

        public static string SettingsIniPath()
        {
            string appPath;
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            appPath = Path.Combine(exePath, appName + ".settings.ini");
            return appPath;
        }

        public static bool MakePortable()
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);

            string appdataDbPath = GetDbPath();
            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string portablePath = Path.Combine(exePath, "PORTABLE");
            string portableDbFolder = Path.Combine(exePath, "db");
            try
            {
                if (Directory.Exists(portableDbFolder))
                {
                    // remove already existing portable files
                    Directory.Delete(portableDbFolder, true);
                    // recreate db folder
                    Directory.CreateDirectory(portableDbFolder);
                }
                else
                {
                    Directory.CreateDirectory(portableDbFolder);
                }

                // create portable marker file
                System.IO.File.Create(portablePath).Dispose();

                // now move database from local app data to program path
                string portableDbPath = GetDbPath();
                System.IO.File.Move(appdataDbPath, portableDbPath);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                _ = metroWindow.ShowMessageAsync("Error", ex.Message);
                return false;
            }
        }

        public static bool UnMakePortable()
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);

            string portableDbPath = GetDbPath();
            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string portablePath = Path.Combine(exePath, "PORTABLE");
            string portableDbFolder = Path.Combine(exePath, "db");

            try
            {
                // remove portable files
                System.IO.File.Delete(portablePath);
                System.IO.File.Delete(SettingsIniPath());

                // now move database from program path to local app data
                string localAppDbPath = GetDbPath();
                System.IO.File.Move(portableDbPath, localAppDbPath);

                // remove portable db folder
                Directory.Delete(portableDbFolder);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                _ = metroWindow.ShowMessageAsync("Error", ex.Message);
                return false;
            }
        }

        public static bool IsPortable()
        {
            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (System.IO.File.Exists(Path.Combine(exePath, "PORTABLE")))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void CreateAutostartShortcut(string shortcutName, string targetFileLocation)
        {
            string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.Description = Properties.Resources.ChronosDesc;
            shortcut.IconLocation = targetFileLocation + ", 0";
            shortcut.TargetPath = targetFileLocation;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetFileLocation);
            shortcut.Save();
        }

        public static void DeleteAutostartShortcut(string shortcutName)
        {
            string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            System.IO.File.Delete(shortcutLocation);
        }

        public static List<string> GetDateRange(DateTime StartingDate, DateTime EndingDate)
        {
            if (StartingDate > EndingDate)
            {
                return null;
            }
            List<string> rst = new List<string>();
            List<DateTime> rv = new List<DateTime>();
            DateTime tmpDate = StartingDate;
            do
            {
                rv.Add(tmpDate);
                rst.Add(tmpDate.ToString("yyyy-MM-dd"));
                tmpDate = tmpDate.AddDays(1);
            } while (tmpDate <= EndingDate);
            return rst;
        }

        public static String BrowseFolder(Environment.SpecialFolder initialDirectory=Environment.SpecialFolder.MyDocuments)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.RootFolder = initialDirectory;
            dialog.ShowDialog();

            return dialog.SelectedPath;
        }
    }
}
