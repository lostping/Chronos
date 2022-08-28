using MahApps.Metro.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
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
        private void StatsChartButton_Click(object sender, RoutedEventArgs e)
        {
            string baseQuery = "SELECT * FROM CalculatedTimes WHERE today";

            var selDate = StatsDatePickG.SelectedDate;
            string type = ((ComboBoxItem)StatsTypeComboboxG.SelectedItem).Tag.ToString();
            if (type.Length < 1 || type == string.Empty) { type = "week"; }
            string finalQuery = "";
            switch (type)
            {
                case "week":
                    var startWeek = selDate.Value.StartOfWeek(DayOfWeek.Monday);
                    var endWeek = startWeek.AddDays(5).Subtract(new TimeSpan(0, 0, 1));
                    string startWeekStr = startWeek.ToString("yyyy-MM-dd");
                    string endWeekStr = endWeek.ToString("yyyy-MM-dd");
                    finalQuery = string.Format("{0} BETWEEN '{1}' AND '{2}';", baseQuery, startWeekStr, endWeekStr);
                    break;
                case "month":
                    var startMonth = DateTimeExtensions.FirstDayOfMonth(selDate.Value);
                    var startMonthStr = startMonth.ToString("yyyy-MM-dd");
                    var endMonth = DateTimeExtensions.LastDayOfMonth(selDate.Value);
                    var endMonthStr = endMonth.ToString("yyyy-MM-dd");
                    finalQuery = string.Format("{0} BETWEEN '{1}' AND '{2}';", baseQuery, startMonthStr, endMonthStr);
                    break;
                case "year":
                    var targetYear = selDate.Value.ToString("yyyy");
                    finalQuery = string.Format("{0} LIKE '{1}-%';", baseQuery, targetYear);
                    break;
                default:
                    // this shouldn't be called as we force string type to week fallback
                    break;
            }
            LoadDataToChart(finalQuery);
        }

        public void LoadDataToChart(string query = "none")
        {
            if (query.Length < 1 || query == "none")
            {
                string baseQuery = "SELECT * FROM CalculatedTimes WHERE today";

                var startWeek = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
                var endWeek = startWeek.AddDays(5).Subtract(new TimeSpan(0, 0, 1));
                string startWeekStr = startWeek.ToString("yyyy-MM-dd");
                string endWeekStr = endWeek.ToString("yyyy-MM-dd");
                query = string.Format("{0} BETWEEN '{1}' AND '{2}';", baseQuery, startWeekStr, endWeekStr);

            }

            string appPath = Helper.GetDbPath();
            SQLite sql = new SQLite(appPath);
            DataTable chartDatapoints = sql.GetDataTable(query);

            Dictionary<string, TimeSpan> dtdp = MakeDateTimes(chartDatapoints);

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

                var maxval = (Int32)dynValues.Max(x => x.Value);
                if (maxval > 12)
                {
                    workchart.AxisY.Clear();

                    var sections = new LiveCharts.Wpf.SectionsCollection();
                    sections.Add(new LiveCharts.Wpf.AxisSection
                    {
                        Value = 0,
                        SectionWidth = tts.MaxDailyWork,
                        Label = "Good",
                        Fill = new SolidColorBrush() { Color = Color.FromRgb(51, 255, 218), Opacity = 0.4 }
                    });
                    sections.Add(new LiveCharts.Wpf.AxisSection()
                    {
                        Value = tts.MaxDailyWork,
                        SectionWidth = maxval,
                        Label = "Bad",
                        Fill = new SolidColorBrush() { Color = Color.FromRgb(245, 114, 114), Opacity = 0.4 }
                    }
                    );

                    workchart.AxisY.Add(
                        new LiveCharts.Wpf.Axis
                        {
                            MinValue = 0,
                            MaxValue = maxval,
                            Sections = sections
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Properties.Resources.ChartDatapointsFail + ": " + ex.Message);
            }
        }
    }
}
