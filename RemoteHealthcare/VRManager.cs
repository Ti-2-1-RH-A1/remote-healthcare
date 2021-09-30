using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using Newtonsoft.Json.Schema;
using System.IO;
using System.Drawing;
using RemoteHealthcare;

namespace VirtualReality
{
    public class VrManager
    {
        //private NetworkStream networkStream;
        private Dictionary<string, string> userSessions;
        private Connection connection;
        private Dictionary<string, string> nodes;

        public VrManager()
        {
            // Initialise and connect to the TcpClient
            // On server: 145.48.6.10 and port: 6666
            TcpClient client = new TcpClient();
            client.Connect("145.48.6.10", 6666);

            // Request the session list from the server

            connection = new Connection(client.GetStream(), this);

            userSessions = VRMethod.GetRunningSessions(ref connection);
        }

        /// <summary>
        /// Reconnect does <c>reconnects to a new client</c> reconnects to a new client and resets all necessary fields
        /// </summary>
        public void Reconnect()
        {
            userSessions = VRMethod.GetRunningSessions(ref connection);
            ConnectToAClient();
            UpdateSceneList();
        }

        private void UpdateSceneList()
        {
            nodes = VRMethod.GetScene(ref connection);
        }


        /// <summary>Start does <c>The beginning of the VRManager</c> This is the beginning of the program, als 
        /// sometimes called the start of a programs life</summary>
        ///
        public void Start()
        {
            ConnectToAClient();

            VRMethod.ResetScene(ref connection);

            nodes = VRMethod.GetScene(ref connection);

            string terrainUuid = VRMethod.CreateTerrain(ref connection);

            VRMethod.SetTexture(ref connection, terrainUuid);

            JArray position = new JArray { 20, 0, 20 };
            JArray rotation = new JArray { 0, 0, 0 };
            string bikename1 = "Bike";
            string bikeUUID = VRMethod.AddModelBike(ref connection, bikename1, position, rotation);

            VRMethod.CreateBikePanel(ref connection);
            VRMethod.DrawOnBikePanel(ref connection, "This is our panel");

            UpdateSceneList();

            Random rnd = new Random();
            for (int i = 0; i < 200; i++)
            {
                JArray positionTree = new JArray { rnd.Next(75, 130), 1, rnd.Next(90, 140) };
                JArray rotationTree = new JArray { 0, rnd.Next(1, 360), 0 };
                if (i < 30)
                {
                    VRMethod.AddStaticModel(ref connection, "Tree" + i, positionTree, rotationTree, 1.9, @"data/NetworkEngine/models/trees/fantasy/tree6.obj");
                }
                else if (i < 60)
                {
                    VRMethod.AddStaticModel(ref connection, "Tree" + i, positionTree, rotationTree, 1.8, @"data/NetworkEngine/models/trees/fantasy/tree5.obj");
                }
                else if (i < 90)
                {
                    VRMethod.AddStaticModel(ref connection, "Tree" + i, positionTree, rotationTree, 1.75, @"data/NetworkEngine/models/trees/fantasy/tree4.obj");
                }
                else
                {
                    VRMethod.AddStaticModel(ref connection, "Tree" + i, positionTree, rotationTree, 1.8, @"data/NetworkEngine/models/trees/fantasy/tree3.obj");
                }

            }

            /// routeNodes tupple: Item 1 = positions, Item 2 = Directions(dir). Every tupple is 1 point in the route.
            List<(JArray, JArray)> routeNodes = new List<(JArray, JArray)>();

            (JArray, JArray) routeNode1;
            routeNode1.Item1 = new JArray { 70, 0, 80 };
            routeNode1.Item2 = new JArray { 5, 0, -5 };
            routeNodes.Add(routeNode1);

            (JArray, JArray) routeNode2;
            routeNode2.Item1 = new JArray { 90, 0, 84 };
            routeNode2.Item2 = new JArray { 5, 0, -5 };
            routeNodes.Add(routeNode2);

            (JArray, JArray) routeNode3;
            routeNode3.Item1 = new JArray { 110, 0, 75 };
            routeNode3.Item2 = new JArray { 5, 0, 5 };
            routeNodes.Add(routeNode3);

            (JArray, JArray) routeNode4;
            routeNode4.Item1 = new JArray { 133, 0, 85 };
            routeNode4.Item2 = new JArray { 5, 0, 5 };
            routeNodes.Add(routeNode4);

            (JArray, JArray) routeNode5;
            routeNode5.Item1 = new JArray { 132, 0, 110 };
            routeNode5.Item2 = new JArray { -5, 0, 5 };
            routeNodes.Add(routeNode5);

            (JArray, JArray) routeNode6;
            routeNode6.Item1 = new JArray { 138, 0, 145 };
            routeNode6.Item2 = new JArray { -5, 0, 5 };
            routeNodes.Add(routeNode6);

            (JArray, JArray) routeNode7;
            routeNode7.Item1 = new JArray { 60, 0, 140 };
            routeNode7.Item2 = new JArray { -5, 0, -5 };
            routeNodes.Add(routeNode7);

            (JArray, JArray) routeNode8;
            routeNode8.Item1 = new JArray { 70, 0, 105 };
            routeNode8.Item2 = new JArray { -5, 0, -5 };
            routeNodes.Add(routeNode8);

            string routeUUID = VRMethod.GenerateRoute(ref connection, routeNodes);

            VRMethod.AddRoad(ref connection, routeUUID);

            VRMethod.SetCamera(ref connection, bikeUUID);

            VRMethod.FollowRoute(ref connection, routeUUID, bikeUUID);
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

            connection.ReceiveFromTcp(out var tunnelCreationResponse, true);

            dynamic responseDeserializeObject = JsonConvert.DeserializeObject(tunnelCreationResponse);
            //string response = responseDeserializeObject["data"]["status"].ToString();

            if (VRMethod.IsStatusOk(ref connection, tunnelCreationResponse))
            {
                connection.currentSessionID = responseDeserializeObject["data"]["id"].ToString();

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}