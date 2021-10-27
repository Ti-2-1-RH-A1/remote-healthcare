using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NetProtocol
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
        private readonly bool useSSL;
        private string name;
        public string UUID;

        public delegate void DataReceivedHandler(object Client, DataReceivedArgs PacketInformation);
        public event DataReceivedHandler DataReceived;

        public delegate void Callback(Dictionary<string, string> header, Dictionary<string, string> data);
        public Dictionary<int, Callback> serialActions;

        public Client(string host = "localhost", bool useSSL = true, string name = "No Name", string authkey = "")
        {
            this.useSSL = useSSL;
            serialActions = new Dictionary<int, Callback>();
            if (authkey == "") authKey = Auth.AuthKey.GetAuthKey();
            else authKey = authkey;
            client = new TcpClient();
            client.BeginConnect(host, 7777, new AsyncCallback(OnConnect), null);
            buffer = new byte[1024];
            loggedIn = false;
            this.name = name;
            Console.WriteLine("Client created");
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None) return true;
            Console.WriteLine($"Certificate error: {sslPolicyErrors}");
            return false;
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void OnConnect(IAsyncResult ar)
        {
            client.EndConnect(ar);
            Console.WriteLine("Verbonden!");

            try
            {
                if (useSSL)
                {
                    SslStream sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

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

            if (!Directory.Exists("/client"))
            {
                Directory.CreateDirectory("/client");
            }

            if (File.Exists("/client/id.txt"))
            {
                string id = File.ReadAllText("/client/id.txt");
                SendPacket(new Dictionary<string, string>() {
                                { "Method", "Login" },
                                { "Auth", authKey }
                }, new Dictionary<string, string>()
                    {
                        { "id", id },
                        { "name", name },
                    });
                UUID = id;
            }
            else
            {
                SendPacket(new Dictionary<string, string>() {
                    { "Method", "Login" },
                    { "Auth", authKey },
                }, new Dictionary<string, string>()
                {
                    { "name", name },
                }, (Dictionary<string, string> header, Dictionary<string, string> data) =>
                {
                    if (data.TryGetValue("id", out string id))
                    {
                        UUID = id;
                    }
                    else
                    {
                        loggedIn = false;
                    }
                });
            }
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

        public void SendPacket(Dictionary<string, string> headers, Dictionary<string, string> data, Callback callback = null)
        {
            if (callback != null)
            {
                int randInt = new Random().Next();
                serialActions.Add(randInt, callback);
                headers.Add("Serial", randInt.ToString());
            }
            Write($"{Protocol.StringifyHeaders(headers)}{Protocol.StringifyData(data)}");
        }

        /// <summary>
        /// This method sends a packet to the server and returns the result headers and data.
        /// </summary>
        /// <param name="headers">Headers to send</param>
        /// <param name="data">Data to send</param>
        /// <returns>Result headers and data</returns>
        public async Task<(Dictionary<string, string> headers, Dictionary<string, string> data)>
            SendPacketAsync(Dictionary<string, string> headers, Dictionary<string, string> data) =>
            await Task.Run(() =>
            {
                var resolve = new TaskCompletionSource<(Dictionary<string, string>, Dictionary<string, string>)>();

                // Send packet and resolve on completed.
                SendPacket(headers, data, (resHeader, resData) => resolve.SetResult((resHeader, resData)));

                // return resolved result.
                return resolve.Task;
            });

        private void Write(string data)
        {
            byte[] dataAsBytes = Encoding.ASCII.GetBytes(data + "\r\n\r\n\r\n");
            stream.Write(dataAsBytes, 0, dataAsBytes.Length);
            stream.Flush();
        }

        public void Disconnect()
        {
            Console.WriteLine("[CLIENT] Sending disconnect packet");
            SendPacket(new Dictionary<string, string>() {
                { "Method", "Disconnect" },
            }, new Dictionary<string, string>());
            Console.WriteLine("[CLIENT] Send disconnect packet");
        }

        private void HandleData(string packetData)
        {
            (Dictionary<string, string> headers, Dictionary<string, string> data) = Protocol.ParsePacket(packetData);
            data.TryGetValue("message", out string messageValue);
            if (headers.TryGetValue("Method", out string methodValue))
            {
                
                if (methodValue == "Login")
                {
                    data.TryGetValue("Result", out string resultValue);
                    if (resultValue == "Error")
                    {
                        Console.WriteLine("Received error packet: {0}", messageValue);
                        SendPacket(new Dictionary<string, string>() {
                            { "Method", "Disconnect" },
                        }, new Dictionary<string, string>());
                        return;
                    }

                    if (data.TryGetValue("id", out string id))
                    {
                        File.WriteAllText("/client/id.txt", id);
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
                    if (headers.TryGetValue("Serial", out string serial))
                    {
                        if (int.TryParse(serial, out int serialInt))
                        {
                            if (serialActions.TryGetValue(serialInt, out Callback action))
                            {
                                action(headers, data);
                                serialActions.Remove(serialInt);
                            }
                        }
                    }
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
