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
        

        static void Main(string[] args)
        {
            // Initialise and connect to the TcpClient
            // On server: 145.48.6.10 and port: 6666
            TcpClient client = new TcpClient();
            client.Connect("145.48.6.10", 6666);

            // Request the session list from the server
            string data = @"{""id"" : ""session/list""}";

            NetworkStream networkStream = client.GetStream();           

            sendToTcp(networkStream, data);
            // receive the response
            string receivedData;
            ReceiveFromTcp(networkStream, out receivedData);

            // parse the received data
            dynamic jsonData = JsonConvert.DeserializeObject(receivedData);
            Console.WriteLine(jsonData.id);

            JArray jsonDataArray = jsonData.data;
            Console.WriteLine(jsonDataArray);

            Dictionary<string, string> userSessionsMap = new Dictionary<string, string>();
            foreach (JObject jObject in jsonDataArray)
            {
                if (jObject.ContainsKey("id") && jObject.ContainsKey("clientinfo"))
                {
                    string user = jObject["clientinfo"]["user"].ToString();
                    Console.WriteLine(user);
                    Console.WriteLine(jObject["id"]);
                    // the dictionary doesn't like duplicates
                    if (!userSessionsMap.ContainsKey(user))
                    {
                        userSessionsMap.Add(jObject["clientinfo"]["user"].ToString(), jObject["id"].ToString());
                    }
                    
                }
            }

            // get user input for which session to connect to
            Console.WriteLine("Which client should be connected to?");
            string userInput = Console.ReadLine();
            if (userSessionsMap.ContainsKey(userInput))
            {
                Console.WriteLine("Creating a tunnel");
                // create a tunnel
                string tunnelCreationDataString = @"{""id"" : ""tunnel/create"", 
                                                        ""data"" : 
                                                        {
                                                            ""session"" : """ + userSessionsMap.GetValueOrDefault(userInput) + @""",
                                                            ""key"" : """"
                                                        }
                                                    }";
                sendToTcp(networkStream, tunnelCreationDataString);
                string tunnelCreationResponse;
                ReceiveFromTcp(networkStream, out tunnelCreationResponse);
            } else
            {
                Console.WriteLine("That client isn't recognised");
            }
        }

        public static void sendToTcp(NetworkStream networkStream, string data)
        {
            byte[] dataBytes = System.Text.Encoding.ASCII.GetBytes(data);
            int dataLength = dataBytes.Length;

            networkStream.Write(BitConverter.GetBytes(dataLength));
            networkStream.Write(dataBytes);
        }

        public static void ReceiveFromTcp(NetworkStream networkStream, out string receivedData)
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
            Console.WriteLine(receivedData);
        }
    }
}
