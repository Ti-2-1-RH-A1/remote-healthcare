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
    public class VrManager
    {
        //private NetworkStream networkStream;
        private Dictionary<string, string> userSessions;
        private Connection connection;

        private Dictionary<string, string> nodes;

        static void Main(string[] args)
        {
            VrManager vrManager = new VrManager();
            vrManager.Start();
        }


        public VrManager()
        {
            // Initialise and connect to the TcpClient
            // On server: 145.48.6.10 and port: 6666
            TcpClient client = new TcpClient();
            client.Connect("145.48.6.10", 6666);

            // Request the session list from the server

            connection = new Connection(client.GetStream(), this);

            userSessions = GetRunningSessions();
        }

        /// <summary>
        /// Reconnect does <c>reconnects to a new client</c> reconnects to a new client and resets all necessary fields
        /// </summary>
        public void Reconnect()
        {
            userSessions = GetRunningSessions();
            ConnectToAClient();
            nodes = GetScene();
        }


        /// <summary>Start does <c>The beginning of the VRManager</c> This is the beginning of the program, als 
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
                List<string> keyList = new List<string>();

                foreach (var (key, value) in userSessions)
                {
                    Console.WriteLine("#" + i + " " + key + " " + value);
                    keyList.Add(key);
                    i++;
                }


                // get user input for which session to connect to
                Console.WriteLine("Which client should be connected to?");
                int userInput = int.Parse(Console.ReadLine());

                if (CreateTunnel(keyList[userInput]))
                {
                    Console.WriteLine("Succes connected to " + keyList[userInput]);
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


            JObject dataJson = new JObject {{"session", userSessions[sessionId]}};
            // place to set the key 
            string sessionKey = "";
            dataJson.Add("key", sessionKey);

            tunnelCreateJson.Add("data", dataJson);
            connection.SendToTcp(tunnelCreateJson.ToString());

            connection.ReceiveFromTcp(out var tunnelCreationResponse);

            dynamic responseDeserializeObject = JsonConvert.DeserializeObject(tunnelCreationResponse);
            //string response = responseDeserializeObject["data"]["status"].ToString();

            if (isStatusOk(tunnelCreationResponse))
            {
                connection.currentSessionID = responseDeserializeObject["data"]["id"].ToString();

                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// isStatusOk does <c>checks if a string contains an ok message</c>
        /// </summary>
        /// <param name="jsonResponse"></param>
        /// <returns>a bool stating if a oke message was found</returns>
        private bool isStatusOk(string jsonResponse)
        {
            return jsonResponse.Contains("\"ok\"");
        }


        /// <summary>GetRunningSessions does <c>Getting a all running sessions from the server</c> returns <returns>A Dictionary<string, string> containing all users as key and a value of all data</returns> sends data using SendDataToTCP and then Receive it using ReceiveFromTcp</summary>
        ///
        private Dictionary<string, string> GetRunningSessions()
        {
            JObject sessionJson = new JObject();
            sessionJson.Add("id", "session/list");
            connection.SendToTcp(sessionJson.ToString());

            // receive the response
            string receivedData;
            connection.ReceiveFromTcp(out receivedData);

            // parse the received data
            dynamic jsonData = JsonConvert.DeserializeObject(receivedData);
            JArray jsonDataArray = jsonData.data;

            // add session ids to the sessions list if they have an id, clientinfo and have a tunnel feature
            Dictionary<string, string> sessions = new Dictionary<string, string>();
            foreach (var jToken in jsonDataArray)
            {
                var jObject = (JObject) jToken;
                if (jObject.ContainsKey("id") && jObject.ContainsKey("clientinfo"))
                {
                    JArray features = (JArray) jObject.GetValue("features");
                    if (features != null && features.Count != 0 && features[0].ToString() == "tunnel")
                    {
                        string user = ((JObject) (jObject.GetValue("clientinfo")))?.GetValue("user")?.ToString();
                        string id = jObject.GetValue("id")?.ToString();

                        if (sessions.ContainsKey(user))
                        {
                            sessions[user] = id;
                        }
                        else
                        {
                            sessions.Add(user, id);
                        }
                    }
                }
            }

            return sessions;
        }

        /// <summary>
        /// shows user al available nodes then ask wich to delete 
        /// </summary>
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
            DeleteNode(userInput);
        }

        /// <summary>
        /// deletes the node specified in the parameter
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public bool DeleteNode(string nodeName)
        {
            // Send the message to the tunnel on what Node to delete
            //string response;
            JObject message = new JObject {{"id", JsonID.SCENE_NODE_DELETE}};
            JObject jsonData = new JObject {{"id", nodes.GetValueOrDefault(nodeName)}};
            message.Add("data", jsonData);
            string response = connection.SendViaTunnel(message);
            Console.WriteLine("Delete node response: " + response);

            return isStatusOk(response);
        }

        /// <summary>
        /// resets the scene
        /// </summary>
        public void ResetScene()
        {
            //string response;
            JObject message = new JObject {{"id", "scene/reset"}};
            connection.SendViaTunnel(message);
        }

        /// <summary>GetScene does <c>recieves a scene from a a connected client</c> using a network stream decodes using ASCII to a string</summary>
        ///
        public Dictionary<string, string> GetScene()
        {
            var dictionary = new Dictionary<string, string>();


            string response;
            JObject message = new JObject {{"id", JsonID.SCENE_GET}};
            response = connection.SendViaTunnel(message);

            //ReceiveFromTcp(out response);
            //Console.WriteLine(response);
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

        /// <summary>
        /// sets the skybox
        /// </summary>
        public void SetSkyBox()
        {
            TimeChange timeChange = new TimeChange(connection);
            Console.WriteLine("static [of] dynamic");
            switch (Console.ReadLine())
            {
                case "static":
                    timeChange.sendData(true);
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