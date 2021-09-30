﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace VirtualReality
{
    public class Connection
    {
        private NetworkStream networkStream;
        public string currentSessionID { get; set; }

        private delegate void Reconnect();

        private Reconnect reconnect;

        private static Random random = new Random();

        public Connection(NetworkStream networkStream, VrManager vrManager)
        {
            this.networkStream = networkStream;
            currentSessionID = "";
            //Sets a timeout if this time is hit a timeout exception will be thrown
            networkStream.ReadTimeout = 10000;
            reconnect = vrManager.Reconnect;
            Thread t = new Thread(Run);
            t.Start();
        }


        /// <summary>ReceiveFromTcp does <c>recieving data from a tcp stream</c> using a network stream decodes using ASCII to a string</summary>
        public void ReceiveFromTcp(out string receivedData,bool useTimeOut)
        {
            if (useTimeOut)
            {
                networkStream.ReadTimeout = 10000;
            }
            else
            {
                networkStream.ReadTimeout = -1;
            }



            // read a small part of the packet and receive the packet length
            byte[] buffer = new byte[4];
            int rc = 0;

            try
            {
                rc = networkStream.Read(buffer, 0, 4);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (rc > 0)
            {
                // read from the stream until the entire packet is written to the buffer
                int packetLength = BitConverter.ToInt32(buffer);
                byte[] packetBuffer = new byte[packetLength];
                int receivedTotal = 0;
                while (receivedTotal < packetLength)
                {
                    rc = networkStream.Read(packetBuffer, receivedTotal, packetLength - receivedTotal);
                    receivedTotal += rc;
                }

                receivedData = System.Text.Encoding.ASCII.GetString(packetBuffer);
            }
            else
            {
                receivedData = String.Empty;
                reconnect();
            }
        }

        /// <summary>SendToTcp does <c>Sending a String over Tcp</c> using ASCII encoding</summary>
        ///
        public void SendToTcp(string data)
        {
            byte[] dataBytes = System.Text.Encoding.ASCII.GetBytes(data);
            int dataLength = dataBytes.Length;

            networkStream.Write(BitConverter.GetBytes(dataLength));
            networkStream.Write(dataBytes);
            networkStream.Flush();
        }


        /// <summary>SendViaTunnel does <c> a tcp data send via a tunnel</c> as long as you have made a connection first </summary>
        /// Returns a string with the response
        public void SendViaTunnel(JObject jObject, Callback callback = null)
        {
            if (currentSessionID.Length == 0)
            {
                Console.WriteLine("not connected");
            }
            else
            {
                string randomIntAsString = random.Next(111111, 999999).ToString();
                JObject tunnelJSon = new JObject();
                tunnelJSon.Add("id", "tunnel/send");
                JObject tunnelJObject = new JObject();
                tunnelJObject.Add("dest", currentSessionID);

                jObject.Add("serial", randomIntAsString);

                if (callback != null)
                {
                    callbacks.Add(randomIntAsString, callback);
                }

                tunnelJObject.Add("data", jObject);
                tunnelJSon.Add("data", tunnelJObject);
                //Console.WriteLine(tunnelJSon.ToString());
                SendToTcp(tunnelJSon.ToString());
            }
        }

        public delegate void Callback(string response);
        private Dictionary<string, Callback> callbacks = new Dictionary<string, Callback>();

        /// <summary>
        /// entry of the network thread
        /// </summary>
        void Run()
        {
            bool running = true;
            while (running)
            {
                if (currentSessionID.Length > 1)
                {
                    ReceiveFromTcp(out var receivedData,false);
                    Console.WriteLine(receivedData);

                    JObject tunnel = JObject.Parse(receivedData);
                    JObject idObject = (JObject) tunnel.GetValue("data");
                    JObject dataObject = (JObject) idObject.GetValue("data");
                    if (dataObject.ContainsKey("serial"))
                    {
                        JToken jToken = dataObject.GetValue("serial");
                        string serial = jToken.ToString();
                        Console.WriteLine(serial);

                        if (callbacks.ContainsKey(serial))
                        {
                            callbacks[serial](dataObject.ToString());
                            callbacks.Remove(serial);
                        }
                    }
                    
                }
            }
        }
    }
}