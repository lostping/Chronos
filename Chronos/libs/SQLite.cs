using System;
using System.Windows;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Globalization;
using NLog;


namespace Chronos.libs
{
    class SQLite
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        public String dbConnection;

        /// <summary>
        /// Single Param Constructor
        /// Specify if you don't want to use "default.sqlite" filename
        /// </summary>
        /// <param name="dbFile">path to sqlite database file</param>
        public SQLite(string dbFile)
        {
            string AppPath = Directory.GetCurrentDirectory();
            string sqlTemplate = "template/Chronos.sqlite";
            string TemplatePath = Path.Combine(AppPath, sqlTemplate);

            string ttdbpath = Path.GetDirectoryName(dbFile);
            if(!Directory.Exists(ttdbpath)) {
                Directory.CreateDirectory(ttdbpath);
            }
            if(!File.Exists(dbFile)) {
                File.Copy(TemplatePath, dbFile);
            }
            dbConnection = String.Format("Data Source={0};datetimeformat=CurrentCulture", dbFile);
        }

        /// <summary>
        /// Single param constructor for advanced connection options
        /// </summary>
        /// <param name="connectionOpts">a dictionary containing all desired options w/ values</param>
        public SQLite(Dictionary<String, String> connectionOpts)
        {
            String str = connectionOpts.Aggregate("", (current, row) => current + String.Format("{0}={1}; ", row.Key, row.Value));
            str = str.Trim().Substring(0, str.Length - 1);
            dbConnection = str;
        }

        /// <summary>
        /// Runs query against database
        /// Returns a data table
        /// </summary>
        /// <param name="query">the query string</param>
        /// <returns></returns>
        public DataTable GetDataTable(string query)
        {
            DataTable dt = new DataTable();
            string bla = dbConnection;
            try
            {
                SQLiteConnection cnn = new SQLiteConnection(bla);
                cnn.Open();
                SQLiteCommand mycommand = new SQLiteCommand(cnn);
                mycommand.CommandText = query;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                dt.Load(reader);
                reader.Close();
                cnn.Close();
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
            }
            return dt;
        }

        /// <summary>
        /// Interact with database (i. e. get rows updated with last query or table creation/dropping)
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>An integer returing the number of updated rows</returns>
        public Int32 ExecuteNonQuery(string sql)
        {
            try
            {
                SQLiteConnection cnn = new SQLiteConnection(dbConnection);
                cnn.Open();
                SQLiteCommand mycommand = new SQLiteCommand(cnn);
                mycommand.CommandText = sql;
                int rowsUpdated = mycommand.ExecuteNonQuery();
                cnn.Close();
                return rowsUpdated;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
            }
            return 0;
        }

        /// <summary>
        /// Retrieve single item from db
        /// </summary>
        /// <param name="query">The query string to run</param>
        /// <returns>A String</returns>
        public String ExecuteScalar(string query)
        {
            try
            {
                SQLiteConnection cnn = new SQLiteConnection(dbConnection);
                cnn.Open();
                SQLiteCommand mycommand = new SQLiteCommand(cnn);
                mycommand.CommandText = query;
                object value = mycommand.ExecuteScalar();
                cnn.Close();
                if (value != null)
                {
                    return value.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
            }
            return "";
        }

        /// <summary>
        ///     Allows the programmer to easily update rows in the DB.
        /// </summary>
        /// <param name="tableName">The table to update.</param>
        /// <param name="data">A dictionary containing Column names and their new values.</param>
        /// <param name="where">The where clause for the update statement.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Update(String tableName, Dictionary<String, String> data, String where)
        {
            try
            {
                SQLiteConnection cnn = new SQLiteConnection(dbConnection);
                cnn.Open();
                SQLiteCommand mycommand = new SQLiteCommand(cnn);

                String vals = "";
                Boolean returnCode = true;
                if (data.Count >= 1)
                {
                    vals = data.Aggregate(vals, (current, val) => current + String.Format(" {0} = '{1}',", val.Key.ToString(CultureInfo.InvariantCulture), val.Value.ToString(CultureInfo.InvariantCulture)));
                    vals = vals.Substring(0, vals.Length - 1);
                }
                try
                {
                    mycommand.CommandText = (String.Format("update {0} set {1} where {2};", tableName, vals, where));
                    mycommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex.Message);
                    returnCode = false;
                }
                cnn.Close();
                return returnCode;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
            }
            return false;
        }

        /// <summary>
        ///     Allows the programmer to easily delete rows from the DB.
        /// </summary>
        /// <param name="tableName">The table from which to delete.</param>
        /// <param name="where">The where clause for the delete.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Delete(String tableName, String where)
        {
            SQLiteConnection cnn = new SQLiteConnection(dbConnection);
            cnn.Open();
            SQLiteCommand mycommand = new SQLiteCommand(cnn);

            Boolean returnCode = true;
            try
            {
                mycommand.CommandText = (String.Format("delete from {0} where {1};", tableName, where));
                mycommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                returnCode = false;
            }
            cnn.Close();
            return returnCode;
        }

        /// <summary>
        ///     Allows the programmer to easily insert into the DB
        /// </summary>
        /// <param name="tableName">The table into which we insert the data.</param>
        /// <param name="data">A dictionary containing the column names and data for the insert.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Insert(String tableName, Dictionary<String, String> data, bool replace = false)
        {
            SQLiteConnection cnn = new SQLiteConnection(dbConnection);
            cnn.Open();
            SQLiteCommand mycommand = new SQLiteCommand(cnn);
            SQLiteTransaction tr = cnn.BeginTransaction();

            String columns = "";
            String values = "";
            Boolean returnCode = true;
            foreach (KeyValuePair<String, String> val in data)
            {
                columns += String.Format(" {0},", val.Key.ToString());
                values += String.Format(" '{0}',", val.Value);
            }
            columns = columns.Substring(0, columns.Length - 1);
            values = values.Substring(0, values.Length - 1);
            try
            {
                mycommand.Transaction = tr;
                if (replace)
                {
                    mycommand.CommandText = (String.Format("insert or replace into {0}({1}) values({2});", tableName, columns, values));
                }
                else
                {
                    mycommand.CommandText = (String.Format("insert into {0}({1}) values({2});", tableName, columns, values));
                }
                mycommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                returnCode = false;
            }
            tr.Commit();
            cnn.Close();
            return returnCode;
        }

        /// <summary>
        ///     Allows the programmer to easily insert into the DB
        /// </summary>
        /// <param name="DataTable">A DataTable containing the data for the insert.
        /// Picks the TableName as target table in the database.
        /// Reads the DataColumns and processes the DataRows to insert statements and executes transaction.
        /// </param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Insert(DataTable data, bool replace = false)
        {
            bool retVal = false;
            SQLiteConnection cnn = new SQLiteConnection(dbConnection);
            cnn.Open();
            SQLiteCommand mycommand = new SQLiteCommand(cnn);
            SQLiteTransaction tr = cnn.BeginTransaction();

            // get tablename
            string tableName = data.TableName;

            // linq method syntax
            string[] columnsArr = data.Columns.Cast<DataColumn>()
                .Select(x => x.ColumnName)
                .ToArray();
            string columns = String.Join(",", columnsArr);

            mycommand.Transaction = tr;

            try
            {
                foreach (DataRow datarow in data.Rows)
                {
                    string rowData = "";
                    rowData = String.Join("','", datarow.ItemArray.Select(i => i.ToString()).ToArray());
                    string completeCommand = "";
                    if (replace)
                    {
                        completeCommand = String.Format("insert or replace into {0}({1}) values('{2}');", tableName, columns, rowData);
                    }
                    else
                    {
                        completeCommand = String.Format("insert into {0}({1}) values('{2}');", tableName, columns, rowData);
                    }

                    completeCommand = completeCommand.Replace("''", "NULL");
                    mycommand.CommandText = completeCommand;
                    mycommand.ExecuteNonQuery();
                }
                tr.Commit();
                cnn.Close();
                retVal = true;
            }
            catch (Exception ex)
            {
                retVal = false;
                Logger.Fatal(ex.Message);
            }

            return retVal;
        }
    }
}
