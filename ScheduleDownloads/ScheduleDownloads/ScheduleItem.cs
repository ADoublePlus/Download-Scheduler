using System;

namespace ScheduleDownloads
{
    public enum StatusType
    {
        CREATING,
        WAITING,
        RUNNING,
        COMPLETED
    }

    abstract public class ScheduleItem
    {
        public StatusType Status { get; set; }
        public DateTime? TargetDate { get; set; }

        public Runner Runner { get; set; }

        public ResourceType ResourceType { get; set; }

        public string LocalName { get; set; }

        private long id;

        public long Id
        {
            get { return id; }

            protected set
            {
                id = value;
            }
        }

        public ScheduleItem()
        {
            InitialFields();
        }

        public void InitialFields()
        {
            ResourceType = ResourceType.UNKNOWN;
            TargetDate = DateTime.Now;
            Status = StatusType.CREATING;
        }

        public abstract Runner GetRunner();

        public abstract override string ToString();

        protected string GetStatusString()
        {
            string s;

            switch (Status)
            {
                case StatusType.CREATING:
                    s = "CREATING";
                    break;

                case StatusType.WAITING:
                    s = "WAITING";
                    break;

                case StatusType.RUNNING:
                    s = "RUNNING";
                    break;

                case StatusType.COMPLETED:
                    s = "COMPLETED";
                    break;

                default:
                    s = "UNKNOWN";
                    break;
            }

            return s;
        }
    }
}