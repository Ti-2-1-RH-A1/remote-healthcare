using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ServerClient
{
    class Client
    {
        private readonly string authKey;
        private readonly TcpClient client;
        private NetworkStream stream;
        private readonly byte[] buffer;
        private string totalBuffer;
        private bool loggedIn;

        public Client(string authkey = "fiets")
        {
            authKey = authkey;
            client = new TcpClient();
            client.BeginConnect("145.49.15.68", 7777, new AsyncCallback(OnConnect), null);
            buffer = new byte[1024];
            loggedIn = false;

            while (true)
            {
                Console.WriteLine("Voer een chatbericht in:");
                string newChatMessage = Console.ReadLine();
                if (loggedIn)
                    Write($"chat\r\n{newChatMessage}");
                else
                    Console.WriteLine("Je bent nog niet ingelogd");
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            client.EndConnect(ar);
            Console.WriteLine("Verbonden!");
            stream = client.GetStream();
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
            SendPacket(new Dictionary<string, string>() {
                { "Method", "Login" },
                { "Auth", authKey }
            }, new Dictionary<string, string>());
        }

        private void OnRead(IAsyncResult ar)
        {
            int receivedBytes = stream.EndRead(ar);
            string receivedText = Encoding.ASCII.GetString(buffer, 0, receivedBytes);
            totalBuffer += receivedText;

            while (totalBuffer.Contains("\r\n\r\n\r\n"))
            {
                string packet = totalBuffer.Substring(0, totalBuffer.IndexOf("\r\n\r\n\r\n"));
                totalBuffer = totalBuffer[(totalBuffer.IndexOf("\r\n\r\n\r\n") + 6)..];
                HandleData(packet);
            }
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }

        private void SendPacket(Dictionary<string, string> headers, Dictionary<string, string> data) =>
            Write($"{Protocol.StringifyHeaders(headers)}{Protocol.StringifyData(data)}");

        private void Write(string data)
        {
            var dataAsBytes = Encoding.ASCII.GetBytes(data + "\r\n\r\n\r\n");
            stream.Write(dataAsBytes, 0, dataAsBytes.Length);
            stream.Flush();
        }

        private void HandleData(string packetData)
        {
            (Dictionary<string, string> headers, Dictionary<string, string> data) = Protocol.ParsePacket(packetData);

            if (headers.TryGetValue("Method", out string methodValue))
            {
                switch (methodValue)
                {
                    case "Login":
                        Console.WriteLine("Login");
                        loggedIn = true;
                        break;

                    case "Get":
                        Console.WriteLine("Get");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Geen method gevonden!");
            }
        }

    }
}
