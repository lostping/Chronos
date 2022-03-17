using System.Windows;
using MahApps.Metro.Controls;
using ToastNotifications.Messages;
using MahApps.Metro.SimpleChildWindow;
using Chronos.libs;


namespace Chronos
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        async private void WindowCommandExport_Click(object sender, RoutedEventArgs e)
        {
            if (!ExportISNon)
            {
                ExportStats es = new ExportStats();
                ExportISNon = true;
                await ChildWindowManager.ShowChildWindowAsync<bool>(this, es);
            }
        }
    }
}
