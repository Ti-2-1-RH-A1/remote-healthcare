using System;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace ServerClient
{
    internal class ClientHandler
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private byte[] buffer = new byte[1024];
        private string totalBuffer = "";

        public bool isDoctor { get; set; }


        public ClientHandler(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;

            this.stream = this.tcpClient.GetStream();
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }
        #region connection stuff
        private void OnRead(IAsyncResult ar)
        {
            try
            {
                int receivedBytes = stream.EndRead(ar);
                string receivedText = System.Text.Encoding.ASCII.GetString(buffer, 0, receivedBytes);
                totalBuffer += receivedText;
            }
            catch (IOException)
            {
                Program.Disconnect(this);
                return;
            }

            while (totalBuffer.Contains("\r\n\r\n\r\n"))
            {
                string packet = totalBuffer.Substring(0, totalBuffer.IndexOf("\r\n\r\n\r\n"));
                totalBuffer = totalBuffer.Substring(totalBuffer.IndexOf("\r\n\r\n\r\n") + 6);
                int headerLenght;
                try
                {
                    headerLenght = int.Parse(packet.Substring(0));
                }
                catch (FormatException e)
                { 
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Header lenght not found.");
                    Program.Disconnect(this);
                    return;
                }

                string[] headerData = new string[headerLenght];

                for (int i = 0; i < headerLenght; i++)
                {
                    headerData[i] = packet.Substring(0, packet.IndexOf("\r\n"));
                    packet = packet.Substring(packet.IndexOf("\r\n") + 2);
                }

                string[] packetData = Regex.Split(packet, "\r\n\r\n");
                handleData(packetData, headerData);
            }
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }
        #endregion

        private void handleData(string[] packetData, string[] headerData)
        {

            Console.WriteLine($"Got a packet: {packetData[0]}");
            switch (packetData[0])
            {

                case "login":
                    // if (*insert auth key auth*)
                    // {
                        Write("login\r\nok");
                        if (headerData[2] == "DoctorKey")
                        { 
                            Write("You are a doctor");
                            this.isDoctor = true;
                        }
                        else
                        {
                            this.isDoctor = false;
                            Write("You are a patient");
                        }
                    // else
                    // {
                    //    Write("Login\r\nerror\r\n\r\nGoodbye");
                    //    Program.Disconnect(this);
                    // }
                    break;
                
            }


        }

        private bool assertPacketData(string[] packetData, int requiredLength)
        {
            if (packetData.Length < requiredLength)
            {
                Write("error");
                return false;
            }
            return true;
        }

        public void Write(string data)
        {
            var dataAsBytes = System.Text.Encoding.ASCII.GetBytes(data + "\r\n\r\n");
            stream.Write(dataAsBytes, 0, dataAsBytes.Length);
            stream.Flush();
        }
    }

}
