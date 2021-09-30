using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ServerClient
{
    internal class ClientHandler
    {
        private readonly TcpClient tcpClient;
        private readonly AuthHandler auth;
        private readonly Stream stream;
        public delegate void Callback(Dictionary<string, string> packetData, Dictionary<string, string> headerData);
        public Dictionary<string, Callback> actions;
        private readonly byte[] buffer = new byte[1024];
        private string totalBufferText = "";

        public bool IsDoctor { get; set; }

        public ClientHandler(TcpClient tcpClient, Stream stream, AuthHandler auth)
        {
            this.tcpClient = tcpClient;
            this.auth = auth;
            this.stream = stream;
            actions = new Dictionary<string, Callback>() {
                { "Login", LoginMethode() },
            };
            this.stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }

        private Callback LoginMethode()
        {
            return delegate (Dictionary<string, string> packetData, Dictionary<string, string> headerData)
            {
                headerData.TryGetValue("Auth", out string key);
                var (DoesExist, IsDoctor) = auth.Check(key);
                if (!DoesExist)
                {
                    SendPacket(new Dictionary<string, string>() {
                        { "Method", "Login" }
                    }, new Dictionary<string, string>(){
                        { "Result", "Error" },
                        {"message","Key doesn't exist"}
                    });
                    Console.WriteLine("Key doesn't exist");
                    return;
                }
                if (IsDoctor)
                {
                    SendPacket(new Dictionary<string, string>() {
                        { "Method", "Login" }
                    }, new Dictionary<string, string>(){
                        { "Result", "ok" },
                        {"message","Doctor logged in."}
                    });
                    Console.WriteLine("Doctor logged in.");
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
                    Console.WriteLine("Patient logged in.");
                }
            };
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
            Console.WriteLine($"Got a packet: {item}");

            if (actions.TryGetValue(item, out Callback action))
            {
                action(packetData, headerData);
            }
            else
            {
                SendPacket(new Dictionary<string, string>() {
                    { "Method", item }
                }, new Dictionary<string, string>(){
                    { "Result", "Error" },
                    { "message", "Method not found" }
                });
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
