using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ServerClient
{
    public class DataReceivedArgs : EventArgs
    {
        public Dictionary<string, string> headers { get; }
        public Dictionary<string, string> data { get; }

        public DataReceivedArgs(Dictionary<string, string> headers, Dictionary<string, string> data)
        {
            this.headers = headers;
            this.data = data;
        }
    }

    public class Client
    {
        private readonly string authKey;
        private readonly TcpClient client;
        private SslStream stream;
        private readonly byte[] buffer;
        private string totalBufferText;
        private bool loggedIn;
        public delegate void DataReceivedHandler(object Client, DataReceivedArgs PacketInformation);
        public event EventHandler DataReceived;

        public Client(string authkey = "fiets")
        {
            authKey = authkey;
            client = new TcpClient();
            client.BeginConnect("localhost", 7777, new AsyncCallback(OnConnect), null);
            buffer = new byte[1024];
            loggedIn = false;

            Console.WriteLine("Client created");
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None) return true;
            Console.WriteLine($"Certificate error: {sslPolicyErrors}");
            return false;
        }

        private void OnConnect(IAsyncResult ar)
        {
            client.EndConnect(ar);
            Console.WriteLine("Verbonden!");
            stream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            
            // Try to authenticate as Client
            try
            {
                stream.AuthenticateAsClient("localhost");
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                if (e.InnerException != null) Console.WriteLine($"Inner exception: {e.InnerException.Message}");
                Console.WriteLine("Authentication failed - closing the connection.");
                client.Close();
                return;
            }

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
            totalBufferText += receivedText;

            while (totalBufferText.Contains("\r\n\r\n\r\n"))
            {
                string packet = totalBufferText.Substring(0, totalBufferText.IndexOf("\r\n\r\n\r\n"));
                totalBufferText = totalBufferText[(totalBufferText.IndexOf("\r\n\r\n\r\n") + 6)..];
                HandleData(packet);
            }
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }

        public void SendPacket(Dictionary<string, string> headers, Dictionary<string, string> data) =>
            Write($"{Protocol.StringifyHeaders(headers)}{Protocol.StringifyData(data)}");

        private void Write(string data)
        {
            byte[] dataAsBytes = Encoding.ASCII.GetBytes(data + "\r\n\r\n\r\n");
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
                        DataReceivedArgs PacketInformation = new DataReceivedArgs(headers, data);
                        DataReceived?.Invoke(this, PacketInformation);
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
