using System;
using System.Net;
using System.IO;

namespace ScheduleDownloads
{
    public class HttpRunner : Runner
    {
        private WebClient wc;

        private string link;

        public string Link
        {
            get { return link; }
        }

        private string localName;

        public string LocalName
        {
            get { return localName; }

            set
            {
                localName = value;
            }
        }

        public HttpRunner(WebScheduleItem wsi)
        {
            InitialFields();

            this.link = wsi.Link;
        }

        protected void InitialFields()
        {
            wc = new WebClient();
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChangedCallback);
            progress = 0;
        }

        private void ProgressChangedCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            progress = (double)e.ProgressPercentage;

            if (progress == 100)
            {
                RenameFile(DownloadDir + @"\" + localName + ".part", DownloadDir + @"\" + localName);
            }
        }

        ~HttpRunner()
        {
            if (wc != null)
            {
                wc.Dispose();
            }
        }

        public override void Start()
        {
            if (File.Exists(DownloadDir + @"\" + localName + ".part"))
                throw new IOException("Local file already exists");

            wc.DownloadFileAsync(new Uri(link), DownloadDir + @"\" + localName + ".part");
        }

        public void Cancel()
        {
            wc.CancelAsync();
        }
    }
}