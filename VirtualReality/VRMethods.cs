using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualReality
{
    class VRMethods
    {
        /// <summary>
        /// Method to create terrain based on a heightmap
        /// </summary>
        /// <param name="connection"> connection to send data to and receive responses from</param>
        public void CreateTerrain(ref Connection connection)
        {

            Console.WriteLine("Enter a path to an heightmap");

            string entryPath = @"" + Console.ReadLine();
            if (!File.Exists(entryPath))
            {
                Console.WriteLine("No file found");
                return;
            }

            // convert the heightmap to a bitmap and then set that into a heightmap array
            Bitmap bitmap = new Bitmap(entryPath);
            int width = 256;
            int height = 256;
            float offset = 10f;
            float[] widthHeight = { width, height };

            float[] heightMap = new float[width * height];

            int index = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    heightMap[index++] = bitmap.GetPixel(i, j).R / 255f * offset;
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

            connection.SendViaTunnel(tunnelAddTerrainNode);
            // TODO receive the response to not clog the buffer
        }
    }
}
