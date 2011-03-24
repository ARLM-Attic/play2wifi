using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Text;

namespace play2wifi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ushort airplayPort = 22555;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            registerZeroconfService();
            startTcpServer();
        }
        private static void registerZeroconfService()
        {
            //var service = new Mono.Zeroconf.RegisterService();
            var service = new Mono.Zeroconf.Providers.Bonjour.RegisterService();
            service.Name = Environment.MachineName.ToLower();
            service.RegType = "_airplay._tcp";
            service.ReplyDomain = "local.";
            service.UPort = airplayPort;
            service.Register();
        }
        protected void startTcpServer()
        {
            new Play2WifiServer { Port = airplayPort }.Start();
        }
    }
    public class Play2WifiServer
    {
        public int Port { get; set; }
        private System.Net.Sockets.TcpListener server;
        public void Start()
        {
            server = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, Port);
            server.Start();
            //task = System.Threading.Tasks.Task.Factory.StartNew(()=> {while(true) {
            //    server.BeginAcceptTcpClient(processClient, server);
            //}});
            server.BeginAcceptTcpClient(processClient, server);            
        }
        void processClient(IAsyncResult ar)
        {
            server.BeginAcceptTcpClient(processClient, server);
            
            var client = server.EndAcceptTcpClient(ar);
            System.Diagnostics.Debug.WriteLine("connection from "+client.Client.RemoteEndPoint+" started");
            var stream = client.GetStream();
            using (var reader = new System.IO.StreamReader(stream))
            using (var writer = new System.IO.StreamWriter(stream))
            {
                string line="";
                StringBuilder request = new StringBuilder();
                //process in http style chunks separated by blank lines
                while (client.Connected && stream.CanRead && null!=line )
                {
                    line=reader.ReadLine();
                    if (line == "" || line==null && request.Length>0)
                    {
                        dataReceived(request.ToString(), writer);
                        request = new StringBuilder();
                    }
                    else
                        request.AppendLine(line);
                }
            }
            System.Diagnostics.Debug.WriteLine("connection  ended");
        }
        void dataReceived(string data, System.IO.StreamWriter writer)
        {
            
            StringBuilder answer= new StringBuilder();
            //		data	"POST /reverse HTTP/1.1\r\nUpgrade: PTTH/1.0\r\nConnection: Upgrade\r\nX-Apple-Purpose: event\r\nContent-Length: 0\r\nUser-Agent: MediaControl/1.0\r\nX-Apple-Session-ID: 06d8ce5e-0783-448d-972c-f89e9d4307d9\r\n\r\n"	string
            if (data.Contains("reverse"))
            {
                answer.AppendLine("HTTP/1.1 101 Switching Protocols");
                answer.Append("Date: ");
                //answer.AppendLine(DateTime.Now.ToString("ddd, dd MMM yyyy HH:mm:ss GMT"));
                answer.AppendLine(DateTime.Now.ToString("r"));
                answer.AppendLine("Upgrade: PTTH/1.0");
                answer.AppendLine("Connection: Upgrade");
                answer.AppendLine();
            }
            else if (data.Contains("Content-Location"))
            {
                playMedia(data);
                answer.AppendLine("HTTP/1.1 200 OK");
                answer.Append("Date: ");
                //answer.AppendLine(DateTime.Now.ToString("ddd, dd MMM yyyy HH:mm:ss GMT"));
                answer.AppendLine(DateTime.Now.ToString("r"));
                answer.AppendLine("Content-Length: 0");
                answer.AppendLine();
            }
            else if (data.Contains("/server-info"))
            {
                respondNotFound(answer);
            }
            else if (data.Contains("POST /scrub"))
            {
                //setPlayerPosition(data);
                respondOk(answer);
            }
            else if(data.Contains("POST /rate"))
            {
                respondOk(answer);
            }
            else if (data.Contains("GET /playback-info"))
            {
                respondNotFound(answer);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Got Something Else");
                System.Diagnostics.Debug.WriteLine(data);
                respondOk(answer);
            }
            if (answer.Length != 0)
            {
                writer.Write(answer.ToString());
                writer.Flush();
                System.Diagnostics.Debug.WriteLine("Got data");
                System.Diagnostics.Debug.WriteLine(data);
                System.Diagnostics.Debug.WriteLine("sent response:");
                System.Diagnostics.Debug.WriteLine(answer.ToString());
            }
            else throw new NotImplementedException("deadcode");
        }
        protected void playMedia(string data)
        {
            var url = data.Split(new[] { "Content-Location: ", "\r\nStart-Position: " }, StringSplitOptions.RemoveEmptyEntries)[0];
            System.Diagnostics.Process.Start("wmplayer.exe", url);
            System.Diagnostics.Debug.WriteLine("starting wmplayer.exe "+ url);
        }
        private static void respondOk(StringBuilder answer)
        {
            answer.AppendLine("HTTP/1.1 200 OK");
            dateAndSizeZero(answer);
        }
        private static void dateAndSizeZero(StringBuilder answer)
        {
            answer.Append("Date: ");
            answer.AppendLine(DateTime.Now.ToString("r"));
            answer.AppendLine("Content-Length: 0");
            answer.AppendLine();
        }
        private static void respondNotFound(StringBuilder answer)
        {
            answer.AppendLine("HTTP/1.1 404 Not Found");
            dateAndSizeZero(answer);
        }
    }
}
