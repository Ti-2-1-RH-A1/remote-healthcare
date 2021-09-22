using System;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;

namespace VirtualReality
{
    public class Connection
    {
        private NetworkStream networkStream;
        public string currentSessionID { get; set; }

        private delegate void Reconnect();

        private Reconnect reconnect;

        public Connection(NetworkStream networkStream, VrManager vrManager)
        {
            this.networkStream = networkStream;
            //Sets a timeout if this time is hit a timeout exception will be thrown
            networkStream.ReadTimeout = 1000;
            reconnect = vrManager.Reconnect;
        }


        /// <summary>ReceiveFromTcp does <c>recieving data from a tcp stream</c> using a network stream decodes using ASCII to a string</summary>
        public void ReceiveFromTcp(out string receivedData)
        {
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

        public string SendViaTunnel(JObject jObject)
        {
            if (currentSessionID.Length == 0)
            {
                return "Not Connected to a Tunnel";
            }
            else
            {
                JObject tunnelJSon = new JObject();
                tunnelJSon.Add("id", "tunnel/send");
                JObject tunnelJObject = new JObject();
                tunnelJObject.Add("dest", currentSessionID);
                tunnelJObject.Add("data", jObject);
                tunnelJSon.Add("data", tunnelJObject);
                //Console.WriteLine(tunnelJSon.ToString());
                SendToTcp(tunnelJSon.ToString());


                ReceiveFromTcp(out var receivedData);
                return receivedData;
            }
        }

        void run()
        {

            while (currentSessionID.Length > 0)
            {
                

            }




        }

    }
}