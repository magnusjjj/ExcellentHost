using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using STUN;
using STUN.Attributes;
using WebSocketSharp;



namespace ExcellentHost
{
    class Program
    {
        static Dictionary<string, ClientThread> clientThreads = new Dictionary<string, ClientThread>();
        static List<string> debugOutput = new List<string>();

        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint local_ipendpoint = new IPEndPoint(IPAddress.Any, 1025); //new IPEndPoint(IPAddress.Any, 1025);
            socket.Bind(local_ipendpoint);
            SimpleSTUN.OnDebug += SimpleSTUN_OnDebug;
            STUNQueryResult queryResult = SimpleSTUN.DoSTUN(socket);

            WebSocket webSocket = new WebSocket("ws://tuxie.nu:10000/ws/excellenthost/");
            webSocket.OnMessage += (sender, eventArgs) =>
            {
            //Console.WriteLine(eventArgs.Data);
            MessageBase messageBase = JsonSerializer.Deserialize<MessageBase>(eventArgs.Data);
            switch (messageBase.type)
            {
              case "EHCONNECT":
                  EHCONNECT ehconnect = JsonSerializer.Deserialize<EHCONNECT>(eventArgs.Data);
                  string connectstring = ehconnect.ip + ":" + ehconnect.port.ToString();
                  if(!clientThreads.ContainsKey(connectstring))
                  {
                      ClientThread ct = new ClientThread();
                      ct.StartClient(ehconnect.ip, ehconnect.port);
                      clientThreads.Add(connectstring, ct);
                  }
                  else
                  {
                      string json = JsonSerializer.Serialize(new EHCONNECT(queryResult.PublicEndPoint.Address.ToString(), queryResult.PublicEndPoint.Port), typeof(EHCONNECT), new JsonSerializerOptions());
                      webSocket.Send(json);
                  }

                  break;
            }
            };

            webSocket.Connect();

            if(queryResult != null)
            {
                new ServerThread().StartServer(socket);
                string json = JsonSerializer.Serialize(new EHCONNECT(queryResult.PublicEndPoint.Address.ToString(), queryResult.PublicEndPoint.Port), typeof(EHCONNECT), new JsonSerializerOptions());
                webSocket.Send(json);
            }

            while(true){ 
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Thread status");
                Console.ForegroundColor = ConsoleColor.White;

                foreach (KeyValuePair<string, ClientThread> thread in clientThreads)
                {
                   Console.WriteLine(thread.Value.Status);
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Debug output".PadRight(80));
                Console.ForegroundColor = ConsoleColor.White;

                foreach (string line in debugOutput)
                {
                   Console.WriteLine(line);
                }



                Thread.Sleep(1000);
                Console.SetCursorPosition(0,0);
            }
        }

        private static void SimpleSTUN_OnDebug(object sender, string e)
        {
            debugOutput.Add(e);
        }

        Dictionary<int, List<Tuple<int, string, DateTime>>> debugmessages;
        Dictionary<int, List<Tuple<DateTime, int, int>>> ping;

        public class MessageBase
        {
            public string type { get; set; } = "Sometype";
        }

        [Serializable]
        public class EHCONNECT : MessageBase
        {
            public string type { get; set; } = "EHCONNECT";
            public string ip { get; set; } = "0.0.0.0";
            public int port { get; set; } = 0;

            public EHCONNECT() // Needed for deserialization
            {
            }

            public EHCONNECT(string ip, int port)
            {
                this.ip = ip;
                this.port = port;
            }
        }
    }
}