using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronos.Classes
{
    public class Objects
    {

        public class DGLog
        {
            public DateTime Date { get; set; }
            public string Level { get; set; }
            public string Message { get; set; }
        }

        public class HolyDay
        {
            public DateTime Date { get; set; }
            public String Holiday { get; set; }
        }

        public class ChronosSettings
        {
            private int save_interval;
            public int SaveInterval
            {
                get
                {
                    return save_interval;
                }

                set
                {
                    this.SaveIntervalString = value.ToString();
                    save_interval = value;
                }
            }
            public string SaveIntervalString { get; set; }

            private float daily_work;
            public float DailyWork
            {
                get
                {
                    return daily_work;
                }

                set
                {
                    this.DailyWorkString = value.ToString();
                    daily_work = value;
                }
            }
            public string DailyWorkString { get; set; }

            private float maxdaily_work;
            public float MaxDailyWork
            {
                get
                {
                    return maxdaily_work;
                }

                set
                {
                    this.MaxDailyWorkString = value.ToString();
                    maxdaily_work = value;
                }
            }
            public string MaxDailyWorkString { get; set; }

            public bool StartWithWindows { get; set; }

            public bool StartHidden { get; set; }

            public bool MinimizeOnClose { get; set; }

            public bool Reminder { get; set; }

            public bool ReminderSound { get; set; }

            public double ReminderThreshold { get; set; }

            private int reminder_interval;
            public int ReminderInterval
            {
                get
                {
                    return reminder_interval;
                }

                set
                {
                    this.ReminderIntervalString = value.ToString();
                    reminder_interval = value;
                }
            }
            public string ReminderIntervalString { get; set; }

            public bool CustomExport { get; set; }
            public string CustomExportFolder { get; set; }
        }
    }
}
