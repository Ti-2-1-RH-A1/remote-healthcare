﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using VirtualReality;

namespace RemoteHealthcare
{
    class VRMethod
    {
        /// <summary>
        /// isStatusOk does <c>checks if a string contains an ok message</c>
        /// </summary>
        /// <param name="jsonResponse"></param>
        /// <returns>a bool stating if a oke message was found</returns>
        public static bool IsStatusOk(ref Connection connection, string jsonResponse)
        {
            return jsonResponse.Contains("\"ok\"");
        }


        /// <summary>GetRunningSessions does <c>Getting a all running sessions from the server</c> returns <returns>A Dictionary<string, string> containing all users as key and a value of all data</returns> sends data using SendDataToTCP and then Receive it using ReceiveFromTcp</summary>
        ///
        public static Dictionary<string, string> GetRunningSessions(ref Connection connection)
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
                var jObject = (JObject)jToken;
                if (jObject.ContainsKey("id") && jObject.ContainsKey("clientinfo"))
                {
                    JArray features = (JArray)jObject.GetValue("features");
                    if (features != null && features.Count != 0 && features[0].ToString() == "tunnel")
                    {
                        string user = ((JObject)(jObject.GetValue("clientinfo")))?.GetValue("user")?.ToString();
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
        public static void DeleteNodeViaUserInput(ref Connection connection, Dictionary<string, string> nodes)
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
            DeleteNode(ref connection, userInput, nodes);
        }

        /// <summary>
        /// deletes the node specified in the parameter
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public static bool DeleteNode(ref Connection connection, string nodeName, Dictionary<string, string> nodes)
        {
            // Send the message to the tunnel on what Node to delete
            //string response;
            JObject message = new JObject { { "id", JsonID.SCENE_NODE_DELETE } };
            JObject jsonData = new JObject { { "id", nodes.GetValueOrDefault(nodeName) } };
            message.Add("data", jsonData);


            string response = "";
            connection.SendViaTunnel(message, callbackResponse => response = callbackResponse);


            Console.WriteLine("Delete node response: " + response);

            return IsStatusOk(ref connection, response);
        }

        /// <summary>
        /// Method to create terrain based on a heightmap
        /// </summary>
        /// <param name="connection"> connection to send data to and receive responses from</param>
        public static dynamic CreateTerrain(ref Connection connection)
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
            JObject tunnelDelterrainJson = new JObject { { "id", JsonID.SCENE_TERRAIN_DELETE } };

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
            JObject tunnelAddterrainJson = new JObject { { "id", JsonID.SCENE_TERRAIN_ADD } };

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
            JObject tunnelAddTerrainNode = new JObject { { "id", JsonID.SCENE_NODE_ADD } };
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
        public static void SetTexture(ref Connection connection, string nodeUuid)
        {
            // Construct the JObject to be able to set the texture on the node
            JObject tunnelSetTerrain = new JObject { { "id", JsonID.SCENE_NODE_ADDLAYER } };
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
        public static Dictionary<string, string> GetScene(ref Connection connection)
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
        public static void ResetScene(ref Connection connection)
        {
            //string response;
            JObject message = new JObject { { "id", JsonID.SCENE_RESET } };
            connection.SendViaTunnel(message);
        }

        /// <summary>
        /// sets the skybox
        /// </summary>
        public static void SetSkyBox(ref Connection connection)
        {
            Console.WriteLine("static [of] dynamic");
            switch (Console.ReadLine())
            {
                case "static":
                    SetSkyBoxStatic(ref connection);
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
                    SetSkyBoxTime(ref connection, entryAmount);
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
        public static string AddModelBike(ref Connection connection, string bikeName, JArray position, JArray rotation)
        {
            JObject jsonModelBike = new JObject();
            jsonModelBike.Add("id", JsonID.SCENE_NODE_ADD);

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
        public static void AddStaticModel(ref Connection connection, string modelName, JArray position, JArray rotation, double scale, string file)
        {
            JObject jsonModel = new JObject();
            jsonModel.Add("id", JsonID.SCENE_NODE_ADD);

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
        public static string GenerateRoute(ref Connection connection, List<(JArray, JArray)> routeNodes)
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

        /// <summary>
        /// Make a given node follow a given route
        /// </summary>
        /// <param name="routeID"></param>
        /// <param name="nodeID"></param>
        public static void FollowRoute(ref Connection connection, string routeID, string nodeID)
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

        /// <summary>
        /// create and draw a panel on the bike
        /// </summary>
        /// <param name="text">the text to draw</param>
        /// <param name="panelName">optional</param>
        public static void DrawOnBikePanel(ref Connection connection, string text, string panelName = "bikePanel")
        {
            int[] position = { 100, 100 };
            int[] color = { 0, 0, 0, 1 };

            ClearPanel(ref connection, GetIdFromNodeName(ref connection, panelName));
            Drawtext(ref connection, panelName, text, position, 32, color, "segoeui");
            SwapPanel(ref connection, GetIdFromNodeName(ref connection, panelName));
        }

        public static void Drawtext(ref Connection connection, string panelNodeName, string text, int[] position, int size, int[] color, string font)
        {
            JObject message = new JObject();
            message.Add("id", JsonID.SCENE_PANEL_DRAWTEXT);

            JObject dataJObject = new JObject();
            dataJObject.Add("id", GetIdFromNodeName(ref connection, panelNodeName));
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
        public static void CreateBikePanel(ref Connection connection, string panelName = "bikePanel")
        {
            int[] position = { -50, 115, 0 };
            int[] rotation = { 315, 90, 0 };
            int[] size = { 50, 25 };
            //int[] resolution = {256, 128};
            int[] resolution = { 512, 512 };
            int[] background = { 1, 1, 1, 1 };

            CreatePanel(ref connection, panelName, position, rotation, size, resolution, background, true, GetBikeID(ref connection));
        }


        public static void ClearPanel(ref Connection connection, string nodeID)
        {
            JObject message = new JObject();
            message.Add("id", JsonID.SCENE_PANEL_CLEAR);


            JObject dataJObject = new JObject();
            dataJObject.Add("id", nodeID);

            message.Add("data", dataJObject);

            connection.SendViaTunnel(message);
        }

        public static void SwapPanel(ref Connection connection, string nodeID)
        {
            JObject message = new JObject();
            message.Add("id", JsonID.SCENE_PANEL_SWAP);


            JObject dataJObject = new JObject();
            dataJObject.Add("id", nodeID);

            message.Add("data", dataJObject);

            connection.SendViaTunnel(message);
        }


        public static string GetBikeID(ref Connection connection)
        {
            return GetIdFromNodeName(ref connection, "Bike");
        }

        /// <summary>
        /// Creates a panel
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="size"></param>
        /// <param name="resolution"></param>
        /// <param name="background"></param>
        /// <param name="castShadows"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static void CreatePanel(ref Connection connection, string name, int[] position, int[] rotation, int[] size, int[] resolution,
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
        public static JObject CreateNode(string name, JToken components, string parent = null)
        {
            JObject message = new JObject { { "id", JsonID.SCENE_NODE_ADD } };

            JObject dataJObject = new JObject { { "name", name } };
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
        public static string GetIdFromNodeName(ref Connection connection, string nodeName)
        {
            JObject message = new JObject { { "id", JsonID.SCENE_NODE_FIND } };
            JObject data = new JObject { { "name", nodeName } };
            message.Add("data", data);

            string response = "";
            connection.SendViaTunnel(message, (callbackResponse => response = callbackResponse));
            while (response.Length == 0)
            {
                Thread.Sleep(10);
            }

            Console.WriteLine("this one: \n" + response);

            JObject responseJObject = JObject.Parse(response);
            JObject responseData = (JObject)(responseJObject.GetValue("data")?[0]);
            if (responseData != null)
            {
                string s = responseData.GetValue("uuid")?.ToString();
                Console.WriteLine(s);
                return s;
            }

            return string.Empty;
        }

        public static void SetSkyBoxStatic(ref Connection connection)
        {
            JObject sendJson = new JObject();
            sendJson.Add("id", JsonID.SCENE_SKYBOX_UPDATE);

            JObject jsonData = new JObject();
            jsonData.Add("type", "static");

            JObject jsonFiles = new JObject();
            jsonFiles.Add("xpos", @"data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_rt.png");
            jsonFiles.Add("xneg", @"data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_lf.png");
            jsonFiles.Add("ypos", @"data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_up.png");
            jsonFiles.Add("yneg", @"data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_dn.png");
            jsonFiles.Add("zpos", @"data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_bk.pn");
            jsonFiles.Add("zneg", @"data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_ft.png");

            jsonData.Add("files", jsonFiles);
            sendJson.Add("data", jsonData);

            Console.WriteLine(sendJson);
            string skyboxUpdateResponse = "";
            connection.SendViaTunnel(sendJson, response => skyboxUpdateResponse = response);

            Console.WriteLine(skyboxUpdateResponse);
        }

        public static void SetSkyBoxTime(ref Connection connection, float time)
        {
            // set the skybox type to dynamic
            JObject sendJson = new JObject();
            sendJson.Add("id", JsonID.SCENE_SKYBOX_UPDATE);

            JObject jsonData = new JObject();
            jsonData.Add("type", "dynamic");

            JObject jsonFiles = new JObject();
            jsonFiles.Add("xpos", @"data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_rt.png");
            jsonFiles.Add("xneg", @"data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_lf.png");
            jsonFiles.Add("ypos", @"data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_up.png");
            jsonFiles.Add("yneg", @"data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_dn.png");
            jsonFiles.Add("zpos", @"data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_bk.pn");
            jsonFiles.Add("zneg", @"data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_ft.png");

            jsonData.Add("files", jsonFiles);
            sendJson.Add("data", jsonData);

            Console.WriteLine(sendJson);

            string skyboxUpdateRespone = "";
            connection.SendViaTunnel(sendJson, response => skyboxUpdateRespone = response);

            Console.WriteLine(skyboxUpdateRespone);

            // set the time
            JObject tunnelSetTimeJson = new JObject { { "id", JsonID.SCENE_SKYBOX_SETTIME } };

            JObject dataJson = new JObject { { "time", time } };

            tunnelSetTimeJson.Add("data", dataJson);

            string skyboxSetTimeResponse = "";
            connection.SendViaTunnel(tunnelSetTimeJson, response => skyboxSetTimeResponse = response);
        }
    }
}