using System;
using System.Net;
using System.Net.Sockets;
using STUN;
using STUN.Attributes;

namespace ExcellentHost
{
    internal class SimpleSTUN
    {
        public static event EventHandler<string> OnDebug;

        public static STUNQueryResult DoSTUN(Socket sock)
        {
            string[] stunhosts = {"stun.l.google.com:19302", "galvinism.ink:3478"};

            foreach (var stunhost in stunhosts)
                try
                {
                    var ihe = Dns.GetHostEntry(stunhost.Split(":")[0]);
                    IPAddress targetip = null;
                    foreach (IPAddress ip in ihe.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                            targetip = ip;
                    }

                    IPEndPoint stunEndPoint = new IPEndPoint(targetip, int.Parse(stunhost.Split(":")[1]));

//                    if (!STUNUtils.TryParseHostAndPort(stunhost, out var stunEndPoint))
//                        throw new Exception("Failed to resolve STUN server address");

                    STUNClient.ReceiveTimeout = 500;
                    var queryResult = STUNClient.Query(sock, stunEndPoint, STUNQueryType.ExactNAT,
                        NATTypeDetectionRFC.Rfc3489);

                    if (queryResult.QueryError != STUNQueryError.Success)
                        throw new Exception("Query Error: " + queryResult.QueryError);


                    Debug(string.Format("PublicEndPoint: {0}", queryResult.PublicEndPoint));
                    Debug(string.Format("LocalEndPoint: {0}", queryResult.LocalEndPoint));
                    Debug(string.Format("NAT Type: {0}", queryResult.NATType));

                    return queryResult;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not contact STUN host " + stunhost + " " + e.Message);
                }

            return null;
        }

        public static void Debug(string input)
        {
            OnDebug?.Invoke(null, input);
        }
    }
}