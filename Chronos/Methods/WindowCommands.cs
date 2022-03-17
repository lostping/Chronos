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
using Chronos.UserControls;
using Chronos.libs;
using Chronos.Classes;
using MahApps.Metro.Controls;
using Salaros.Configuration;

namespace Chronos
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private void WindowCommandSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsControl sec = new SettingsControl();
            Settings.Content = sec;

            Logger.Info(Properties.Resources.SettingsRead);

            Objects.ChronosSettings tts_read = Configuration.LoadSettings(portableConfFile);

            sec.DailyWork.Text = tts_read.DailyWorkString;
            sec.MaxDailyWork.Text = tts_read.MaxDailyWorkString;
            sec.SaveInterval.Text = tts_read.SaveIntervalString;
            sec.tog_start_with_windows.IsOn = tts_read.StartWithWindows;
            sec.tog_start_hidden.IsOn = tts_read.StartHidden;
            sec.tog_gohome.IsOn = tts_read.Reminder;
            sec.tog_sound.IsOn = tts_read.ReminderSound;
            sec.EndWorkReminderThreshold.Value = tts_read.ReminderThreshold;
            sec.EndWorkReminderInterval.Value = tts_read.ReminderInterval;
            sec.tog_hide_minimize.IsOn = tts_read.MinimizeOnClose;
            sec.tog_exportdefaultpath.IsOn = tts_read.CustomExport;
            sec.tb_exportfolder.Text = tts_read.CustomExportFolder;

            Settings.IsOpen = true;
        }

        private void WindowCommandHelp_Click(object sender, RoutedEventArgs e)
        {
            AboutControl abc = new AboutControl();
            About.Content = abc;
            About.IsOpen = true;
        }
    }
}
