using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace ServerClient
{
    internal class ClientHandler
    {
        private readonly TcpClient tcpClient;
        private readonly SslStream stream;
        private readonly byte[] buffer = new byte[1024];
        private string totalBufferText = "";

        public bool IsDoctor { get; set; }


        public ClientHandler(TcpClient tcpClient, SslStream sslStream)
        {
            this.tcpClient = tcpClient;
            stream = sslStream;
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }

        private void OnRead(IAsyncResult ar)
        {
            try
            {
                int receivedBytes = stream.EndRead(ar);
                string receivedText = Encoding.ASCII.GetString(buffer, 0, receivedBytes);
                Console.WriteLine(receivedText);

                totalBufferText += receivedText;
            }
            catch (IOException)
            {
                stream.Close();
                tcpClient.Close();
                Program.Disconnect(this);
                return;
            }

            while (totalBufferText.Contains("\r\n\r\n\r\n"))
            {
                (Dictionary<string, string> headerData, Dictionary<string, string> packetData) = Protocol.ParsePacket(totalBufferText);
                totalBufferText = totalBufferText.Substring(totalBufferText.IndexOf("\r\n\r\n\r\n") + 6);
                HandleData(packetData, headerData);
            }
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }

        private void HandleData(Dictionary<string, string> packetData, Dictionary<string, string> headerData)
        {
            headerData.TryGetValue("Method", out string item);
            headerData.TryGetValue("Auth", out string key);
            Console.WriteLine($"Got a packet: {item}");
            switch (item)
            {

                case "Login":
                    // if (*insert auth key auth*)
                    // {
                    if (key == "fiets")
                    {
                        SendPacket(new Dictionary<string, string>() {
                                { "Method", "Login" }
                            }, new Dictionary<string, string>(){
                                { "Result", "ok" },
                                {"message","Doctor logged in."}
                            });

                        this.IsDoctor = true;
                    }
                    else
                    {
                        this.IsDoctor = false;
                        SendPacket(new Dictionary<string, string>() {
                                { "Method", "Login" }
                            }, new Dictionary<string, string>(){
                                { "Result", "ok" },
                                {"message","Patient logged in."}
                            });
                    }
                    // else
                    // {
                    //    Write("Login\r\nerror\r\n\r\nGoodbye");
                    //    Program.Disconnect(this);
                    // }
                    break;

            }
        }

        private void SendPacket(Dictionary<string, string> headers, Dictionary<string, string> data) =>
            Write($"{Protocol.StringifyHeaders(headers)}{Protocol.StringifyData(data)}");

        public void Write(string data)
        {
            byte[] message = Encoding.ASCII.GetBytes(data + "\r\n\r\n\r\n");
            stream.Write(message);
            stream.Flush();
        }
    }
}
