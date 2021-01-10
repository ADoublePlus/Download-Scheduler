namespace ScheduleDownloads
{
    abstract public class ScheduleItemFactory
    {
        public abstract ScheduleItem GetScheduleItem(ResourceType rt);
    }
}