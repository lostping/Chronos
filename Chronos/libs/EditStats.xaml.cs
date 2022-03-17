using System.Windows;
using MahApps.Metro.SimpleChildWindow;

namespace Chronos.libs
{
    /// <summary>
    /// Interaktionslogik für EditStats.xaml
    /// </summary>
    public partial class EditStats : ChildWindow
    {
        public EditStats()
        {
            InitializeComponent();
        }

        private void SaveEdit_Click(object sender, RoutedEventArgs e)
        {
            bool invalid = ValidateDialog();
            if (!invalid)
            {
                this.Close(true);
            }
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            this.Close(false);
        }

        private bool ValidateDialog()
        {
            return false;
        }
    }
}
