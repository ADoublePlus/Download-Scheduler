using System;

namespace ScheduleDownloads
{
    public enum CommandType
    {
        UNKNOWN,
        FIN,
        STAT,
        NQUE,
        DQUE
    }

    public enum ResourceType
    {
        UNKNOWN,
        WEB,
        FTP,
        /*
         * FILE,
         * SITE
         */
    }

    public class FormatInterpreter
    {
        protected ScheduleItemFactory sif;
        protected ScheduleItem si;

        private char[] delims = { '|' };

        public char[] Delims
        {
            get { return delims; }
        }

        private CommandType command;

        public CommandType Command
        {
            get { return command; }
        }

        private ResourceType resourceType;

        public ResourceType ResourceType
        {
            get { return resourceType; }
        }

        private string resource;
        
        public string Resource
        {
            get { return resource; }
        }

        private string filename;

        public string Filename
        {
            get { return filename; }
        }

        private string host;

        public string Host
        {
            get { return host; }
        }

        private string user;

        public string User
        {
            get { return user; }
        }

        private string pass;

        public string Pass
        {
            get { return pass; }
        }

        private DateTime? target;

        public DateTime? Target
        {
            get { return target; }
        }

        public FormatInterpreter()
        {
            InitialFields();
        }

        public FormatInterpreter(ScheduleItemFactory t)
        {
            sif = t;

            InitialFields();
        }

        public FormatInterpreter(string s)
        {
            InitialFields();

            ParseString(s);
        }

        protected void InitialFields()
        {
            if (sif == null)
            {
                sif = new AppScheduleItemFactory();
            }
        }

        protected void Clear()
        {
            si = null;

            resourceType = ResourceType.UNKNOWN;
        }

        public void ParseString(string s)
        {
            Clear();

            string[] toks = s.Replace("\r\n", "").Split(delims);

            // Field 1: COMMAND
            if (toks.Length > 0)
            {
                command = GenCommandType(toks[0]);

                // Field: RESOURCE TYPE (WEB, FTP, ETC)
                string resTypeStr = FindSection(toks, "TYPE");

                if (IsEmpty(resTypeStr))
                {
                    resourceType = ResourceType.UNKNOWN;
                }
                else
                {
                    resourceType = GenResourceType(resTypeStr);
                }

                // Field: WEB RESOURCE/FTP PATH
                resource = FindSection(toks, "RES");

                // Field: LOCAL FILENAME
                filename = FindSection(toks, "NAME");

                // Field: DATE/TIME TO BEGIN
                string td = FindSection(toks, "WHEN");

                if (td == null)
                {
                    target = null;
                }
                else
                {
                    target = GenTarget(td);
                }

                // Field: HOSTNAME(FTP)
                host = FindSection(toks, "HOST");

                // Field: USERNAME
                user = FindSection(toks, "USER");

                // Field: PASSWORD
                pass = FindSection(toks, "PASS");
            }
        }

        protected string FindSection(string[] t, string s)
        {
            string r = null;

            foreach (string i in t)
            {
                if (i.StartsWith(s + "="))
                {
                    r = i.Substring(s.Length + 1);

                    break;
                }
            }

            return r;
        }

        protected CommandType GenCommandType(string s)
        {
            CommandType result;

            switch (s.ToUpper())
            {
                case "FIN":
                    result = CommandType.FIN;
                    break;

                case "STAT":
                    result = CommandType.STAT;
                    break;

                case "NQUE":
                    result = CommandType.NQUE;
                    break;

                case "DQUE":
                    result = CommandType.DQUE;
                    break;

                default:
                    result = CommandType.UNKNOWN;
                    break;
            }

            return result;
        }

        protected ResourceType GenResourceType(string s)
        {
            ResourceType result;

            switch (s.ToUpper())
            {
                case "WEB":
                    result = ResourceType.WEB;
                    break;

                case "FTP":
                    result = ResourceType.FTP;
                    break;

                default:
                    result = ResourceType.UNKNOWN;
                    break;
            }

            return result;
        }

        protected DateTime GenTarget(string s)
        {
            return new DateTime(Convert.ToInt32(s.Substring(0, 4)), Convert.ToInt32(s.Substring(4, 2)), 
                                                       Convert.ToInt32(s.Substring(6, 2)), Convert.ToInt32(s.Substring(8, 2)), 
                                                       Convert.ToInt32(s.Substring(10, 2)), Convert.ToInt32(s.Substring(12, 2)));
        }

        protected bool IsEmpty(string s)
        {
            bool r = false;

            if (s == null)
            {
                r = true;
            }
            else
            {
                if (s.Trim() == String.Empty)
                {
                    r = true;
                }
            }

            return r;
        }

        public ScheduleItem GetScheduleItem()
        {
            ScheduleItem r = null;

            if (resourceType == ResourceType.UNKNOWN)
                throw new Exception("Unknown Resource Type");
            else
            {
                if (si == null)
                {
                    si = sif.GetScheduleItem(resourceType);

                    si.LocalName = Filename;

                    if (Target != null)
                    {
                        si.TargetDate = Target;
                    }

                    if (si.GetType() == typeof(WebScheduleItem))
                    {
                        ((WebScheduleItem)si).Link = Resource;
                    }

                    if (si.GetType() == typeof(FtpScheduleItem))
                    {
                        ((FtpScheduleItem)si).Hostname = Host;
                        ((FtpScheduleItem)si).Username = User;
                        ((FtpScheduleItem)si).Password = Pass;
                        ((FtpScheduleItem)si).RemoteName = Resource;
                        ((FtpScheduleItem)si).LocalName = Filename;
                    }
                }

                r = si;
            }

            return r;
        }
    }
}