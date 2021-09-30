using System;
using System.Collections.Generic;
using System.IO;
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
        public readonly string authKey;
        private readonly TcpClient client;
        private Stream stream;
        private readonly byte[] buffer;
        private string totalBufferText;
        public bool loggedIn;
        private bool useSSL;

        public delegate void DataReceivedHandler(object Client, DataReceivedArgs PacketInformation);
        public event EventHandler DataReceived;

        public Client(string host = "localhost", string authkey = "fiets", bool useSSL = true)
        {
            this.useSSL = useSSL;
            authKey = authkey;
            client = new TcpClient();
            client.BeginConnect(host, 7777, new AsyncCallback(OnConnect), null);
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

            try
            {
                if (useSSL)
                {
                    SslStream sslStream = new(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

                    // Try to authenticate as Client
                    sslStream.AuthenticateAsClient("localhost");

                    stream = sslStream;
                }
                else
                {
                    stream = client.GetStream();
                }
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                if (e.InnerException != null) Console.WriteLine($"Inner exception: {e.InnerException.Message}");
                Console.WriteLine("Authentication failed - closing the connection.");
                stream.Close();
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
                totalBufferText = totalBufferText[(totalBufferText.IndexOf("\r\n\r\n\r\n") + (totalBufferText.Length - totalBufferText.IndexOf("\r\n\r\n\r\n")))..];
                HandleData(packet);
            }

            if (stream.CanRead) stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
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
                if (!(data.TryGetValue("Result", out string resultValue)))
                {
                    Console.WriteLine("Response from server did not contain result. Skipping packet!");
                    return;
                }

                if (methodValue == "Login")
                {
                    if (resultValue == "Error")
                    {
                        data.TryGetValue("message", out string messageValue);
                        Console.WriteLine("Received error packet: {0}", messageValue);
                        SendPacket(new Dictionary<string, string>() {
                            { "Method", "Disconnect" }
                        }, new Dictionary<string, string>());


                        return;
                    }

                    Console.WriteLine("Login");
                    loggedIn = true;
                }
                else if (methodValue == "Disconnect")
                {
                    stream.Close();
                    client.Close();
                }
                else
                {
                    DataReceived?.Invoke(this, new DataReceivedArgs(headers, data));
                }
            }
            else
            {
                Console.WriteLine("Geen method gevonden!");
            }
        }

    }
}
