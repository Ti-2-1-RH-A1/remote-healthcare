using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using Newtonsoft.Json.Schema;

namespace VirtualReality
{
    class Program
    {
        private NetworkStream networkStream;
        private List<(string, string)> userSessions;
        private String currentSessionID;
        private Dictionary<string, string> nodes;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start();
            
        }


        public Program()
        {
            // Initialise and connect to the TcpClient
            // On server: 145.48.6.10 and port: 6666
            TcpClient client = new TcpClient();
            client.Connect("145.48.6.10", 6666);
            // Request the session list from the server
            networkStream = client.GetStream();
            userSessions = GetRunningSessions();
        }

        /// <summary>Start does <c>The beginning of the Program</c> This is the beginning of the program, als 
        /// sometimes called the start of a programs life</summary>
        ///
        public void Start()
        {
            ConnectToAClient();

            ResetScene();

            nodes = GetScene();

            DeleteNodeViaUserInput();
            SetSkyBox();
        }


        /// <summary>
        /// connect to a client
        /// </summary>
        private void ConnectToAClient()
        {
            bool isConnected = false;

            while (!isConnected)
            {
                int i = 0;
                foreach ((string, string) s in userSessions)
                {
                    Console.WriteLine("#" + i + " " + s.Item2 + " " + s.Item1);
                    i++;
                }

                // get user input for which session to connect to
                Console.WriteLine("Which client should be connected to?");
                string userInput = Console.ReadLine();

                if (CreateTunnel(userInput))
                {
                    Console.WriteLine("Succes connected to " + userInput);
                    isConnected = true;
                }
                else
                {
                    Console.WriteLine("couldn't connect to that client");
                }
            }
        }


        /// <summary>CreateTunnel does <c>Creating a network tunnel</c> returns <returns>A Boolean</returns> sends the correct json and then checks connection status based on that it returns a boolean</summary>
        ///
        public bool CreateTunnel(string sessionId)
        {
            Console.WriteLine("Creating a tunnel");
            // create a tunnel
            JObject tunnelCreateJson = new JObject {{"id", "tunnel/create"}};

            JObject dataJson = new JObject {{"session", userSessions[int.Parse(sessionId)].Item1}};
            // place to set the key 
            string sessionKey = "";
            dataJson.Add("key", sessionKey);

            tunnelCreateJson.Add("data", dataJson);
            SendToTcp(tunnelCreateJson.ToString());

            ReceiveFromTcp(out var tunnelCreationResponse);

            dynamic responseDeserializeObject = JsonConvert.DeserializeObject(tunnelCreationResponse);
            //string response = responseDeserializeObject["data"]["status"].ToString();

            if (isStatusOk(tunnelCreationResponse))
            {
                currentSessionID = responseDeserializeObject["data"]["id"].ToString();

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool isStatusOk(string jsonResponse)
        {
            return jsonResponse.Contains("\"ok\"");
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

        /// <summary>GetRunningSessions does <c>Getting a all running sessions from the server</c> returns <returns>A Dictionary<string, string> containing all users as key and a value of all data</returns> sends data using SendDataToTCP and then Receive it using ReceiveFromTcp</summary>
        ///
        private List<(string, string)> GetRunningSessions()
        {
            JObject sessionJson = new JObject();
            sessionJson.Add("id", "session/list");
            SendToTcp(sessionJson.ToString());

            // receive the response
            string receivedData;
            ReceiveFromTcp(out receivedData);

            // parse the received data
            dynamic jsonData = JsonConvert.DeserializeObject(receivedData);
            JArray jsonDataArray = jsonData.data;

            // add session ids to the sessions list if they have an id, clientinfo and have a tunnel feature
            List<(string, string)> sessions = new List<(string, string)>();
            foreach (var jToken in jsonDataArray)
            {
                var jObject = (JObject) jToken;
                if (jObject.ContainsKey("id") && jObject.ContainsKey("clientinfo"))
                {
                    JArray features = (JArray) jObject.GetValue("features");
                    if (features != null && features.Count != 0 && features[0].ToString() == "tunnel")
                    {
                        string user = ((JObject) (jObject.GetValue("clientinfo")))?.GetValue("user")?.ToString();
                        sessions.Add((jObject.GetValue("id")?.ToString(), user));
                    }
                }
            }

            return sessions;
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

        public void DeleteNodeViaUserInput()
        {
            // Get user input for which node to delete
            string userInput = "";
            while (!nodes.ContainsKey(userInput))
            {
                Console.WriteLine("Select a node to remove from the following list:");
                foreach (string s in nodes.Keys)
                {
                    Console.WriteLine(s);
                }

                userInput = Console.ReadLine();
            }

            Console.WriteLine("Selected: " + userInput);
            Console.WriteLine(DeleteNode(userInput));
            
        }

        public bool DeleteNode(string nodeName)
        {
            // Send the message to the tunnel on what Node to delete
            //string response;
            JObject message = new JObject {{"id", JsonID.SCENE_NODE_DELETE}};
            JObject jsonData = new JObject {{"id", nodes.GetValueOrDefault(nodeName)}};
            message.Add("data", jsonData);
            string response = SendViaTunnel(message);
            Console.WriteLine("Delete node response: " + response);

            return isStatusOk(response);
        }

        public void ResetScene()
        {
            //string response;
            JObject message = new JObject {{"id", "scene/reset"}};
            SendViaTunnel(message);
        }

        public Dictionary<string, string> GetScene()
        {
            var dictionary = new Dictionary<string, string>();


            string response;
            JObject message = new JObject {{"id", JsonID.SCENE_GET}};
            response = SendViaTunnel(message);

            //ReceiveFromTcp(out response);
            Console.WriteLine(response);
            dynamic responseData = JsonConvert.DeserializeObject(response);
            if (responseData != null)
            {
                dictionary.Add(responseData.data.data.data.name.ToString(),
                    responseData.data.data.data.uuid.ToString());
                JArray children = responseData.data.data.data.children;
                foreach (var jToken in children)
                {
                    var jObject = (JObject) jToken;
                    dictionary.Add(jObject.GetValue("name")?.ToString() ?? string.Empty,
                        jObject.GetValue("uuid")?.ToString());
                }
            }

            return dictionary;


            // no gui stuff in methods please
            // foreach (string s in nodes.Keys)
            // {
            //     Console.WriteLine(s);
            // }
        }

        /// <summary>ReceiveFromTcp does <c>recieving data from a tcp stream</c> using a network stream decodes using ASCII to a string</summary>
        ///
        public void ReceiveFromTcp(out string receivedData)
        {
            // read a small part of the packet and receive the packet length
            byte[] buffer = new byte[4];
            int rc = networkStream.Read(buffer, 0, 4);
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

        public void SetSkyBox()
        {
            TimeChange timeChange = new TimeChange(this);
            Console.WriteLine("static [of] dynamic");
            switch (Console.ReadLine())
            {
                case "static":
                    timeChange.sendData();
                    break;
                case "dynamic":
                    Console.Write("Enter time between 0 - 24 : ");
                    float entryAmount = 0;
                    bool validEntery = false;
                    while (!validEntery)
                    {
                        try
                        {
                            entryAmount = float.Parse(Console.ReadLine());
                            if (entryAmount < 0 || entryAmount > 24)
                            {
                                throw new Exception();
                            }

                            validEntery = true;
                        }
                        catch (Exception)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine("Getal invoer is niet correct probeer opnieuw");
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                    }

                    timeChange.sendData(entryAmount);
                    break;
            }
        }
    }
}