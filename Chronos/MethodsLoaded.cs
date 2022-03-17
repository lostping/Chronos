using System;
using System.Globalization;
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
using MahApps.Metro.Controls;
using FluentDate;
using FluentDateTime;
using Nager.Date;
using Nager.Date.Model;
using FluentScheduler;
using Chronos.Classes;
using Chronos.libs;

namespace Chronos
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private void ChronosWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Load settings and Scheduler
            try
            {
                tts = Configuration.LoadSettings(portableConfFile);
                tb_soll.Text = tts.DailyWorkString;
                LoadDataToDatagrid();

                Logger.Info(String.Format("{0}: {1}", Properties.Resources.JobInitMinutes, tts.SaveInterval));
                JobManager.Initialize(new JobRegistry(tts));
            }
            catch (Exception ex)
            {
                Logger.Fatal(Properties.Resources.JobInitFailed + " Exception: " + ex.Message);
            }
            if (tts.StartHidden)
            {
                this.Visibility = Visibility.Collapsed;
                tray_minmax.Header = Properties.Resources.WindowShow;
            }

            Logger.Info(Properties.Resources.HolidayCalculating);
            try
            {
                string lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                Logger.Info(String.Format("{0}: {1}", Properties.Resources.CountryDetected, lang));
                var endofYear = new DateTime(DateTime.Now.AddYears(1).Year, 1, 1);
                IEnumerable<PublicHoliday> publicHolidays = DateSystem.GetPublicHolidays(DateTime.Now, endofYear, lang);
                var pubdays = publicHolidays.Where(x => x.Counties == null).Select(x => new Objects.HolyDay { Date = x.Date, Holiday = x.LocalName }).ToList();
                dg_holidays.ItemsSource = pubdays;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                // fallback to usa
                Logger.Info(String.Format("{0}: {1}", Properties.Resources.CountryDetectFail, "US"));
                var endofYear = new DateTime(DateTime.Now.AddYears(1).Year, 1, 1);
                IEnumerable<PublicHoliday> publicHolidays = DateSystem.GetPublicHolidays(DateTime.Now, endofYear, CountryCode.US);
                var pubdays = publicHolidays.Where(x => x.Counties == null).Select(x => new Objects.HolyDay { Date = x.Date, Holiday = x.LocalName }).ToList();
                dg_holidays.ItemsSource = pubdays;
            }
        }
    }
}
