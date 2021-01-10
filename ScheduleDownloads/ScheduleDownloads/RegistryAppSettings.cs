using Microsoft.Win32;

namespace ScheduleDownloads
{
    public class RegistryAppSettings : AppSettings
    {
        private RegistryKey rk;
        private RegistryKey sk;

        private string KeyAddress { get; set; }

        public RegistryAppSettings(string regkey) : base()
        {
            KeyAddress = "SOFTWARE\\" + regkey;

            Load();
        }

        protected override void Read()
        {
            rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            sk = rk.OpenSubKey(KeyAddress, false);

            if (sk != null)
            {
                DefaultDirectory = sk.GetValue("DefaultDir").ToString();

                sk.Close();
            }
        }

        protected override void Write()
        {
            rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            sk = rk.OpenSubKey(KeyAddress, true);

            if (sk != null)
            {
                sk = rk.CreateSubKey(KeyAddress);
            }

            sk.SetValue("DefaultDir", DefaultDirectory);

            sk.Close();
        }
    }
}