using System;
using System.Windows;
using System.Windows.Controls;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using MahApps.Metro.Controls;
using AudioPlayerLib;
using WaveResources;
using Chronos.Properties;

namespace Chronos
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static int NotificationCounter = 1;
        private static readonly AudioPlayer ToastPlayerWarn = new AudioPlayer(WaveResourceAssembly.Assembly, "WaveResources", "warn_siren.mp3");
        private static readonly AudioPlayer ToastPlayerCrit = new AudioPlayer(WaveResourceAssembly.Assembly, "WaveResources", "crit_meltdown.mp3");

        // toast notifications
        public static Notifier GoHomeNotifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new PrimaryScreenPositionProvider(
                corner: Corner.TopRight,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new CountBasedLifetimeSupervisor(
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.DisplayOptions.TopMost = true;

            cfg.Dispatcher = Application.Current.Dispatcher;

        });

        public static Notifier TrayNotifier = new Notifier(cfg => {
            cfg.PositionProvider = new PrimaryScreenPositionProvider(
                corner: Corner.BottomRight,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(5),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.DisplayOptions.TopMost = true;

            cfg.Dispatcher = Application.Current.Dispatcher;
        });

        public static void Toaster(double workhours)
        {
            if (tts.Reminder)
            {
                if (workhours >= tts.ReminderThreshold)
                {
                    if (NotificationCounter < 4)
                    {
                        GoHomeNotifier.ShowWarning(String.Format("{0} ... ({1})", Properties.Resources.GoHomeNotifierWarn, NotificationCounter.ToString()));
                        if (tts.ReminderSound)
                        {
                            ToastPlayerWarn.Play();
                        }
                    }
                    else if (NotificationCounter >= 4)
                    {
                        GoHomeNotifier.ShowError(String.Format("{0} ... ({1})", Properties.Resources.GoHomeNotifierCrit, NotificationCounter.ToString()));
                        if (tts.ReminderSound)
                        {
                            ToastPlayerCrit.Play();
                        }
                    }
                }
                NotificationCounter++;
            }
        }
    }
}
