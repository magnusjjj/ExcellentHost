using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ExcellentHost
{
    class ServerThread
    {
        private Thread thread;
        private bool running = false;
        private Socket socket;

        private void Main()
        {
            running = true;


            while (running)
            {
                byte[] buffer = new byte[8];
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 1025);
                socket.ReceiveFrom(buffer, ref remoteEndPoint);
                socket.SendTo(buffer, remoteEndPoint);
                //Console.Out.WriteLineAsync("Received a packet from " + remoteEndPoint.ToString());
            }
        }

        public void StartServer(Socket sock)
        {
            socket = sock;
            thread = new Thread(Main);
            thread.Start();
        }
    }
}
