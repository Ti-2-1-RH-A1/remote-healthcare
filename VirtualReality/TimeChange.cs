using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtualReality
{
    class TimeChange
    {
        private readonly Connection connection;

        public TimeChange(Connection connection)
        {
            this.connection = connection;
        }

        /// <summary>
        /// Change the <c>time</c> to the float specified
        /// </summary>
        /// <param name="time"></param>
        public void sendData(float time)
        {
            sendData(false);
            JObject tunnelSetTimeJson = new JObject { { "id", "scene/skybox/settime" } };

            JObject dataJson = new JObject { { "time", time } };

            tunnelSetTimeJson.Add("data", dataJson);

            string tunnelCreationResponse = "";
            connection.SendViaTunnel(tunnelSetTimeJson, response => tunnelCreationResponse = response);
            //
            // dynamic responseDeserializeObject = JsonConvert.DeserializeObject(tunnelCreationResponse);
            // string response = responseDeserializeObject.ToString();
        }

        /// <summary>
        /// Set the <c>skybox</c> to static 
        /// </summary>
        /// <param name="mode"></param>
        public void sendData(bool mode)
        {
            if (mode)
            {
                JObject sendJson = new JObject();
                sendJson.Add("id", "scene/skybox/update");

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


                string tunnelCreationResponse = "";
                connection.SendViaTunnel(sendJson, response => tunnelCreationResponse = response);

                Console.WriteLine(tunnelCreationResponse);
            }
            else
            {
                JObject sendJson = new JObject();
                sendJson.Add("id", "scene/skybox/update");

                JObject jsonData = new JObject();
                jsonData.Add("type", "dynamic");

                JObject sendJson = new JObject();
                sendJson.Add("id", "scene/skybox/update");

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
                string tunnelCreationResponse = "";
                connection.SendViaTunnel(sendJson, response => tunnelCreationResponse = response);



                Console.WriteLine(tunnelCreationResponse);
            }
        }
    }
}