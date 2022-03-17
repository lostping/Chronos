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
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using ToastNotifications.Lifetime.Clear;
using AudioPlayerLib;
using WaveResources;
using Chronos.libs;
using Chronos.Classes;
using NLog;
using Salaros.Configuration;
using MahApps.Metro.SimpleChildWindow;

namespace Chronos.UserControls
{
    /// <summary>
    /// Interaktionslogik für SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        public static string portableConfFile = "Chronos.settings.ini";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly AudioPlayer ToastPlayerWarn = new AudioPlayer(WaveResourceAssembly.Assembly, "WaveResources", "warn_siren.mp3");
        private static readonly AudioPlayer ToastPlayerCrit = new AudioPlayer(WaveResourceAssembly.Assembly, "WaveResources", "crit_meltdown.mp3");

        public SettingsControl()
        {
            InitializeComponent();
            if (Helper.IsPortable())
            {
                tog_portable.IsOn = true;
            }
            else
            {
                tog_portable.IsOn = false;
            }
        }

        private void SaveSettings()
        {
            try
            {
                MainWindow mw = Application.Current.MainWindow as MainWindow;
                Logger.Info(Properties.Resources.SettingsSave);
                if (Helper.IsPortable())
                {
                    Logger.Info(Properties.Resources.PortableMode);
                    var conf = new ConfigParser(portableConfFile);
                    conf.SetValue("Work", "SaveInterval", SaveInterval.Value.ToString());
                    conf.SetValue("Work", "DailyWork", DailyWork.Value.ToString());
                    conf.SetValue("Work", "MaxDailyWork", MaxDailyWork.Value.ToString());
                    conf.SetValue("App", "StartWithWindows", tog_start_with_windows.IsOn.ToString());
                    conf.SetValue("App", "StartHidden", tog_start_hidden.IsOn.ToString());
                    conf.SetValue("App", "Reminder", tog_gohome.IsOn.ToString());
                    conf.SetValue("App", "ReminderThreshold", EndWorkReminderThreshold.Value.ToString());
                    conf.SetValue("App", "EndWorkReminderInterval", EndWorkReminderInterval.Value.ToString());
                    conf.SetValue("App", "ReminderSound", tog_sound.IsOn.ToString());
                    conf.SetValue("App", "MinimizeOnClose", tog_hide_minimize.IsOn.ToString());
                    conf.SetValue("App", "CustomExport", tog_exportdefaultpath.IsOn.ToString());
                    conf.SetValue("App", "CustomExportFolder", tb_exportfolder.Text);
                    conf.Save(portableConfFile);
                }
                else
                {
                    Logger.Info(Properties.Resources.PortableModeOff);
                    Properties.Settings.Default.DailyWork = (float)DailyWork.Value;
                    Properties.Settings.Default.MaxDailyWork = (float)MaxDailyWork.Value;
                    Properties.Settings.Default.SaveInterval = (int)SaveInterval.Value;
                    Properties.Settings.Default.StartWithWindows = tog_start_with_windows.IsOn;
                    Properties.Settings.Default.StartHidden = tog_start_hidden.IsOn;
                    Properties.Settings.Default.MinimizeOnClose = tog_hide_minimize.IsOn;
                    Properties.Settings.Default.Reminder = tog_gohome.IsOn;
                    Properties.Settings.Default.ReminderSound = tog_sound.IsOn;
                    Properties.Settings.Default.ReminderThreshold = (double)EndWorkReminderThreshold.Value;
                    Properties.Settings.Default.EndWorkReminderInterval = (int)EndWorkReminderInterval.Value;
                    Properties.Settings.Default.CustomExport = tog_exportdefaultpath.IsOn;
                    Properties.Settings.Default.CustomExportFolder = tb_exportfolder.Text;
                    Properties.Settings.Default.Save();
                    Logger.Info(Properties.Resources.SettingsSaveSuccess);
                }
                mw.Settings.IsOpen = false;
            }
            catch (Exception ex)
            {
                Logger.Fatal(Properties.Resources.SettingsSaveFail + ex.Message);
            }
        }

        private void Tog_start_with_windows_Toggled(object sender, RoutedEventArgs e)
        {
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            if (tog_start_with_windows.IsOn)
            {
                Helper.CreateAutostartShortcut("Chronos", appPath);
            }
            else
            {
                Helper.DeleteAutostartShortcut("Chronos");
            }
        }

        private void Tog_gohome_Toggled(object sender, RoutedEventArgs e)
        {
            if (tog_gohome.IsOn)
            {
                EndWorkReminderInterval.IsEnabled = true;
                EndWorkReminderThreshold.IsEnabled = true;
            }
            else
            {
                EndWorkReminderInterval.IsEnabled = false;
                EndWorkReminderThreshold.IsEnabled = false;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void Toggle_toast_Click(object sender, RoutedEventArgs e)
        {
            if(toggle_toast.IsChecked.Value)
            {
                MainWindow.GoHomeNotifier.ShowWarning(Properties.Resources.ToastTestWarn);
                MainWindow.GoHomeNotifier.ShowError(Properties.Resources.ToastTestCrit);
            }
            else
            {
                MainWindow.GoHomeNotifier.ClearMessages(new ClearAll());
            }
        }

        private void Toggle_sound_warn_Click(object sender, RoutedEventArgs e)
        {
            if (toggle_sound_warn.IsChecked.Value)
            {
                ToastPlayerWarn.Play();
            }
            else
            {
                ToastPlayerWarn.Stop();
            }
        }

        private void Toggle_sound_crit_Click(object sender, RoutedEventArgs e)
        {
            if (toggle_sound_crit.IsChecked.Value)
            {
                ToastPlayerCrit.Play();
            }
            else
            {
                ToastPlayerCrit.Stop();
            }
        }

        private void Tog_exportdefaultpath_Toggled(object sender, RoutedEventArgs e)
        {
            if (tog_exportdefaultpath.IsOn)
            {
                tb_exportfolder.IsEnabled = true;
                bt_exportfolder.IsEnabled = true;
                Save.IsEnabled = false;
            }
            else
            {
                tb_exportfolder.IsEnabled = false;
                bt_exportfolder.IsEnabled = false;
                Save.IsEnabled = true;
            }
        }

        private void Bt_exportfolder_Click(object sender, RoutedEventArgs e)
        {
            string exportPath = Helper.BrowseFolder();
            if (exportPath.Length > 0)
            {
                Save.IsEnabled = true;
            } 
            else
            {
                Save.IsEnabled = false;
            }
            tb_exportfolder.Text = exportPath;
        }

        private void Tog_portable_Toggled(object sender, RoutedEventArgs e)
        {
            bool success = false;
            if (tog_portable.IsOn && (tog_portable.IsMouseOver || tog_portable.IsFocused))
            {
                success = Helper.MakePortable();
                SaveSettings();
                if (success)
                {
                    MainWindow.GoHomeNotifier.ShowInformation(Properties.Resources.PortableSuccess);
                }
                else
                {
                    MainWindow.GoHomeNotifier.ShowError(Properties.Resources.PortableFail);
                }
            }
            else if(!tog_portable.IsOn && (tog_portable.IsMouseOver || tog_portable.IsFocused))
            {
                success = Helper.UnMakePortable();
                SaveSettings();
                if (success)
                {
                    MainWindow.GoHomeNotifier.ShowInformation(Properties.Resources.PortableUndoSuccess);
                }
                else
                {
                    MainWindow.GoHomeNotifier.ShowError(Properties.Resources.PortableUndoFail);
                }
            }
            if (success)
            {
                MainWindow.GoHomeNotifier.ShowWarning(Properties.Resources.RestartApplication);
            }
        }
    }
}
