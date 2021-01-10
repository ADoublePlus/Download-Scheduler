using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ScheduleDownloads
{
    public class TCPServer
    {
        private IPHostEntry hostInfo;
        private uint port = 9999;
        private TcpListener listener;
        private bool listening = false;

        private Task serverThread;
        private ArrayList clientList;

        public string Host
        {
            get { return hostInfo.HostName; }

            set
            {
                if (value != "")
                {
                    if (value.ToLower() == "localhost")
                    {
                        UseLocalhost();
                    }
                    else
                    {
                        hostInfo = Dns.GetHostEntry(value.ToLower());
                    }

                    NewEndpoint();
                }
            }
        }

        public uint Port
        {
            get { return port; }

            set
            {
                if ((value > 0) && (value < 65536))
                {
                    port = value;

                    NewEndpoint();
                }
            }
        }

        public bool Listening
        {
            get { return listening; }
        }

        public delegate void LogHandler(TCPServer server, LogEventArgs args);
        public event LogHandler OnLog;

        public TCPServer()
        {
            InitialFields();
        }

        public TCPServer(uint port)
        {
            InitialFields();

            hostInfo = new IPHostEntry();
            hostInfo.AddressList = new IPAddress[1];

            UseLocalhost();

            this.port = port;

            NewEndpoint();
        }

        public TCPServer(string host, uint port)
        {
            InitialFields();

            hostInfo = new IPHostEntry();
            hostInfo.AddressList = new IPAddress[1];

            this.Host = host;
            this.Port = port;
        }

        ~TCPServer()
        {
            if (listening)
            {
                this.Stop();
            }
        }

        private void InitialFields()
        {
            clientList = new ArrayList();
        }

        private void log(string s)
        {
            if (OnLog != null)
            {
                OnLog(this, new LogEventArgs(s));
            }
        }

        private void UseLocalhost()
        {
            byte[] ip = { 127, 0, 0, 1 };

            hostInfo.AddressList[0] = new IPAddress(ip);
            hostInfo.HostName = "localhost";
        }

        private void NewEndpoint()
        {
            try
            {
                log("Re-building host & port");
                listener = new TcpListener(new IPEndPoint(hostInfo.AddressList[0], (int)port));
            }

            catch (Exception e)
            {
                listener = null;
            }
        }

        public void Start()
        {
            if ((listener != null) && (!listening))
            {
                log("Starting server listener");
                listening = true;
                listener.Start();
                serverThread = new Task(delegate { ServerListener(); });
                serverThread.Start();
            }
        }

        public void Stop()
        {
            if ((listener != null) && (listening))
            {
                log("Stopping server listener");
                listening = false;
                listener.Stop();
                // TODO - cancel thread, set var to null
            }
        }

        public void ClientLog(TCPServerClient t, LogEventArgs e)
        {
            log(e.Text);
        }

        private void ServerListener()
        {
            Socket clientSocket = null;
            TCPServerClient clientHandler;

            while (listening)
            {
                try
                {
                    // Wait for any client requests and if there is any
                    // request from any client accept it (wait indefinitely)
                    clientSocket = listener.AcceptSocket();

                    // Create a socket listener object for the client
                    clientHandler = new TCPServerClient(clientSocket);
                    clientHandler.OnLog += ClientLog;

                    // Add the socket listener to an array list in a thread safe fashion
                    lock (clientList)
                    {
                        clientList.Add(clientHandler);
                    }

                    // Start communicating with the client in a different thread
                    clientHandler.Start();
                }

                catch (SocketException se)
                {
                    //throw;
                }
            }
        }
    }
}