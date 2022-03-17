using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using Chronos.libs;
using Chronos.Properties;

namespace Chronos
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        private void tt_dg_thismonthswork_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            tt_dg_thismonthswork.Focus();
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            LoadDataToDatagrid();
        }

        public async void Add_Click(object sender, RoutedEventArgs e)
        {
            AddStats addStats = new AddStats();

            DateTime nowdt = DateTime.Now;

            addStats.error.Content = "";
            addStats.StartTimePicker.Value = nowdt;
            addStats.EndTimePicker.Value = nowdt;
            addStats.StartTimePicker.Minimum = nowdt.Date.AddDays(-30);
            addStats.StartTimePicker.Maximum = nowdt.Date.AddDays(1).AddTicks(-1);
            addStats.EndTimePicker.Minimum = nowdt.Date.AddDays(-30);
            addStats.EndTimePicker.Maximum = nowdt.Date.AddDays(1).AddTicks(-1);
            addStats.Title = addStats.Title.ToString() + " " + nowdt.ToString("dddd, dd. MMMM yyyy");

            bool validAdd = await ChildWindowManager.ShowChildWindowAsync<bool>(this, addStats);

            if (validAdd)
            {
                DateTime startTime = (DateTime)addStats.StartTimePicker.Value;
                DateTime endTime = (DateTime)addStats.EndTimePicker.Value;
                string dayDate = startTime.ToString("yyyy-MM-dd");

                string appPath = Helper.GetDbPath();
                SQLite sql = new SQLite(appPath);

                Dictionary<string, string> dataToInsert = new Dictionary<string, string>();
                dataToInsert.Add("today", dayDate);
                dataToInsert.Add("start_time", startTime.ToString("yyyy-MM-dd HH:mm:ss"));
                dataToInsert.Add("end_time", endTime.ToString("yyyy-MM-dd HH:mm:ss"));
                var res = sql.Insert("Chronos", dataToInsert);

                if (res)
                {
                    Logger.Info(Properties.Resources.DayInsertSuccess + ": " + dayDate);
                }
                else
                {
                    Logger.Error(Properties.Resources.DayInsertFail + ": " + dayDate);
                }
                LoadDataToDatagrid();

            }
            else
            {
                Logger.Warn(Properties.Resources.AbortAction + ": " + Properties.Resources.Add);
            }
        }

        public async void Edit_Click(object sender, RoutedEventArgs e)
        {
            DateTime st_full = new DateTime();
            DateTime et_full;
            var row = tt_dg_thismonthswork.SelectedIndex;

            EditStats es = new EditStats();
            DataRowView item = (DataRowView)tt_dg_thismonthswork.SelectedItem;

            if (row == -1)
            {
                es.error.Content = Properties.Resources.SelectRowFail;
                es.SaveEdit.IsEnabled = false;
            }
            else
            {
                var st_ts = item.Row.ItemArray[1];
                var et_ts = item.Row.ItemArray[2];
                st_full = DateTime.Parse(st_ts.ToString());
                et_full = DateTime.Parse(et_ts.ToString());

                es.error.Content = "";
                es.StartTimePicker.Value = st_full;
                es.StartTimePicker.Minimum = st_full.Date;
                es.StartTimePicker.Maximum = st_full.Date.AddDays(1).AddTicks(-1);
                es.EndTimePicker.Value = et_full;
                es.EndTimePicker.Minimum = et_full.Date;
                es.EndTimePicker.Maximum = et_full.Date.AddDays(1).AddTicks(-1);
                es.Title = es.Title.ToString() + " " + st_full.ToString("dddd, dd. MMMM yyyy");
            }

            bool validEdit = await ChildWindowManager.ShowChildWindowAsync<bool>(this, es);


            if (validEdit)
            {
                DateTime startTime = (DateTime)es.StartTimePicker.Value;
                DateTime endTime = (DateTime)es.EndTimePicker.Value;
                string dayDate = st_full.ToString("yyyy-MM-dd");

                string appPath = Helper.GetDbPath();

                SQLite sql = new SQLite(appPath);

                var res = sql.GetDataTable("SELECT * FROM Chronos WHERE today ='" + dayDate + "';");
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("start_time", startTime.ToString("yyyy-MM-dd HH:mm:ss"));
                keyValuePairs.Add("end_time", endTime.ToString("yyyy-MM-dd HH:mm:ss"));
                try
                {
                    var updted = sql.Update("Chronos", keyValuePairs, "today='" + dayDate + "'");
                    Logger.Info(Properties.Resources.DayEditSuccess + ": " + dayDate);
                    LoadDataToDatagrid();
                }
                catch (Exception ex)
                {
                    Logger.Error(Properties.Resources.DayEditFail + ": " + dayDate);
                    Logger.Error(ex.Message);
                }
            }
            else
            {
                Logger.Warn(Properties.Resources.AbortAction + ": " + Properties.Resources.Edit);
            }
        }

        public async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var row = tt_dg_thismonthswork.SelectedIndex;
            DelStats delStats = new DelStats();
            string delDay = "";

            DataRowView item = (DataRowView)tt_dg_thismonthswork.SelectedItem;

            if (row == -1)
            {
                delStats.error.Content = Properties.Resources.SelectRowFail;
                delStats.SaveDel.IsEnabled = false;
            } 
            else
            {
                var dtDay = DateTime.Parse(item.Row.ItemArray[0].ToString());
                delDay = dtDay.ToString("yyyy-MM-dd");
                delStats.day.Content = delDay;
            }

            bool validDelete = await ChildWindowManager.ShowChildWindowAsync<bool>(this, delStats);

            if (validDelete)
            {
                string appPath = Helper.GetDbPath();

                SQLite sql = new SQLite(appPath);

                var res = sql.Delete("Chronos", "today='" + delDay + "';");

                if (res)
                {
                    Logger.Info(Properties.Resources.DayDeleteSuccess + ": " + delDay);
                }
                else
                {
                    Logger.Error(Properties.Resources.DayDeleteFail + ": " + delDay);
                }
                LoadDataToDatagrid();

            }
            else
            {
                Logger.Warn(Properties.Resources.AbortAction + ": " + Properties.Resources.Delete);
            }
        }
    }
}
