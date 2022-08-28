using LiveCharts;
using MahApps.Metro.Controls;
using NLog;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LiveCharts.Wpf;
using Chronos.Classes;
using Chronos.libs;
using System.Diagnostics;
using ToastNotifications.Messages;
using ToastNotifications.Lifetime.Clear;

namespace Chronos
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static string portableConfFile = "Chronos.settings.ini";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        // logging adds a row to observable collection, which will immediately show up in the data grid
        private static readonly ObservableCollection<Objects.DGLog> obs = new ObservableCollection<Objects.DGLog>();
        // chart values observable collection
        public static ChartValues<LiveCharts.Defaults.ObservableValue> dynValues = new ChartValues<LiveCharts.Defaults.ObservableValue>();
        public static SeriesCollection SeriesCollection { get; set; }
        public static List<string> LabelCollection { get; set; }

        public static Objects.ChronosSettings tts = new Objects.ChronosSettings();

        public static bool ExportISNon;

        public MainWindow()
        {
            InitializeComponent();

            // detect setting changes
            Properties.Settings.Default.SettingChanging += Default_SettingChanging;

            // Version in Title
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string title = this.Title;
            this.Title = string.Format("{0} | {1}", title, version);

            #region Logging to datagrid
            // make ObservableCollection updatable from other threads!
            object _itemsLock = new object();
            BindingOperations.EnableCollectionSynchronization(obs, _itemsLock);

            // initialize datagrid log
            tt_dg.ItemsSource = obs;
            tt_dg.Items.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));
            #endregion

            // charting
            #region Charting
            SeriesCollection = new SeriesCollection()
            {
                new ColumnSeries // alternative bar display
                //new LineSeries // Line display
                {
                    Title = Properties.Resources.Worktime,
                    Values = new ChartValues<double> { }
                }
            };

            SeriesCollection[0].Values = dynValues;
            LabelCollection = new List<string>();
            DataContext = this;
            #endregion
        }

        private void Default_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            tts = Configuration.LoadSettings(portableConfFile);
            tb_soll.Text = tts.DailyWorkString;
            LoadDataToDatagrid();
        }

        /// <summary>
        /// LogToDG will pass nlog messages to observable collection
        /// </summary>
        /// <param name="date">the date to be displayed</param>
        /// <param name="level">the log level</param>
        /// <param name="message">the log message</param>
        public static void LogToDG(string date, string level, string message)
        {
            Objects.DGLog row = new Objects.DGLog { Date = Convert.ToDateTime(date), Level = level, Message = message };
            obs.Add(row);
        }


        public static void LoadDataToDatagrid(string query="none")
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

            MainWindow mw = Application.Current.MainWindow as MainWindow;
            mw.tt_dg.Items.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));
            string appPath = Helper.GetDbPath();

            SQLite sql = new SQLite(appPath);
            DataTable thisMonthsWork = sql.GetDataTable(query);
            DataTable sum = new DataTable();
            sum.Columns.Add();
            sum.Columns.Add("SumTitle");
            sum.Columns.Add("WorkTimeSum");
            sum.Columns.Add();
            DataRow dr = sum.NewRow();
            var secsum = thisMonthsWork.Rows.Cast<DataRow>()
                        .AsEnumerable()
                        .Sum(r => TimeSpan.Parse(r.ItemArray[3].ToString()).TotalSeconds);
            TimeSpan tsSum = TimeSpan.FromSeconds(secsum);
            dr["SumTitle"] = Properties.Resources.TotalHoursWorked;
            dr["WorkTimeSum"] = String.Format("{0:0.00}", tsSum.TotalHours);

            sum.Rows.Add(dr);
            DataView thismonthView = thisMonthsWork.DefaultView;
            thismonthView.Sort = "today DESC";
            mw.tt_dg_thismonthswork.ItemsSource = thismonthView;
            mw.tt_dg_sum.ItemsSource = sum.DefaultView;
        }

        public static void UpdateWorkVisuals(double gross, double nettoGauge, string entry, string outro)
        {
            MainWindow mw = Application.Current.MainWindow as MainWindow;
            mw.tb_brutto.Value = gross;
            mw.tb_netto.Value = nettoGauge;
            mw.netto_gauge.Value = nettoGauge;
            mw.tt_in_time.Text = entry;
            mw.tt_out_time.Text = outro;
        }

        private void ChronosWindow_Closed(object sender, EventArgs e)
        {
            TrayNotifier.ClearMessages(new ClearAll());
            TrayNotifier.Dispose();
            GoHomeNotifier.ClearMessages(new ClearAll());
            GoHomeNotifier.Dispose();
        }

        private void ChronosWindow_Closing(object sender, CancelEventArgs e)
        {
            if (tts.MinimizeOnClose) {
                if(ChronosWindow.Visibility == Visibility.Visible) {
                    Tray_minmax_Click(sender, null);
                    e.Cancel = true;
                }
            }
        }

        private void Tray_openexport_Click(object sender, RoutedEventArgs e)
        {
            if (!tts.CustomExport)
            {
                try
                {
                    Process.Start("explorer", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Chronos"));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                }
            }
            else
            {
                try
                {
                    Process.Start("explorer", tts.CustomExportFolder);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                }
            }
        }

        private void StatsFilterButton_Click(object sender, RoutedEventArgs e)
        {
            string baseQuery = "SELECT * FROM CalculatedTimes WHERE today";

            var selDate = StatsDatePick.SelectedDate;
            string type = ((ComboBoxItem)StatsTypeCombobox.SelectedItem).Tag.ToString();
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
            LoadDataToDatagrid(finalQuery);
        }
    }
}
