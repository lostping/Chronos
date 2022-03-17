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
using MahApps.Metro.Controls;
using ToastNotifications.Messages;

namespace Chronos
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private void Tray_quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Tray_minmax_Click(object sender, RoutedEventArgs e)
        {
            switch (this.Visibility)
            {
                case Visibility.Visible:
                    this.Visibility = Visibility.Collapsed;
                    TrayNotifier.ShowSuccess(Properties.Resources.MinToTray);
                    tray_minmax.Header = Properties.Resources.WindowShow;
                    break;
                case Visibility.Hidden:
                    this.Show();
                    this.Topmost = true;
                    tray_minmax.Header = Properties.Resources.WindowHide;
                    break;
                case Visibility.Collapsed:
                    this.Show();
                    this.Topmost = true;
                    tray_minmax.Header = Properties.Resources.WindowHide;
                    break;
                default:
                    break;
            }
        }

        private void ChronosNotify_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Tray_minmax_Click(this, null);
        }
    }
}
