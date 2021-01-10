using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace ScheduleDownloads
{
    /// <summary>
    /// Summary description for TCPServerClient.
    /// </summary>
    public class TCPServerClient
    {
        private FormatInterpreter fi;

        private Socket clientSocket;
        private Task clientHandlerThread;
        private bool stopClient = false;

        private bool markedForDeletion = false;

        /// <summary>
        /// Working variables.
        /// </summary>
        private StringBuilder oneLineBuf = new StringBuilder();

        private DateTime lastReceiveDateTime;
        private DateTime currentReceiveDateTime;

        private char[] delims = { '|' };

        public delegate void LogHandler(TCPServerClient server, LogEventArgs args);
        public event LogHandler OnLog;

        public string RemoteHost { get; protected set; }
        public int RemotePort { get; protected set; }

        /// <summary>
        /// Client socket listener constructor.
        /// </summary>
        /// <param name="clientSocket"></param>
        public TCPServerClient(Socket clientSocket)
        {
            this.clientSocket = clientSocket;

            RemoteHost = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();
            RemotePort = ((IPEndPoint)clientSocket.RemoteEndPoint).Port;

            fi = new FormatInterpreter();
        }

        /// <summary>
        /// Client socket listener destructor.
        /// </summary>
        ~TCPServerClient()
        {
            StopSocket();
        }

        /// <summary>
        /// Method that sends a message to the log.
        /// </summary>
        public void Log(string s)
        {
            if (OnLog != null)
            {
                OnLog(this, new LogEventArgs(s));
            }
        }

        /// <summary>
        /// Method that starts socket listener thread.
        /// </summary>
        public void Start()
        {
            if (clientSocket != null)
            {
                Task clientHandlerThread = new Task(delegate { clientHandler(); });
                clientHandlerThread.Start();
            }
        }

        /// <summary>
        /// Thread method that does the communication to the client.
        /// </summary>
        private void clientHandler()
        {
            uint timeoutMs = 1000 * 45;
            int size = 0;
            Byte[] byteBuffer = new Byte[4096];

            Log("New connection from " + RemoteHost + ":" + RemotePort);

            lastReceiveDateTime = DateTime.Now;
            currentReceiveDateTime = DateTime.Now;

            Timer t = new System.Threading.Timer(new TimerCallback(CheckClientCommInterval), null, timeoutMs, timeoutMs);

            while (!stopClient)
            {
                try
                {
                    size = clientSocket.Receive(byteBuffer);
                    currentReceiveDateTime = DateTime.Now;
                    ParseReceiveBuffer(byteBuffer, size);
                }

                catch (SocketException se)
                {
                    stopClient = true;
                    markedForDeletion = true;
                }
            }

            t.Change(Timeout.Infinite, Timeout.Infinite);
            t = null;

            if (clientSocket != null)
            {
                clientSocket.Close();
            }

            Log("Closed connection from " + RemoteHost + ":" + RemotePort);
        }

        /// <summary>
        /// Method that stops client socket listening thread.
        /// </summary>
        public void StopSocket()
        {
            if (clientSocket != null)
            {
                stopClient = true;
                clientSocket.Close();
                Log("Stop client socket");

                // Wait for one second for the thread to stop
                if (clientHandlerThread != null)
                {
                    clientHandlerThread.Wait();
                }

                clientHandlerThread = null;
                clientSocket = null;
                markedForDeletion = true;
            }
        }

        /// <summary>
        /// Method that returns the state of this object i.e. whether this
        /// object is marked for deletion or not.
        /// </summary>
        public bool IsMarkedForDeletion() { return markedForDeletion; }

        /// <summary>
        /// This method parses data that is sent by a client using TCP/IP.
        /// </summary>
        /// <param name="byteBuffer"></param>
        /// <param name="size"></param>
        private void ParseReceiveBuffer(Byte[] byteBuffer, int size)
        {
            string data = Encoding.ASCII.GetString(byteBuffer, 0, size);
            int lineEndIndex = 0;

            // Check whether data from client has more than one line of
            // information, where each line of information ends with "CRLF"
            // ("\r\n"). If so, break data into different lines and process separately.

            do
            {
                lineEndIndex = data.IndexOf("\r\n");

                if (lineEndIndex != -1)
                {
                    oneLineBuf = oneLineBuf.Append(data, 0, lineEndIndex + 2);
                    ProcessClientRequest(oneLineBuf.ToString());
                    oneLineBuf.Remove(0, oneLineBuf.Length);
                    data = data.Substring(lineEndIndex + 2, data.Length - lineEndIndex - 2);
                }
                else
                {
                    // Append to the existing buffer
                    oneLineBuf = oneLineBuf.Append(data);
                }
            } while (lineEndIndex != -1);
        }

        /// <summary>
        /// Method that processes the client data as per the protocol.
        /// </summary>
        /// <param name="oneLine"></param>
        private void ProcessClientRequest(String oneLine)
        {
            string reply = String.Empty;
            Scheduler sc = Scheduler.Instance;
            ScheduleItem si;

            try
            {
                Log("Received data: " + oneLine);
                fi.ParseString(oneLine);

                stopClient = (fi.Command == CommandType.FIN);

                if (!stopClient)
                {
                    switch (fi.Command)
                    {
                        case CommandType.STAT:
                            reply = sc.GetStatus();
                            reply += "[OK]\r\n";
                            break;

                        case CommandType.NQUE:
                            si = sc.Add(fi.GetScheduleItem());
                            reply = "[OK]\r\n";
                            break;

                        /*case CommandType.DQUE:
                            // TODO
                            reply = "[Not implemented as of yet]\r\n";
                            break;*/

                        default:
                            reply = "[Unrecognized command]\r\n";
                            break;
                    }
                }

                byte[] sData = System.Text.Encoding.ASCII.GetBytes(reply);
                clientSocket.Send(sData);
            }

            catch (SocketException se) { throw; }
        }

        /// <summary>
        /// Checks whether there are any client calls for the last xx seconds.
        /// </summary>
        /// <param name="o"></param>
        private void CheckClientCommInterval(object o)
        {
            if (lastReceiveDateTime.Equals(currentReceiveDateTime))
            {
                this.StopSocket();
            }
            else
            {
                lastReceiveDateTime = currentReceiveDateTime;
            }
        }
    }
}