using System;
using System.IO;
using System.Data;
using NanoXLSX;
using NanoXLSX.Styles;
using NLog;

namespace Chronos.libs
{
    class XLSXExport
    {
        static Workbook wb;
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public void Workbook(string fileName, string exportPath)
        {
            string saveFile = Path.Combine(exportPath, fileName);
            wb = new Workbook(saveFile, Properties.Resources.Worktime);
            wb.CurrentWorksheet.SetColumnWidth(0, 12f);
            wb.CurrentWorksheet.SetColumnWidth(1, 20f);
            wb.CurrentWorksheet.SetColumnWidth(2, 20f);
        }

        public bool DatasetToSheet(DataTable dt)
        {
            try
            {
                // write header
                DataColumnCollection cols = dt.Columns;
                foreach (DataColumn col in cols)
                {
                    wb.CurrentWorksheet.AddNextCell(col.ColumnName, BasicStyles.Bold);
                }
                wb.CurrentWorksheet.GoToNextRow();

                // write rows
                foreach (DataRow row in dt.Rows)
                {
                    foreach (var value in row.ItemArray)
                    {
                        wb.CurrentWorksheet.AddNextCell(value.ToString(), BasicStyles.DateFormat);
                    }
                    wb.CurrentWorksheet.GoToNextRow();
                }
                wb.Save();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
        }
    }
}
