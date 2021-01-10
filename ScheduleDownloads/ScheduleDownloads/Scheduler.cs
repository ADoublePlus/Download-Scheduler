using System;
using System.Collections.Generic;
using System.Threading;

namespace ScheduleDownloads
{
    public sealed class Scheduler
    {
        // Singleton implementation for instance
        private static volatile Scheduler instance;
        private static readonly object syncThreads = new Object();

        private Timer timer;
        private uint timeoutMs = 1000 * 60;

        private List<ScheduleItem> schedule;

        public delegate void LogHandler(Scheduler scheduler, LogEventArgs args);
        public event LogHandler OnLog;

        public bool Started { get; private set; }
        public string DownloadDir { get; set; }

        public static Scheduler Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncThreads)
                    {
                        if (instance == null)
                        {
                            instance = new Scheduler();
                        }
                    }
                }

                return instance;
            }
        }

        private Scheduler()
        {
            InitialFields();
        }

        private void InitialFields()
        {
            schedule = new List<ScheduleItem>();

            Started = false; // Guarantee initial state
        }

        ~Scheduler()
        {
            Stop();
        }

        /// <summary>
        /// Starts the Scheduler (starts polling).
        /// </summary>
        public void Start()
        {
            if (!Started)
            {
                Started = true;
                timer = new Timer(new TimerCallback(Service), null, timeoutMs, timeoutMs);
            }
        }

        /// <summary>
        /// Stop the Scheduler (stops polling).
        /// </summary>
        public void Stop()
        {
            if (Started)
            {
                Started = false;
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer = null;
            }
        }

        /// <summary>
        /// Method that sends a message to the log.
        /// </summary>
        public void Log(string s)
        {
            if (OnLog != null)
            {
                OnLog(this, new LogEventArgs(s));
            }
        }

        /// <summary>
        /// Adds an existing ScheduleItem to the schedule.
        /// </summary>
        public ScheduleItem Add(ScheduleItem n)
        {
            lock (syncThreads)
            {
                schedule.Add(n);
                n.Status = StatusType.WAITING;
            }

            return n;
        }

        /// <summary>
        /// Get status of all scheduled items.
        /// </summary>
        public string GetStatus()
        {
            string s = String.Empty;

            foreach (ScheduleItem i in schedule)
            {
                s += i.ToString() + "\r\n";
            }

            s = "Items=" + Convert.ToString(schedule.Count) + "\r\n" + s;

            return s;
        }

        /// <summary>
        /// Called every minute to check for work.
        /// </summary>
        private void Service(object o)
        {
            List<ScheduleItem> scheduleLocal;

            lock (syncThreads)
            {
                scheduleLocal = new List<ScheduleItem>(schedule);
                Log("Checking schedule... (" + scheduleLocal.Count + " items)");
            }

            foreach (ScheduleItem i in scheduleLocal)
            {
                // Handle COMPLETED tasks
                if (i.Status == StatusType.RUNNING)
                {
                    if ((i.Runner != null) && (i.Runner.Progress == 100))
                    {
                        lock (syncThreads)
                        {
                            i.Status = StatusType.COMPLETED;
                        }
                    }
                }

                // Handle WAITING tasks
                if ((i.Status == StatusType.WAITING) && (i.TargetDate < DateTime.Now))
                {
                    lock (syncThreads)
                    {
                        i.Status = StatusType.RUNNING;
                    }

                    switch (i.ResourceType)
                    {
                        case ResourceType.WEB:
                            HttpRunner hr = (HttpRunner)i.GetRunner();
                            i.Runner = hr;
                            hr.DownloadDir = DownloadDir;
                            Log("WEB Download: " + ((WebScheduleItem)i).Link + " To local file: " + i.LocalName);
                            hr.Start();
                            break;

                        case ResourceType.FTP:
                            FtpRunner fr = (FtpRunner)i.GetRunner();
                            i.Runner = fr;
                            fr.DownloadDir = DownloadDir;
                            Log("FTP Download: " + ((FtpScheduleItem)i).ToString());
                            fr.Start();
                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }
}