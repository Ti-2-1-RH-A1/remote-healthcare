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
            updateSceneList();
        }

        private void updateSceneList()
        {
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

            string terrainUuid = CreateTerrain();

            SetTexture(terrainUuid);

            JArray position = new JArray { 20, 0, 20 };
            JArray rotation = new JArray { 0, 0, 0 };
            string bikename1 = "Bike";
            string bikeUUID = AddModelBike(bikename1, position, rotation);

            CreateBikePanel();
            drawOnBikePanel("hoegaboega");

            updateSceneList();


            Random rnd = new Random();
            for (int i = 0; i < 200; i++)
            {
                JArray positionTree = new JArray { rnd.Next(75, 130), 1, rnd.Next(90, 140) };
                JArray rotationTree = new JArray { 0, rnd.Next(1, 360), 0 };
                if(i < 30)
                {
                    AddStaticModel("Tree" + i, positionTree, rotationTree, 1.9, @"data/NetworkEngine/models/trees/fantasy/tree6.obj");
                } else if(i < 60)
                {
                    AddStaticModel("Tree" + i, positionTree, rotationTree, 1.8, @"data/NetworkEngine/models/trees/fantasy/tree5.obj");
                } else if(i < 90)
                {
                    AddStaticModel("Tree" + i, positionTree, rotationTree, 1.75, @"data/NetworkEngine/models/trees/fantasy/tree4.obj");
                } else
                {
                    AddStaticModel("Tree" + i, positionTree, rotationTree, 1.8, @"data/NetworkEngine/models/trees/fantasy/tree3.obj");
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

            string routeUUID = GenerateRoute(routeNodes);

            AddRoad(routeUUID);

            FollowRoute(routeUUID, bikeUUID);

            DeleteNodeViaUserInput();
            SetSkyBox();
        }

        /// <summary>
        /// create and draw a panel on the bike
        /// </summary>
        /// <param name="text">the text to draw</param>
        /// <param name="panelName">optional</param>
        private void drawOnBikePanel(string text, string panelName = "bikePanel")
        {
            int[] position = {100, 100};
            int[] color = {0, 0, 0, 1};

            ClearPanel(GetIdFromNodeName(panelName));
            drawtext(panelName, text, position, 32, color, "segoeui");
            SwapPanel(GetIdFromNodeName(panelName));
        }

        private void drawtext(string panelNodeName, string text, int[] position, int size, int[] color, string font)
        {
            JObject message = new JObject();
            message.Add("id", JsonID.SCENE_PANEL_DRAWTEXT);

            JObject dataJObject = new JObject();
            dataJObject.Add("id", GetIdFromNodeName(panelNodeName));
            dataJObject.Add("text", text);
            dataJObject.Add("position", JArray.FromObject(position));
            dataJObject.Add("size", size);
            dataJObject.Add("color", JArray.FromObject(color));
            dataJObject.Add("font", font);

            message.Add("data", dataJObject);

            string response = "";
            connection.SendViaTunnel(message, (callbackResponse => response = callbackResponse));
            while (response.Length == 0)
            {
                Thread.Sleep(10);
            }

            Console.WriteLine(response);
        }


        /// <summary>
        /// creates a bike panel using some default values
        /// </summary>
        private void CreateBikePanel(string panelName = "bikePanel")
        {
            int[] position = {-50, 115, 0};
            int[] rotation = {315, 90, 0};
            int[] size = {50, 25};
            //int[] resolution = {256, 128};
            int[] resolution = {512, 512};
            int[] background = {1, 1, 1, 1};

            CreatePanel(panelName, position, rotation, size, resolution, background, true, getBikeID());
        }


        private void ClearPanel(string nodeID)
        {
            JObject message = new JObject();
            message.Add("id", JsonID.SCENE_PANEL_CLEAR);


            JObject dataJObject = new JObject();
            dataJObject.Add("id", nodeID);

            message.Add("data", dataJObject);

            connection.SendViaTunnel(message);
        }

        private void SwapPanel(string nodeID)
        {
            JObject message = new JObject();
            message.Add("id", JsonID.SCENE_PANEL_SWAP);


            JObject dataJObject = new JObject();
            dataJObject.Add("id", nodeID);

            message.Add("data", dataJObject);

            connection.SendViaTunnel(message);
        }


        public string getBikeID()
        {
            return GetIdFromNodeName("Bike");
        }

        /// <summary>
        /// Createsa panel
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="size"></param>
        /// <param name="resolution"></param>
        /// <param name="background"></param>
        /// <param name="castShadows"></param>
        /// <param name="parent"></param>
        public void CreatePanel(string name, int[] position, int[] rotation, int[] size, int[] resolution,
            int[] background, bool castShadows = true, string parent = null)
        {
            JObject components = new JObject();

            JObject transform = new JObject();
            transform.Add("position", JArray.FromObject(position));
            transform.Add("scale", 1);
            transform.Add("rotation", JArray.FromObject(rotation));

            components.Add("transform", transform);

            JObject panel = new JObject();
            panel.Add("size", JArray.FromObject(size));
            panel.Add("resolution", JArray.FromObject(resolution));
            panel.Add("background", JArray.FromObject(background));
            panel.Add("castShadow", castShadows);

            components.Add("panel", panel);


            JObject node = CreateNode(name, components, parent);

            Console.WriteLine("\n\n" + node.ToString() + "\n\n");

            string response = "";
            connection.SendViaTunnel(node, (callbackResponse => response = callbackResponse));
            while (response.Length == 0)
            {
                Thread.Sleep(10);
            }

            Console.WriteLine(response);
        }


        /// <summary>
        /// Creates a node with the components
        /// </summary>
        /// <param name="name">the name of the node</param>
        /// <param name="components">the components of the node</param>
        /// <param name="parent">the parent of the node</param>
        /// <returns>a new node jObject</returns>
        private JObject CreateNode(string name, JToken components, string parent = null)
        {
            JObject message = new JObject {{"id", JsonID.SCENE_NODE_ADD}};

            JObject dataJObject = new JObject {{"name", name}};
            if (parent != null)
            {
                dataJObject.Add("parent", parent);
            }

            dataJObject.Add("components", components);


            message.Add("data", dataJObject);

            return message;
        }


        /// <summary>
        /// GetIdFromNodeName does <c></c>
        /// </summary>
        /// <returns>a string with the bikes current id</returns>
        public string GetIdFromNodeName(string nodeName)
        {
            JObject message = new JObject {{"id", JsonID.SCENE_NODE_FIND}};
            JObject data = new JObject {{"name", nodeName}};
            message.Add("data", data);

            string response = "";
            connection.SendViaTunnel(message, (callbackResponse => response = callbackResponse));
            while (response.Length == 0)
            {
                Thread.Sleep(10);
            }

            Console.WriteLine("this one: \n" + response);

            JObject responseJObject = JObject.Parse(response);
            JObject responseData = (JObject) (responseJObject.GetValue("data")?[0]);
            if (responseData != null)
            {
                string s = responseData.GetValue("uuid")?.ToString();
                Console.WriteLine(s);
                return s;
            }

            return string.Empty;
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
            connection.ReceiveFromTcp(out receivedData, true);

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


            string response = "";
            connection.SendViaTunnel(message, callbackResponse => response = callbackResponse);


            Console.WriteLine("Delete node response: " + response);

            return isStatusOk(response);
        }

        /// <summary>
        /// Method to create terrain based on a heightmap
        /// </summary>
        /// <param name="connection"> connection to send data to and receive responses from</param>
        public string CreateTerrain()
        {

            Console.WriteLine("Enter a path to an heightmap");

            string entryPath = @"" + Console.ReadLine();
            if (!File.Exists(entryPath))
            {
                Console.WriteLine("No file found");
                return null;
            }

            // convert the heightmap to a bitmap and then set that into a heightmap array
            Bitmap bitmap = new Bitmap(entryPath);
            int width = 256;
            int height = 256;
            float offset = 0.03f;
            float[] widthHeight = { width, height };

            float[] heightMap = new float[width * height];

            int index = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    heightMap[index++] = bitmap.GetPixel(i, j).R * offset;
                }
            }

            // First delete old terrain
            JObject tunnelDelterrainJson = new JObject { { "id", "scene/terrain/delete" } };

            JObject dataJson = new JObject();
            tunnelDelterrainJson.Add("data", dataJson);

            string tunnelCreationResponse = "";
            connection.SendViaTunnel(tunnelDelterrainJson, (callbackResponse => tunnelCreationResponse = callbackResponse));
            while (tunnelCreationResponse.Length == 0)
            {
                Thread.Sleep(10);
            }

            Console.WriteLine(tunnelCreationResponse);

            dynamic responseDeserializeObject = JsonConvert.DeserializeObject(tunnelCreationResponse);
            string response = responseDeserializeObject.ToString();

            // Start creating the JObject to create the new terrain
            JObject tunnelAddterrainJson = new JObject { { "id", "scene/terrain/add" } };

            // create a JArray of the dimensions of the heightmap
            JArray jarrayWH = new JArray();
            jarrayWH.Add(width);
            jarrayWH.Add(height);

            // Create a JArray For the heightmap
            JArray heightMapJArray = new JArray();
            foreach (float item in heightMap)
            {
                heightMapJArray.Add(item);
            }

            // Gather the data into a JObject, add it to the wrapper object to create the terrain and send it via the connection
            JObject dataAddJson = new JObject();
            dataAddJson.Add("size", jarrayWH);
            dataAddJson.Add("heights", heightMapJArray);

            tunnelAddterrainJson.Add("data", dataAddJson);
            connection.SendViaTunnel(tunnelAddterrainJson);
            // TODO receive the response to not clog the buffer

            // Then add the node linked to the terrain
            JObject tunnelAddTerrainNode = new JObject { { "id", "scene/node/add" } };
            JObject dataAddNodeJson = new JObject();
            dataAddNodeJson.Add("name", "terrain");

            JObject jsonComponents = new JObject();
            JObject jsonTransform = new JObject();
            JArray transPostion = new JArray();
            transPostion.Add(0);
            transPostion.Add(0);
            transPostion.Add(0);
            jsonTransform.Add("position", transPostion);
            jsonTransform.Add("scale", 1);
            jsonTransform.Add("rotation", transPostion);
            jsonComponents.Add("transform", jsonTransform);
            JObject jsonTerrain = new JObject();
            jsonTerrain.Add("smoothnormals", true);
            jsonComponents.Add("terrain", jsonTerrain);

            dataAddNodeJson.Add("components", jsonComponents);

            tunnelAddTerrainNode.Add("data", dataAddNodeJson);

            ///connection.SendViaTunnel(tunnelAddTerrainNode);

            string responseTerrain = "";
            connection.SendViaTunnel(tunnelAddTerrainNode, (callbackResponse => responseTerrain = callbackResponse));
            while (responseTerrain.Length == 0)
            {
                Thread.Sleep(10);
            }

            dynamic terrainRespond = JsonConvert.DeserializeObject(responseTerrain);

            Console.WriteLine(tunnelAddTerrainNode);

            return terrainRespond.data.uuid;
        }

        /// <summary>
        /// Set the texture of the specified node id to a set hardcoded texture
        /// </summary>
        /// <param name="nodeUuid"></param>
        public void SetTexture(string nodeUuid)
        {
            // Construct the JObject to be able to set the texture on the node
            JObject tunnelSetTerrain = new JObject { { "id", "scene/node/addlayer" } };
            JObject dataAddNodeJson = new JObject();
            dataAddNodeJson.Add("id", nodeUuid);
            string file = @"data\NetworkEngine\textures\terrain\grass_green_d.jpg";
            dataAddNodeJson.Add("diffuse", file);
            dataAddNodeJson.Add("normal", file);
            dataAddNodeJson.Add("minHeight", -100);
            dataAddNodeJson.Add("maxHeight", 100);
            dataAddNodeJson.Add("fadeDist", 1);

            tunnelSetTerrain.Add("data", dataAddNodeJson);

            // Send the message via the connection
            ///connection.SendViaTunnel(tunnelSetTerrain);

            string responseTerrainTexture = "";
            connection.SendViaTunnel(tunnelSetTerrain, (callbackResponse => responseTerrainTexture = callbackResponse));
            while (responseTerrainTexture.Length == 0)
            {
                Thread.Sleep(10);
            }

            dynamic terrainRespond = JsonConvert.DeserializeObject(responseTerrainTexture);

            Console.WriteLine(tunnelSetTerrain);
        }

        /// <summary>GetScene does <c>recieves a scene from a a connected client</c> using a network stream decodes using ASCII to a string</summary>
        ///
        public Dictionary<string, string> GetScene()
        {
            var dictionary = new Dictionary<string, string>();


            JObject message = new JObject { { "id", JsonID.SCENE_GET } };
            string response = "";
            connection.SendViaTunnel(message, (callbackResponse => response = callbackResponse));
            while (response.Length == 0)
            {
                Thread.Sleep(10);
            }


            //ReceiveFromTcp(out response);
            //Console.WriteLine(response);
            dynamic responseData = JsonConvert.DeserializeObject(response);
            if (responseData != null)
            {
                dictionary.Add(responseData.data.name.ToString(),
                    responseData.data.uuid.ToString());
                JArray children = responseData.data.children;
                foreach (var jToken in children)
                {
                    var jObject = (JObject)jToken;
                    dictionary.Add(jObject.GetValue("name")?.ToString() ?? string.Empty,
                        jObject.GetValue("uuid")?.ToString());
                }
            }

            return dictionary;
        }

        /// <summary>
        /// resets the scene
        /// </summary>
        public void ResetScene()
        {
            //string response;
            JObject message = new JObject { { "id", "scene/reset" } };
            connection.SendViaTunnel(message);
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

        /// <summary>
        /// Add a bike model to the specified position with the specified rotation
        /// </summary>
        /// <param name="bikeName"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public string AddModelBike(string bikeName, JArray position, JArray rotation)
        {
            JObject jsonModelBike = new JObject();
            jsonModelBike.Add("id", "scene/node/add");

            JObject jsonModelBikeData = new JObject();
            jsonModelBikeData.Add("name", bikeName);

            JObject jsonModelBikeComponents = new JObject();

            JObject jsonModelBikeComponentTransform = new JObject();
            jsonModelBikeComponentTransform.Add("position", position);
            jsonModelBikeComponentTransform.Add("scale", 0.01);
            jsonModelBikeComponentTransform.Add("rotation", rotation);

            JObject jsonModelBikeComponentModel = new JObject();
            jsonModelBikeComponentModel.Add("file", @"data/NetworkEngine/models/bike/bike_anim.fbx");
            jsonModelBikeComponentModel.Add("cullbackfaces", true);
            jsonModelBikeComponentModel.Add("animated", true);
            jsonModelBikeComponentModel.Add("animation", "Armature|Fietsen");

            // Adding every header to their parent.
            jsonModelBikeComponents.Add("transform", jsonModelBikeComponentTransform);
            jsonModelBikeComponents.Add("model", jsonModelBikeComponentModel);
            jsonModelBikeData.Add("components", jsonModelBikeComponents);
            jsonModelBike.Add("data", jsonModelBikeData);

            string response = "";
            connection.SendViaTunnel(jsonModelBike, (callbackResponse => response = callbackResponse));
            while (response.Length == 0)
            {
                Thread.Sleep(10);
            }

            dynamic routeRespond = JsonConvert.DeserializeObject(response);

            Console.WriteLine(jsonModelBike);

            return routeRespond.data.uuid;
        }

        /// <summary>
        /// Add a specified model
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <param name="file"></param>
        public void AddStaticModel(string modelName, JArray position, JArray rotation, double scale, string file)
        {
            JObject jsonModel = new JObject();
            jsonModel.Add("id", "scene/node/add");

            JObject jsonModelData = new JObject();
            jsonModelData.Add("name", modelName);

            JObject jsonModelComponents = new JObject();

            JObject jsonModelComponentTransform = new JObject();
            jsonModelComponentTransform.Add("position", position);
            jsonModelComponentTransform.Add("scale", scale);
            jsonModelComponentTransform.Add("rotation", rotation);

            JObject jsonModelComponentModel = new JObject();
            jsonModelComponentModel.Add("file", file);
            jsonModelComponentModel.Add("cullbackfaces", true);
            jsonModelComponentModel.Add("animated", false);

            // Adding every header to their parent.
            jsonModelComponents.Add("transform", jsonModelComponentTransform);
            jsonModelComponents.Add("model", jsonModelComponentModel);
            jsonModelData.Add("components", jsonModelComponents);
            jsonModel.Add("data", jsonModelData);

            //Console.WriteLine(jsonModel);
            connection.SendViaTunnel(jsonModel);
        }

        /// <summary>
        /// Generate a route using the given routenodes
        /// </summary>
        /// <param name="routeNodes">List of JArray,JArray tuples, item1 gives the xyz position of a node and item2 the xyz direction, the route is generated through the different nodes in the list</param>
        /// <returns></returns>
        public string GenerateRoute(List<(JArray, JArray)> routeNodes)
        {
            JArray nodesArray = new JArray();

            foreach ((JArray, JArray) node in routeNodes)
            {
                JObject nodeObject = new JObject();
                nodeObject.Add("pos", node.Item1);
                nodeObject.Add("dir", node.Item2);
                nodesArray.Add(nodeObject);
            }

            JObject nodesObjectHeader = new JObject { { "nodes", nodesArray } };

            JObject dataObjectHeader = new JObject { { "id", JsonID.ROUTE_ADD } };
            dataObjectHeader.Add("data", nodesObjectHeader);

            Console.WriteLine(dataObjectHeader);

            string response = "";
            connection.SendViaTunnel(dataObjectHeader, (callbackResponse => response = callbackResponse));
            while (response.Length == 0)
            {
                Thread.Sleep(10);
            }

            dynamic routeRespond = JsonConvert.DeserializeObject(response);


            Console.WriteLine(response);

            return routeRespond.data.uuid;
        }

        public void AddRoad(string routeID)
        {
            JObject dataRoad = new JObject();

            dataRoad.Add("route", routeID);
            dataRoad.Add("diffuse", @"data/NetworkEngine/textures/terrain/mntn_black_d.jpg");
            dataRoad.Add("normal", @"data/NetworkEngine/textures/terrain/mntn_black_d.jpg");
            dataRoad.Add("specular", @"data/NetworkEngine/textures/terrain/mntn_black_d.jpg");
            dataRoad.Add("heightoffset", 12);

            JObject roadObject = new JObject { { "id", JsonID.SCENE_ROAD_ADD } };
            roadObject.Add("data", dataRoad);

            string response = "";
            connection.SendViaTunnel(roadObject, (callbackResponse => response = callbackResponse));
            while (response.Length == 0)
            {
                Thread.Sleep(10);
            }

            dynamic routeRespond = JsonConvert.DeserializeObject(response);
        }

        public void FollowRoute(string routeID, string nodeID)
        {
            JObject dataRoute = new JObject();

            dataRoute.Add("route", routeID);
            dataRoute.Add("node", nodeID);
            dataRoute.Add("speed", 1.0);
            dataRoute.Add("offset", 0.0);
            dataRoute.Add("rotate", "XZ");
            dataRoute.Add("smoothing", 1.0);
            dataRoute.Add("followHeight", true);
            dataRoute.Add("rotateOffset", new JArray { 0, 0, 0 });
            dataRoute.Add("positionOffset", new JArray { 0, 0, 0 });

            JObject routeObject = new JObject { { "id", JsonID.ROUTE_FOLLOW } };
            routeObject.Add("data", dataRoute);

            string response = "";
            connection.SendViaTunnel(routeObject, (callbackResponse => response = callbackResponse));
            while (response.Length == 0)
            {
                Thread.Sleep(10);
            }

            dynamic routeRespond = JsonConvert.DeserializeObject(response);

        }
    }
}