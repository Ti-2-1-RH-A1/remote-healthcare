using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace VirtualReality
{
    class Program
    {


        private NetworkStream networkStream;
        private Dictionary<string, string> userSessionsMap;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.start();
        }

        public Program()
        {
            // Initialise and connect to the TcpClient
            // On server: 145.48.6.10 and port: 6666
            TcpClient client = new TcpClient();
            client.Connect("145.48.6.10", 6666);
            // Request the session list from the server
            networkStream = client.GetStream();
            userSessionsMap = getRunningSessions();
        }


        public void start()
        {
            
            foreach (KeyValuePair<string, string> keyValuePair in userSessionsMap)
            {
                Console.WriteLine(keyValuePair.ToString());
            }

            // get user input for which session to connect to
            Console.WriteLine("Which client should be connected to?");
            string userInput = Console.ReadLine();

            if (createTunnel(userInput))
            {
                Console.WriteLine("Succes connected to "+userInput);
            }
            else
            {
                Console.WriteLine("couldn't connect to that client");
            }

        }

        public Boolean createTunnel(String sessionID)
        {
            if (userSessionsMap.ContainsKey(sessionID))
            {
                Console.WriteLine("Creating a tunnel");
                // create a tunnel
                JObject tunnelCreateJson = new JObject { { "id", "tunnel/create" } };

                JObject dataJson = new JObject { { "session", userSessionsMap.GetValueOrDefault(sessionID) } };
                // place to set the key 
                string sessionKey = "";
                dataJson.Add("key", sessionKey);

                tunnelCreateJson.Add("data", dataJson);


                sendToTcp(networkStream, tunnelCreateJson.ToString());
                string tunnelCreationResponse ="";



                ReceiveFromTcp(networkStream, out tunnelCreationResponse);


                dynamic responseDeserializeObject = JsonConvert.DeserializeObject(tunnelCreationResponse);
                string response = responseDeserializeObject["data"]["status"].ToString();

                if (response != "oke")
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }






        public void sendToTcp(NetworkStream networkStream, string data)
        {
            byte[] dataBytes = System.Text.Encoding.ASCII.GetBytes(data);
            int dataLength = dataBytes.Length;

            networkStream.Write(BitConverter.GetBytes(dataLength));
            networkStream.Write(dataBytes);
        }


        private Dictionary<string, string> getRunningSessions()
        {
            JObject sessionJson = new JObject();
            sessionJson.Add("id", "session/list");
            sendToTcp(networkStream, sessionJson.ToString());

            // receive the response
            string receivedData;
            ReceiveFromTcp(networkStream, out receivedData);

            // parse the received data
            dynamic jsonData = JsonConvert.DeserializeObject(receivedData);

            JArray jsonDataArray = jsonData.data;

            Dictionary<string, string> userSessionsMap = new Dictionary<string, string>();
            foreach (JObject jObject in jsonDataArray)
            {
                if (jObject.ContainsKey("id") && jObject.ContainsKey("clientinfo"))
                {
                    string user = jObject["clientinfo"]["user"].ToString();
                    if (!userSessionsMap.ContainsKey(user))
                    {
                        userSessionsMap.Add(jObject["clientinfo"]["user"].ToString(), jObject["id"].ToString());
                    }
                }
            }


            return userSessionsMap;
        }


        public void ReceiveFromTcp(NetworkStream networkStream, out string receivedData)
        {
            // read a small part of the packet and receive the packet length
            byte[] buffer = new byte[4];
            int rc = networkStream.Read(buffer, 0, 4);
            // read from the stream untill the entire packet is written to the buffer
            int packetLength = BitConverter.ToInt32(buffer);
            byte[] packetBuffer = new byte[packetLength];
            int receivedTotal = 0;
            while (receivedTotal < packetLength)
            {
                rc = networkStream.Read(packetBuffer, receivedTotal, packetLength - receivedTotal);
                receivedTotal += rc;
            }

            receivedData = System.Text.Encoding.ASCII.GetString(packetBuffer);
            //Console.WriteLine(receivedData);
        }
    }
}