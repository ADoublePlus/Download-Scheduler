using System;

namespace ScheduleDownloads
{
    public class FtpScheduleItem : ScheduleItem
    {
        private string hostname;

        public string Hostname
        {
            get { return hostname; }

            set
            {
                hostname = value;
            }
        }

        private string username;

        public string Username
        {
            get { return username; }

            set
            {
                username = value;
            }
        }

        private string password;

        public string Password
        {
            get { return password; }

            set
            {
                password = value;
            }
        }

        private string remoteName;

        public string RemoteName
        {
            get { return remoteName; }

            set
            {
                remoteName = value;

                if (LocalName == String.Empty)
                {
                    LocalName = remoteName;
                }
            }
        }

        public FtpScheduleItem() : base() { }

        public FtpScheduleItem(string host, string user, string password) : base()
        {
            Hostname = host;
            Username = user;
            Password = password;
        }

        public override Runner GetRunner()
        {
            return new FtpRunner(this);
        }

        public override string ToString()
        {
            string ss;

            ss = Id.ToString() + "|";
            ss += TargetDate.ToString() + "|";
            ss += "ftp://" + Username + @"@" + Hostname + "|";
            ss += RemoteName + "|";
            ss += GetStatusString() + "|";

            return ss;
        }
    }
}