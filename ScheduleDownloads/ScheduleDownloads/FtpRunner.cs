using System.Net;
using WinSCP;

namespace ScheduleDownloads
{
    class FtpRunner : Runner
    {
        private SessionOptions so;

        private string site;
        private string user;
        private string passw;

        private string remoteFile;

        public string RemoteFile
        {
            get { return remoteFile; }

            protected set
            {
                remoteFile = value;
            }
        }

        private string localFile;

        public string LocalFile
        {
            get { return localFile; }

            protected set
            {
                localFile = value;
            }
        }

        public FtpRunner(FtpScheduleItem fsi)
        {
            this.site = fsi.Hostname;
            this.user = fsi.Username;
            this.passw = fsi.Password;
            this.remoteFile = fsi.RemoteName;
            this.localFile = fsi.LocalName;

            InitialFields();
        }

        protected void InitialFields()
        {
            so = new SessionOptions
            {
                Protocol = Protocol.Ftp,
                HostName = site,
                UserName = user,
                Password = passw
            };

            // SshHostKeyFingerprint = "ssh-rsa 2048 xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx"

            progress = 0;
        }

        private void ProgressChangedCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            progress = (double)e.ProgressPercentage;
        }

        ~FtpRunner() { }

        public override void Start()
        {
            using (Session session = new Session())
            {
                // Connect
                session.Open(so);

                TransferOptions transferOptions = new TransferOptions();
                transferOptions.TransferMode = TransferMode.Binary;

                TransferOperationResult transferResult;
                transferResult = session.GetFiles(RemoteFile, DownloadDir + @"\" + LocalFile, false, transferOptions);

                // Throw on any error
                transferResult.Check();

                // Print results
                /*foreach (TransferEventArgs transfer in transferResult.Transfers)
                {
                    Console.WriteLine("Upload of {0} succeeded", transfer.FileName);
                }*/
            }
        }
    }
}