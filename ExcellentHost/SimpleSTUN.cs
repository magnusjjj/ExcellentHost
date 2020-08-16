﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using STUN;
using STUN.Attributes;

namespace ExcellentHost
{
    class SimpleSTUN
    {
        public static event EventHandler<string> OnDebug;
        public static STUNQueryResult DoSTUN(Socket sock)
        {
            string[] stunhosts = new[] { "stun.l.google.com:19302", "galvinism.ink:3478" };

            foreach (string stunhost in stunhosts)
            {
                try
                {
                    if (!STUNUtils.TryParseHostAndPort(stunhost, out IPEndPoint stunEndPoint))
                        throw new Exception("Failed to resolve STUN server address");

                    STUNClient.ReceiveTimeout = 500;
                    var queryResult = STUNClient.Query(sock, stunEndPoint, STUNQueryType.ExactNAT, NATTypeDetectionRFC.Rfc3489);

                    if (queryResult.QueryError != STUNQueryError.Success)
                        throw new Exception("Query Error: " + queryResult.QueryError.ToString());

                    
                    Debug(String.Format("PublicEndPoint: {0}", queryResult.PublicEndPoint));
                    Debug(String.Format("LocalEndPoint: {0}", queryResult.LocalEndPoint));
                    Debug(String.Format("NAT Type: {0}", queryResult.NATType));
                    
                    return queryResult;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not contact STUN host " + stunhost + " " + e.Message);
                }
            }

            return null;
        }

        public static void Debug(string input)
        {
            SimpleSTUN.OnDebug?.Invoke(null, input);
        }
    }
}
