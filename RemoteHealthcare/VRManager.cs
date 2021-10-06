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

            /*Ground_Add groundAdd = new Ground_Add(connection);

            groundAdd.SetTerrain();*/

            JArray position = new JArray { 1, 0, 1 };
            JArray rotation = new JArray { 0, 0, 0 };
            string bikename1 = "Bike";
            string bikeUUID = VRMethod.AddModelBike(ref connection, bikename1, position, rotation);

            VRMethod.CreateBikePanel(ref connection);
            VRMethod.CreateMessagePanel(ref connection);
            VRMethod.DrawOnBikePanel(ref connection, "hoegaboega");
            VRMethod.DrawChatMessage(ref connection, "PLACEHOLDER[Ontvangen messages van doktor applicatie]", "messagePanel");

            UpdateSceneList();


            Random rnd = new Random();
            for (int i = 0; i < 20; i++) /// Note: Dont try to add 200 trees. Thank you.
            {
                JArray positionTree = new JArray {rnd.Next(-30, 30), 0, rnd.Next(-30, 30)};
                JArray rotationTree = new JArray {0, rnd.Next(1, 360), 0};
                VRMethod.AddStaticModel(ref connection, "Tree" + i, positionTree, rotationTree, 1.25,
                    @"data/NetworkEngine/models/trees/fantasy/tree6.obj");
            }

            /// routeNodes tupple: Item 1 = positions, Item 2 = Directions(dir). Every tupple is 1 point in the route.
            List<(JArray, JArray)> routeNodes = new List<(JArray, JArray)>();

            (JArray, JArray) routeNode1;
            routeNode1.Item1 = new JArray { 0, 0, 0 };
            routeNode1.Item2 = new JArray { 5, 0, -5 };
            routeNodes.Add(routeNode1);

            (JArray, JArray) routeNode2;
            routeNode2.Item1 = new JArray { 50, 0, 0 };
            routeNode2.Item2 = new JArray { 5, 0, 5 };
            routeNodes.Add(routeNode2);

            (JArray, JArray) routeNode3;
            routeNode3.Item1 = new JArray { 50, 0, 50 };
            routeNode3.Item2 = new JArray { -5, 0, 5 };
            routeNodes.Add(routeNode3);

            (JArray, JArray) routeNode4;
            routeNode4.Item1 = new JArray { 0, 0, 50 };
            routeNode4.Item2 = new JArray { -5, 0, -5 };
            routeNodes.Add(routeNode4);

            string routeUUID = VRMethod.GenerateRoute(ref connection, routeNodes);

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