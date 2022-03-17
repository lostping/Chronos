using System.Windows;
using MahApps.Metro.SimpleChildWindow;

namespace Chronos.libs
{
    /// <summary>
    /// Interaktionslogik für DelStats.xaml
    /// </summary>
    public partial class DelStats : ChildWindow
    {
        public DelStats()
        {
            InitializeComponent();
        }

        private void SaveDel_Click(object sender, RoutedEventArgs e)
        {
            bool invalid = ValidateDialog();
            if (!invalid)
            {
                this.Close(true);
            }
        }

        private void CancelDel_Click(object sender, RoutedEventArgs e)
        {
            this.Close(false);
        }

        private bool ValidateDialog()
        {
            return false;
        }
    }
}
