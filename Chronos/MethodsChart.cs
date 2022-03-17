using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chronos.libs;

namespace Chronos
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private void ChartWeekClick(object sender, RoutedEventArgs e)
        {
            string whichWeek = ((MenuItem)sender).Tag.ToString();
            switch (whichWeek)
            {
                case "this":
                    ChartWeek(DateTime.Now.StartOfWeek(DayOfWeek.Monday));
                    break;
                case "last":
                    ChartWeek(DateTime.Now.StartOfLastWeek(DayOfWeek.Monday));
                    break;
                default:
                    break;
            }
        }

        private void ChartMonthClick(object sender, RoutedEventArgs e)
        {
            string whichWeek = ((MenuItem)sender).Tag.ToString();
            switch (whichWeek)
            {
                case "this":
                    ChartMonth(DateTimeExtensions.GetDaysOfWeek(DateTime.Now, DayOfWeek.Monday));
                    break;
                case "last":
                    ChartMonth(DateTimeExtensions.GetDaysOfWeek(DateTime.Now.AddMonths(-1), DayOfWeek.Monday));
                    break;
                default:
                    break;
            }
        }

        private void ChartWeek(DateTime weekStart)
        {
            // get start of week, get end of week and substract one second to stay within work week
            DateTime dt_week_start = weekStart;
            DateTime dt_week_end = dt_week_start.AddDays(5).Subtract(new TimeSpan(0, 0, 1));

            List<string> list = Helper.GetDateRange(dt_week_start, dt_week_end);
            string queryRange = "SELECT * FROM CalculatedTimes WHERE today in ('" + String.Join("','", list) + "') ORDER BY today ASC;";

            string appPath = Helper.GetDbPath();

            SQLite sQLite = new SQLite(appPath);
            DataTable datapoints = sQLite.GetDataTable(queryRange);

            Dictionary<string, TimeSpan> dtdp = MakeDateTimes(datapoints);

            FillChart(dtdp);
        }

        private void ChartMonth(IEnumerable<DateTime> targetDates)
        {
            List<string> workdaysList = new List<string>();
            Dictionary<DateTime, DateTime> workWeeks = new Dictionary<DateTime, DateTime>();
            foreach (var date in targetDates)
            {
                workWeeks.Add(date, date.AddDays(5).Subtract(new TimeSpan(0, 0, 1)));
            }
            foreach (var workweek in workWeeks)
            {
                workdaysList.AddRange(Helper.GetDateRange(workweek.Key, workweek.Value));
            }

            string queryRange = "SELECT * FROM CalculatedTimes WHERE today in ('" + String.Join("','", workdaysList) + "') ORDER BY today ASC;";

            string appPath = Helper.GetDbPath();

            SQLite sQLite = new SQLite(appPath);
            DataTable datapoints = sQLite.GetDataTable(queryRange);

            Dictionary<string, TimeSpan> dtdp = MakeDateTimes(datapoints);

            FillChart(dtdp);
        }

        // helper methods
        private Dictionary<string, TimeSpan> MakeDateTimes(DataTable dt)
        {
            Dictionary<string, TimeSpan> tspans = new Dictionary<string, TimeSpan>();

            foreach (DataRow row in dt.Rows)
            {
                string day = DateTime.Parse(row["today"].ToString()).ToString("yyyy-MM-dd");
                TimeSpan ts = TimeSpan.Parse(row["time_worked"].ToString());
                tspans.Add(day, ts);
            }
            return tspans;
        }

        private async void FillChart(Dictionary<string, TimeSpan> dtdp)
        {
            dynValues.Clear();
            LabelCollection.Clear();

            try
            {
                await Task.Run(async () =>
                {
                    foreach (var time in dtdp)
                    {
                        LabelCollection.Add(time.Key);
                        dynValues.Add(new LiveCharts.Defaults.ObservableValue(time.Value.TotalHours));
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error(Properties.Resources.ChartDatapointsFail + ": " + ex.Message);
            }
        }
    }
}
