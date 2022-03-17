using System;
using System.Windows;
using MahApps.Metro.SimpleChildWindow;
using System.Windows.Controls;

namespace Chronos.libs
{
    /// <summary>
    /// Interaktionslogik für AddStats.xaml
    /// </summary>
    public partial class AddStats : ChildWindow
    {
        public AddStats()
        {
            InitializeComponent();
        }

        private void SaveNew_Click(object sender, RoutedEventArgs e)
        {
            bool invalid = ValidateDialog();
            if (!invalid)
            {
                this.Close(true);
            }
        }

        private void CancelNew_Click(object sender, RoutedEventArgs e)
        {
            this.Close(false);
        }

        private bool ValidateDialog()
        {
            this.error.Content = "";
            if (StartTimePicker.Value == null || StartTimePicker.Value.ToString().Length == 0 || EndTimePicker.Value == null || EndTimePicker.Value.ToString().Length == 0)
            {
                this.error.Content = Properties.Resources.AddStatsDaysNotEmpty;
                StartTimePicker.BorderBrush = System.Windows.Media.Brushes.Red;
                EndTimePicker.BorderBrush = System.Windows.Media.Brushes.Red;
                return true;
            }

            if (StartTimePicker.Value.Value.Date != EndTimePicker.Value.Value.Date)
            {
                this.error.Content = Properties.Resources.AddStatsStartEndMustMatch;
                StartTimePicker.BorderBrush = System.Windows.Media.Brushes.Red;
                EndTimePicker.BorderBrush = System.Windows.Media.Brushes.Red;
                return true;
            }

            string appPath = Helper.GetDbPath();
            DateTime thismonth = (DateTime)this.EndTimePicker.Value;
            string mstring = thismonth.ToString("yyyy-MM-dd");

            SQLite sql = new SQLite(appPath);
            string match = sql.ExecuteScalar("SELECT COUNT(today) FROM CalculatedTimes WHERE today = '" + mstring + "'");

            if (match == "1")
            {
                this.error.Content = Properties.Resources.AddStatsAlreadyExist;
                //this.IsPrimaryButtonEnabled = false;
                return true;
            }
            else if (match == "0")
            {
                return false;
            }

            return true;
        }
    }
}
