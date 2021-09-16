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

            SendToTcp(networkStream, data);
            // receive the response
            string receivedData;
            ReceiveFromTcp(networkStream, out receivedData);

            // parse the received data
            dynamic jsonData = JsonConvert.DeserializeObject(receivedData);
            Console.WriteLine(jsonData.id);

            JArray jsonDataArray = jsonData.data;
            Console.WriteLine(jsonDataArray);

            Dictionary<string, string> userSessionsMap = new Dictionary<string, string>();
            List<string> sessions = new List<string>(); 
            foreach (JObject jObject in jsonDataArray)
            {
                if (jObject.ContainsKey("id") && jObject.ContainsKey("features") && jObject.ContainsKey("clientinfo"))
                {
                    JArray features = (JArray)jObject.GetValue("features");
                    if (features.Count != 0 && features[0].ToString() == "tunnel")
                    {
                        string user = jObject["clientinfo"]["user"].ToString();
                        Console.WriteLine("#" + (sessions.Count));
                        Console.WriteLine(user);
                        Console.WriteLine(jObject["id"]);
                        sessions.Add(jObject["id"].ToString());
                        // the dictionary doesn't like duplicates
                        /*if (!userSessionsMap.ContainsKey(user))
                        {
                            userSessionsMap.Add(jObject["clientinfo"]["user"].ToString(), jObject["id"].ToString());
                        }*/
                    }                                       
                }
            }

            // get user input for which session to connect to
            Console.WriteLine("Which client should be connected to?");
            string userInput = Console.ReadLine();
            string tunnelCreationResponse = "";
            if (/*userSessionsMap.ContainsKey(userInput)*/ int.Parse(userInput) < sessions.Count)
            {
                Console.WriteLine("Creating a tunnel");
                // create a tunnel
                string tunnelCreationDataString = @"{""id"" : ""tunnel/create"", 
                                                        ""data"" : 
                                                        {
                                                            ""session"" : """ + /*userSessionsMap.GetValueOrDefault(userInput)*/ sessions[int.Parse(userInput)] + @""",
                                                            ""key"" : """"
                                                        }
                                                    }";
                SendToTcp(networkStream, tunnelCreationDataString);
                
                ReceiveFromTcp(networkStream, out tunnelCreationResponse);
            } else
            {
                Console.WriteLine("That client isn't recognised");
            }

            dynamic tunnelCreationResponseJsonData = JsonConvert.DeserializeObject(tunnelCreationResponse);

            string id = "";
            if (tunnelCreationResponseJsonData != null)
            {
                id = tunnelCreationResponseJsonData.data.id;
                Console.WriteLine("Id to send to the tunnel: " + id);
            }

            /*ResetScene(networkStream, id);*/
            Dictionary<string, string> nodes = new Dictionary<string, string>();
            GetScene(networkStream, id, ref nodes);
            DeleteNode(networkStream, id, ref nodes);
            /*FindNode(networkStream, id, ref nodes);*/
            ResetScene(networkStream, id);
        }

        public static void DeleteNode(NetworkStream networkStream, string destId, ref Dictionary<string, string> nodes)
        {
            Console.WriteLine("Select a node to remove from the following list:");
            foreach (string s in  nodes.Keys)
            {
                Console.WriteLine(s);
            }
            string userInput = "";
            while (!nodes.ContainsKey(userInput))
            {
                userInput = Console.ReadLine();
            }
            Console.WriteLine("Selected: " + userInput);
            string response;
            /*SendToTunnelWithData(networkStream, destId, "scene/node/delete", @"""id"" : {" + nodes.GetValueOrDefault(userInput) + @"}", out response);*/
            string json = @"{""id"" : ""tunnel/send"",""data"" :{""dest"" : """ + destId + @""",""data"" : {""id"" : ""scene/node/delete"",""data"" :{""id"" : """ + nodes.GetValueOrDefault(userInput) + @"""}}}}";


            SendToTunnel(networkStream, json, out response);
        }

        public static void FindNode(NetworkStream networkStream, string destId, ref Dictionary<string, string> nodes)
        {
            Console.WriteLine("Select a node to find from the following list:");
            foreach (string s in nodes.Keys)
            {
                Console.WriteLine(s);
            }
            string userInput = "";
            while (!nodes.ContainsKey(userInput))
            {
                userInput = Console.ReadLine();
            }
            Console.WriteLine("Selected: " + userInput);
            string response;
            SendToTunnelWithData(networkStream, destId, "scene/node/find", @"""name"" : " + userInput, out response);
        }

        public static void ResetScene(NetworkStream networkStream, string destId)
        {
            string response;
            SendToTunnelWithoutData(networkStream, destId, "scene/reset", out response);
        }

        public static void GetScene(NetworkStream networkStream, string destId, ref Dictionary<string, string> nodes)
        {
            string response;
            SendToTunnelWithoutData(networkStream, destId, "scene/get", out response);

            dynamic responseData = JsonConvert.DeserializeObject(response);
            nodes.Add(responseData.data.data.data.name.ToString(), responseData.data.data.data.uuid.ToString());
            JArray children = responseData.data.data.data.children;
            foreach (JObject jObject in children)
            {
                nodes.Add(jObject.GetValue("name").ToString(), jObject.GetValue("uuid").ToString());
            }
            foreach (string s in nodes.Keys) {
                Console.WriteLine(s);
            }
        }

        public static void SendToTunnel(NetworkStream networkStream, string message, out string response)
        {
            Console.WriteLine("Press any key to send to tunnel");
            Console.ReadLine();
            SendToTcp(networkStream, message);


            ReceiveFromTcp(networkStream, out response);
            Console.WriteLine("Response: " + response);
        }

        public static void SendToTunnelWithData(NetworkStream networkStream, string destId, string id, string data, out string response)
        {
            SendToTunnel(networkStream, 
                @"{
                    ""id"" : ""tunnel/send"",
	                ""data"" :
	                    {
                            ""dest"" : """ + destId + @""",
		                    ""data"" : 
		                        {
                                    ""id"" : """ + id + @""",
                                    ""data"" : {" + data + @"}
                                }
                        }
        }", out response);
        }

        public static void SendToTunnelWithoutData(NetworkStream networkStream, string destId, string id, out string response)
        {
            SendToTunnel(networkStream, 
                @"{
                    ""id"" : ""tunnel/send"",
	                ""data"" :
	                    {
                            ""dest"" : """ + destId + @""",
		                    ""data"" : 
		                        {
                                    ""id"" : """ + id + @"""
                                 }
                        }
        }", out response);
        }

        public static void SendToTcp(NetworkStream networkStream, string data)
        {
            byte[] dataBytes = System.Text.Encoding.ASCII.GetBytes(data);
            int dataLength = dataBytes.Length;

            networkStream.Write(BitConverter.GetBytes(dataLength));
            networkStream.Write(dataBytes);
            networkStream.Flush();
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
