using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using ServerClient.Data;

namespace ServerClient
{
    public class ClientHandler
    {
        private readonly TcpClient tcpClient;
        public string authKey;
        private readonly AuthHandler auth;
        private readonly Stream stream;
        private readonly ClientsManager manager;
        private readonly DataHandler dataHandler;
        public delegate void Callback(Dictionary<string, string> header, Dictionary<string, string> data);
        public Dictionary<string, Callback> actions;
        private readonly byte[] buffer = new byte[1024];
        private string totalBufferText = "";

        public bool IsDoctor { get; set; }

        public ClientHandler(TcpClient tcpClient, Stream stream, AuthHandler auth, ClientsManager manager)
        {
            this.dataHandler = new DataHandler();
            dataHandler.LoadAllData();
            this.manager = manager;
            this.tcpClient = tcpClient;
            this.auth = auth;
            this.stream = stream;
            actions = new Dictionary<string, Callback>() {
                { "Login", LoginMethode() },
                { "Disconnect", disconnectCallback()},
                {"GetClients",GetClients()}
            };
            this.stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }

        private Callback GetClients()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                header.TryGetValue("Serial", out string serial);
                SendPacket(header, new Dictionary<string, string>(){
                    { "Result", "Ok" },
                    {"Data",Util.StringifyClients(manager.GetClients())}
                });
            };
        }

        private Callback disconnectCallback()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                SendPacket(header, new Dictionary<string, string>()
                {
                    {"Result", "ok"},
                    {"message", "Request for disconnect received"}
                });
                
                stream.Close();
                tcpClient.Close();
                manager.Disconnect(this);
            };
        }

        private Callback LoginMethode()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                header.TryGetValue("Auth", out string key);
                var (DoesExist, IsDoctor) = auth.Check(key);
                if (!DoesExist)
                {
                    SendPacket(header, new Dictionary<string, string>(){
                        { "Result", "Error" },
                        {"message","Key doesn't exist"},
                    });
                    Console.WriteLine("Key doesn't exist");
                    return;
                }

                authKey = key;
                if (IsDoctor)
                {

                    
                        SendPacket(header, new Dictionary<string, string>(){
                            { "Result", "ok" },
                            {"message","Doctor logged in."},
                        });
                    
                    Console.WriteLine("Doctor logged in.");
                    this.IsDoctor = true;
                }
                else
                {
                    if (data.TryGetValue("id", out string id))
                    {
                        SendPacket(header, new Dictionary<string, string>()
                        {
                            {"Result", "ok"},
                            {"message", "Patient logged in."},
                        });
                    }
                    else
                    {
                        Guid myuuid = Guid.NewGuid();
                        SendPacket(header, new Dictionary<string, string>()
                        {
                            {"Result", "ok"},
                            {"message", "Patient logged in."},
                            {"id", myuuid.ToString()},
                        });
                        id = myuuid.ToString();
                    }

                    data.TryGetValue("name", out string name);
                    dataHandler.AddFile(id, name);
                    Console.WriteLine("Patient logged in.");
                    this.IsDoctor = false;
                }
            };
        }

        private void OnRead(IAsyncResult ar)
        {
            try
            {
                int receivedBytes = stream.EndRead(ar);
                string receivedText = Encoding.ASCII.GetString(buffer, 0, receivedBytes);
                totalBufferText += receivedText;
            }
            catch (IOException)
            {
                stream.Close();
                tcpClient.Close();
                manager.Disconnect(this);
                return;
            }

            while (totalBufferText.Contains("\r\n\r\n\r\n"))
            {
                (Dictionary<string, string> header, Dictionary<string, string> data) = Protocol.ParsePacket(totalBufferText);

                totalBufferText = totalBufferText.Substring(totalBufferText.IndexOf("\r\n\r\n\r\n") + (totalBufferText.Length - totalBufferText.IndexOf("\r\n\r\n\r\n")));
                HandleData(header, data);
            }

            if (stream.CanRead) stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }

        private void HandleData(Dictionary<string, string> header, Dictionary<string, string> data)
        {
            header.TryGetValue("Method", out string item);

            if (actions.TryGetValue(item, out Callback action))
            {
                action(header, data);
            }
            else
            {
                SendPacket(header, new Dictionary<string, string>(){
                    { "Result", "Error" },
                    { "message", "Method not found" },
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
