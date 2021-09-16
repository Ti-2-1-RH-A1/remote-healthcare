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
        public float time { get; set; }
        private readonly Program program;

        public TimeChange(float time, Program program)
        {
            this.program = program;
            this.time = time;
         //   sendData(time);
        }

        public void setTime(float time)
        {
            this.time = time;
           // sendData(time);
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
            program.SendToTcp(tunnelCreateJson.ToString());
            string tunnelCreationResponse = "";

           

            program.ReceiveFromTcp(out tunnelCreationResponse);

            dynamic responseDeserializeObject = JsonConvert.DeserializeObject(tunnelCreationResponse);
            string response = responseDeserializeObject["data"]["status"].ToString();
        }
        public void sendData(bool back)
        {
            string sendString = @"{
            ""id"" : ""scene/skybox/update"",
            ""data"" :
            {
                ""type"" : ""static"",
                ""files"" :
                {
                    ""xpos"" : ""data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_rt.png"",
                    ""xneg"" : ""data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_lf.png"",
                    ""ypos"" : ""data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_up.png"",
                    ""yneg"" : ""data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_dn.png"",
                    ""zpos"" : ""data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_bk.png"",
                    ""zneg"" : ""data/NetworkEngine/textures/SkyBoxes/interstellar/interstellar_ft.png""

                }
            }
        }";
            JObject sendJson = JObject.Parse(sendString);
            program.SendToTcp(sendJson.ToString());
            string tunnelCreationResponse = "";



            program.ReceiveFromTcp(out tunnelCreationResponse);

            dynamic responseDeserializeObject = JsonConvert.DeserializeObject(tunnelCreationResponse);
            string response = responseDeserializeObject["data"]["status"].ToString();
        }

        public void SetStatic()
        {
            sendData(true);
        }
        
    }
}
