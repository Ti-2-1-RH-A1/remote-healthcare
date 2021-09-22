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
        private readonly Program program;

        public TimeChange(Program program)
        {
            this.program = program;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>

        public void sendData(float time)
        {
            // create a tunnel
            JObject tunnelCreateJson = new JObject { { "id", "scene/skybox/settime" } };

            JObject dataJson = new JObject { { "time", time } };

            tunnelCreateJson.Add("data", dataJson);
            program.SendViaTunnel(tunnelCreateJson);
            string tunnelCreationResponse = "";



            program.ReceiveFromTcp(out tunnelCreationResponse);

            dynamic responseDeserializeObject = JsonConvert.DeserializeObject(tunnelCreationResponse);
            string response = responseDeserializeObject.ToString();
        }
        public void sendData()
        {




            //JObject sendJson = JObject.Parse(sendString);

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
            program.SendViaTunnel(sendJson);
            string tunnelCreationResponse = "";
            program.ReceiveFromTcp(out tunnelCreationResponse);

            Console.WriteLine(tunnelCreationResponse);
        }

    }
}
