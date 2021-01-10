using System.Diagnostics;

namespace ScheduleDownloads
{
    class EventLogger : ILogger
    {
        public EventLogger() { }

        public void LogMessage(string msg)
        {
            EventLog.WriteEntry("Logger", msg);
        }
    }
}