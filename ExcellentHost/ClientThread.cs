using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using STUN;
using STUN.Attributes;

namespace ExcellentHost
{
    class ClientThread
    {
        private Thread thread = null;
        private UInt32 current = 0;
        private bool running = false;
        private UdpClient socket;
        private List<int> completedpings;
        private Dictionary<UInt32, Stopwatch> pendingpings = new Dictionary<uint, Stopwatch>();
        private byte[] readbuffer = new byte[8];

        void OnUdpData(IAsyncResult result)
        {
            // this is what had been passed into BeginReceive as the second parameter:
            UdpClient socket = result.AsyncState as UdpClient;
            // points towards whoever had sent the message:
            IPEndPoint source = new IPEndPoint(0, 0);
            // get the actual message and fill out the source:
            byte[] message = socket.EndReceive(result, ref source);
            // do what you'd like with `message` here:
            Console.WriteLine("Got " + message.Length + " bytes from " + source);
            // schedule the next receive operation once reading is done:
            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);

            string type = Encoding.ASCII.GetString(message[Range.EndAt(4)]);
            UInt32 number = BitConverter.ToUInt32(message[Range.StartAt(4)]);
            Stopwatch s = pendingpings[number];
            s.Stop();

            Console.WriteLine("Got answer for " + number.ToString() + " elapsed time " + s.Elapsed.ToString());
            pendingpings.Remove(number);
        }

        private void Main(object parameters)
        {
            running = true;

            Tuple<string, int> input = (Tuple<string, int>) parameters;

            socket = new UdpClient();

            socket.Connect(new IPEndPoint(IPAddress.Parse(input.Item1), input.Item2));
            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);

            while (running)
            {
                SendPing();
                Console.WriteLine("Number of pings waiting for result: " + pendingpings.Count.ToString());
                Thread.Sleep(1000);
            }
        }

        private void SendPing()
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            pendingpings[current] = s;
            socket.Send(Encoding.ASCII.GetBytes("PING").Concat(BitConverter.GetBytes(current)).ToArray(), 8);
            current++;
        }

        public void StartClient(string address, int port)
        {
            thread = new Thread(Main);
            thread.Start(new Tuple<string, int>(address, port));
        }
    }
}
