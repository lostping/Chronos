using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.SimpleChildWindow;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FluentDate;
using FluentDateTime;
using Chronos.libs;
using ToastNotifications.Messages;

namespace Chronos.libs
{

/// <summary>
/// Interaktionslogik für ExportStats.xaml
/// </summary>
public partial class ExportStats : ChildWindow
    {
        DateTime begWeek;
        DateTime endWeek;
        DateTime begMonth;
        DateTime endMonth;
        string selYear;


        public enum ExportType
        {
            Week,
            Month,
            Year
        }

        public ExportStats()
        {
            InitializeComponent();
            ExportDatePicker.DisplayDateEnd = DateTime.Today;
        }

        private void ExportDatePicker_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            ExportWeek.IsEnabled = true;
            ExportMonth.IsEnabled = true;
            ExportYear.IsEnabled = true;

            begWeek = ExportDatePicker.SelectedDate.Value.BeginningOfWeek();
            string begWeekT = begWeek.ToString("dd-MM-yyyy");
            endWeek = ExportDatePicker.SelectedDate.Value.EndOfWeek();
            string endWeekT = endWeek.ToString("dd-MM-yyyy");
            begMonth = ExportDatePicker.SelectedDate.Value.BeginningOfMonth();
            string begMonthT = begMonth.ToString("dd-MM-yyyy");
            endMonth = ExportDatePicker.SelectedDate.Value.EndOfMonth();
            string endMonthT = endMonth.ToString("dd-MM-yyyy");
            selYear = ExportDatePicker.SelectedDate.Value.Year.ToString();

            ExportWeekT.Text = String.Join(" ", begWeekT, Properties.Resources.To, endWeekT);
            ExportMonthT.Text = String.Join(" ", begMonthT, Properties.Resources.To, endMonthT);
            ExportYearT.Text = selYear;
        }

        private void ExportWeek_Click(object sender, RoutedEventArgs e)
        {
            List<string> ds = DateTimeExtensions.GetDatesBetweenAsString(begWeek, endWeek, "yyyy-MM-dd");
            string days = "'"+String.Join("','", ds)+"'";
            ExportData(ExportType.Week, days, ExportAll.IsOn);
        }

        private void ExportMonth_Click(object sender, RoutedEventArgs e)
        {
            string month = begMonth.ToString("yyyy-MM");
            ExportData(ExportType.Month, month, ExportAll.IsOn);
        }

        private void ExportYear_Click(object sender, RoutedEventArgs e)
        {
            string year = selYear;
            ExportData(ExportType.Year, year, ExportAll.IsOn);
        }

        private void ExportDatePicker_GotMouseCapture(object sender, MouseEventArgs e)
        {
            UIElement originalElement = e.OriginalSource as UIElement;
            if (originalElement is CalendarDayButton || originalElement is CalendarItem)
            {
                originalElement.ReleaseMouseCapture();
            }
        }

        private void ExportData(ExportType exType, string dates, bool gt)
        {
            string appPath = Helper.GetDbPath();
            SQLite sQ = new SQLite(appPath);
            DataTable exportData;
            string query;
            string greater;
            if (gt) { 
                greater = "AND CAST(STRFTIME('%H',time_worked) AS INTEGER) >= 10 ";
            } else
            {
                greater = "";
            }

            string exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Chronos");
            if (MainWindow.tts.CustomExport)
            {
                exportPath = MainWindow.tts.CustomExportFolder;
            }
            else
            {
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }
            }
            string fileName;

            string weekN = String.Format("__{0} {1} {2}", ExportDatePicker.SelectedDate.Value.BeginningOfWeek().ToString("dd-MM-yyyy"), Properties.Resources.To, ExportDatePicker.SelectedDate.Value.EndOfWeek().ToString("dd-MM-yyyy"));
            string monthN = String.Format("__{0} {1} {2}", ExportDatePicker.SelectedDate.Value.BeginningOfMonth().ToString("dd-MM-yyyy"), Properties.Resources.To, ExportDatePicker.SelectedDate.Value.EndOfMonth().ToString("dd-MM-yyyy"));
            string yearN = String.Format("__{0}", ExportDatePicker.SelectedDate.Value.Year.ToString());

            switch (exType)
            {
                case ExportType.Week:
                    fileName = string.Format("W_{0}{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), weekN);
                    query = string.Format("SELECT STRFTIME('%d.%m.%Y', today) AS {0}, start_time AS {1}, end_time AS {2}, time_worked AS {3} FROM CalculatedTimes WHERE today in ({4}) {5}ORDER BY today;", Properties.Resources.Day, Properties.Resources.StartTime, Properties.Resources.EndTime, Properties.Resources.Worktime, dates, greater);
                    exportData = sQ.GetDataTable(query);
                    ExportWorktime(exportData, fileName,exportPath);
                    break;
                case ExportType.Month:
                    fileName = string.Format("M_{0}{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), monthN);
                    query = string.Format("SELECT STRFTIME('%d.%m.%Y', today) AS {0}, start_time AS {1}, end_time AS {2}, time_worked AS {3} FROM CalculatedTimes WHERE today like ('{4}-%') {5}ORDER BY today;", Properties.Resources.Day, Properties.Resources.StartTime, Properties.Resources.EndTime, Properties.Resources.Worktime, dates, greater);
                    exportData = sQ.GetDataTable(query);
                    ExportWorktime(exportData, fileName, exportPath);
                    break;
                case ExportType.Year:
                    fileName = string.Format("Y_{0}{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), yearN);
                    query = string.Format("SELECT STRFTIME('%d.%m.%Y', today) AS {0}, start_time AS {1}, end_time AS {2}, time_worked AS {3} FROM CalculatedTimes WHERE today like ('{4}-%') {5}ORDER BY today;", Properties.Resources.Day, Properties.Resources.StartTime, Properties.Resources.EndTime, Properties.Resources.Worktime, dates, greater);
                    exportData = sQ.GetDataTable(query);
                    ExportWorktime(exportData, fileName, exportPath);
                    break;
                default:
                    break;
            }
        }

        private static void ExportWorktime(DataTable exportData, string fileName, string exportPath)
        {
            XLSXExport xLSX = new XLSXExport();
            xLSX.Workbook(fileName, exportPath);
            bool success = xLSX.DatasetToSheet(exportData);
            if (success)
            {
                MainWindow.TrayNotifier.ShowSuccess(Properties.Resources.ExportSuccess);
            }
            else
            {
                MainWindow.TrayNotifier.ShowError(Properties.Resources.ExportFail);
            }
        }

        private void Exportchildwindow_ClosingFinished(object sender, RoutedEventArgs e)
        {
            MainWindow.ExportISNon = false;
        }
    }
}
