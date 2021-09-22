using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace VirtualReality
{
    class Program
    {
        private NetworkStream networkStream;
        private Dictionary<string, string> userSessionsMap;
        private String currentSessionID;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start();
            program.SetSkyBox(program);
            }

        public Program()
        {
            // Initialise and connect to the TcpClient
            // On server: 145.48.6.10 and port: 6666
            TcpClient client = new TcpClient();
            client.Connect("145.48.6.10", 6666);
            // Request the session list from the server
            networkStream = client.GetStream();
            userSessionsMap = GetRunningSessions();
        }

        /// <summary>Start does <c>The beginning of the Program</c> This is the beginning of the program, als 
        /// sometimes called the start of a programs life</summary>
        ///
        public void Start()
        {
            foreach (KeyValuePair<string, string> keyValuePair in userSessionsMap)
            {
                Console.WriteLine(keyValuePair.ToString());
            }

            // get user input for which session to connect to
            Console.WriteLine("Which client should be connected to?");
            string userInput = Console.ReadLine();

            if (CreateTunnel(userInput))
            {
                Console.WriteLine("Succes connected to " + userInput);
            }
            else
            {
                Console.WriteLine("couldn't connect to that client");
            }

             // A Simple Test code
            // JObject jObject = new JObject();
            // jObject.Add("id", "scene/reset");
            //
            // jObject.Add("data", new JObject());
            //
            // SendViaTunnel(jObject);
            //
            // string test;
            // ReceiveFromTcp(out test);
            // Console.WriteLine(test);
            // Expected Response
            // {"id":"tunnel/send","data":{"id":"13bc8b1f-36fa-4464-850d-7b9d5e99ae2a","data":{"id":"scene/reset","status":"ok"}}}
        }

        /// <summary>CreateTunnel does <c>Creating a network tunnel</c> returns <returns>A Boolean</returns> sends the correct json and then checks connection status based on that it returns a boolean</summary>
        ///
        public Boolean CreateTunnel(String sessionID)
        {
            if (userSessionsMap.ContainsKey(sessionID))
            {
                Console.WriteLine("Creating a tunnel");
                // create a tunnel
                JObject tunnelCreateJson = new JObject {{"id", "tunnel/create"}};

                JObject dataJson = new JObject {{"session", userSessionsMap.GetValueOrDefault(sessionID)}};
                // place to set the key 
                string sessionKey = "";
                dataJson.Add("key", sessionKey);

                tunnelCreateJson.Add("data", dataJson);
                SendToTcp(tunnelCreateJson.ToString());
                string tunnelCreationResponse = "";


                ReceiveFromTcp(out tunnelCreationResponse);


                dynamic responseDeserializeObject = JsonConvert.DeserializeObject(tunnelCreationResponse);
                string response = responseDeserializeObject["data"]["status"].ToString();

                if (response != "ok")
                {
                    return false;
                }

                currentSessionID = responseDeserializeObject["data"]["id"].ToString();
            }
            else
            {
                return false;
            }

            return true;
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
        private Dictionary<string, string> GetRunningSessions()
        {
            JObject sessionJson = new JObject();
            sessionJson.Add("id", "session/list");
            SendToTcp(sessionJson.ToString());

            // receive the response
            string receivedData;
            ReceiveFromTcp( out receivedData);

            // parse the received data
            dynamic jsonData = JsonConvert.DeserializeObject(receivedData);

            JArray jsonDataArray = jsonData.data;

            Dictionary<string, string> userSessionsMap = new Dictionary<string, string>();
            foreach (JObject jObject in jsonDataArray)
            {
                if (jObject.ContainsKey("id") && jObject.ContainsKey("clientinfo"))
                {
                    string user = jObject["clientinfo"]["user"].ToString();
                    if (!userSessionsMap.ContainsKey(user))
                    {
                        userSessionsMap.Add(jObject["clientinfo"]["user"].ToString(), jObject["id"].ToString());
                    }
                }
            }
            return userSessionsMap;
        }

        /// <summary>SendViaTunnel does <c> a tcp data send via a tunnel</c> as long as you have made a connection first </summary>
        ///
        
        public void SendViaTunnel(JObject jObject)
        {

            if (currentSessionID.Length == 0)
            {
                Console.WriteLine("Not Connected to a Tunnel");
            }
            else
            {
                JObject tunnelJSon = new JObject();
                tunnelJSon.Add("id", "tunnel/send");
                JObject tunnelJObject = new JObject();
                tunnelJObject.Add("dest", currentSessionID);
                tunnelJObject.Add("data", jObject);
                tunnelJSon.Add("data", tunnelJObject);
                Console.WriteLine(tunnelJSon.ToString());
                SendToTcp(tunnelJSon.ToString());
            }
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

        public void SetSkyBox(Program program)
        {

            TimeChange timeChange = new TimeChange(program);
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
