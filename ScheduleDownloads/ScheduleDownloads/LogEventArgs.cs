using System;

namespace ScheduleDownloads
{
    public class LogEventArgs : EventArgs
    {
        public string Text { get; private set; }

        public LogEventArgs(string s)
        {
            Text = s;
        }
    }
}