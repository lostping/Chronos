using System;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chronos.Classes;
using FluentScheduler;
using NLog;

namespace Chronos.libs
{
    public class JobRegistry : Registry
    {
        public JobRegistry(Objects.ChronosSettings settings)
        {
            Schedule<UpdateWorktime>().ToRunNow().AndEvery(settings.SaveInterval).Minutes();
            Schedule<FireToaster>().ToRunNow().AndEvery(settings.ReminderInterval).Minutes();
        }
    }

    public class FireToaster : IJob
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public void Execute()
        {
            try
            {
                double nettoWorktime = 0;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow mw = Application.Current.MainWindow as MainWindow;
                    try
                    {
                        nettoWorktime = (double)mw.tb_netto.Value;
                    }
                    catch (Exception)
                    {
                        nettoWorktime = 0;
                    }
                });
                Application.Current.Dispatcher.Invoke(new Action(() => { MainWindow.Toaster(nettoWorktime); }));
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
            }
        }
    }

    public class UpdateWorktime : IJob
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public void Execute()
        {
            try
            {
                string appPath = Helper.GetDbPath();
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                SQLite sql = new SQLite(appPath);

                string insert_str = "INSERT OR IGNORE INTO Chronos VALUES('" + today + "', '" + now + "', '" + now + "');";
                string update_str = "UPDATE Chronos SET end_time = '" + now + "' WHERE today == '" + today + "';";

                string statement = insert_str + Environment.NewLine + update_str;
                sql.ExecuteNonQuery(statement);

                var timeWorked = sql.GetDataTable("SELECT start_time,end_time,time_worked from CalculatedTimes WHERE today='"+today+"'");
                string strTW = timeWorked.Rows[0].ItemArray[2].ToString();
                string strST = timeWorked.Rows[0].ItemArray[0].ToString();
                string strET = timeWorked.Rows[0].ItemArray[1].ToString();
                TimeSpan tsTimeWorked = TimeSpan.Parse(strTW);
                string startTime = DateTime.Parse(strST).ToString("HH:mm");
                string endTime = DateTime.Parse(strET).ToString("HH:mm");
                double workedHours = 0;

                switch (tsTimeWorked.Hours)
                {
                    case int n when n > 6:
                        workedHours = tsTimeWorked.Subtract(TimeSpan.FromMinutes(30)).TotalHours;
                        break;
                    case int n when n > 9:
                        workedHours = tsTimeWorked.Subtract(TimeSpan.FromMinutes(45)).TotalHours;
                        break;
                    default:
                        workedHours = tsTimeWorked.TotalHours;
                        break;
                }

                try
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => { MainWindow.UpdateWorkVisuals(tsTimeWorked.TotalHours, workedHours, startTime, endTime); }));
                    Application.Current.Dispatcher.Invoke(new Action(() => { MainWindow.LoadDataToDatagrid(); }));
                }
                catch (Exception ex)
                {
                    Logger.Fatal(Properties.Resources.UpdateVisualsFailed + " " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(Properties.Resources.TTSaveFailed + " " + ex.Message);
            }
        }
    }
}
