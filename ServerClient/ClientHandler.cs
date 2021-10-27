using NetProtocol;
using ServerClient.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace ServerClient
{
    public class ClientHandler
    {
        private readonly TcpClient tcpClient;
        public string authKey;
        public string UUID;
        public string Name;
        private readonly AuthHandler auth;
        private readonly Stream stream;
        private readonly ClientsManager manager;
        public delegate void Callback(Dictionary<string, string> header, Dictionary<string, string> data);
        public Dictionary<string, Callback> actions;
        private readonly byte[] buffer = new byte[1024];
        private string totalBufferText = "";

        public bool IsDoctor { get; set; }

        public ClientHandler(TcpClient tcpClient, Stream stream, AuthHandler auth, ClientsManager manager)
        {
            
            this.manager = manager;
            this.tcpClient = tcpClient;
            this.auth = auth;
            this.stream = stream;
            actions = new Dictionary<string, Callback>() {
                { "Login", LoginMethode() },
                { "Disconnect", disconnectCallback() },
                { "GetClients", GetClients() },
                { "GetHistoryClients", GetHistoryClient()},
                { "GetHistory", GetHistoryData() },
                { "SendToClients", SendToClients() },
                { "Post", Post() },
                { "Get", Get() },
                { "PostRT", PostRT() }, // post realtime to doctor.
            };
            this.stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }
        
        private Callback Get()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                if (header.TryGetValue("Id", out string id))
                {
                    if (manager.dataHandler.ClientData.TryGetValue(id, out ClientData clientData))
                    {
                        if (header.TryGetValue("GetKeys", out string getKeys))
                        {
                            // Parse keys to recieve
                            List<string> keys = new(getKeys.Split(","));
                            Dictionary<string, string> result = new();

                            // Get data from datastore
                            PropertyInfo[] properties = typeof(ClientData).GetProperties();
                            foreach (PropertyInfo property in properties)
                            {
                                if (keys.Contains(property.Name))
                                {
                                    result.TryAdd(property.Name, property.GetValue(clientData) as string);
                                }
                            }

                            // Send result to client
                            result.Add("Result", "Ok");
                            SendPacket(header, result);
                            return;
                        }
                        SendError(header, "Keys not found!");
                        return;
                    }
                    SendError(header, "Client not found!");
                    return;
                }
                SendError(header, "ID not found!");
            };
        }

        public override string ToString() => UUID;

        private Callback Post()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                if (header.TryGetValue("Id", out string id))
                {
                    if (manager.dataHandler.ClientData.TryGetValue(id, out ClientData clientData))
                    {
                        PropertyInfo[] properties = typeof(ClientData).GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (data.TryGetValue(property.Name, out string value))
                            {
                                property.SetValue(clientData, value);
                            }
                        }

                        manager.dataHandler.StoreData(id, data);

                        SendPacket(header, new Dictionary<string, string>(){
                            { "Result", "Ok" },
                        });
                        return;
                    }
                    SendError(header, "Client not found!");
                    return;
                }
                SendError(header, "ID not found!");
            };
        }

        private Callback PostRT()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                manager.SendToClients(ClientsManager.ClientType.DOCTOR, "Realtime", data);
            };
        }

        private Callback SendToClients()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                manager.SendToClients(header, data);
            };
        }

        private Callback GetClients()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                SendPacket(header, new Dictionary<string, string>(){
                    { "Result", "Ok" },
                    { "Data", Util.StringifyClients(manager.GetClients()) },
                });
            };
        }

        private Callback disconnectCallback()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                SendPacket(header, new Dictionary<string, string>()
                {
                    { "Result", "ok" },
                    { "message", "Request for disconnect received" },
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
                (bool DoesExist, bool IsDoctor) = auth.Check(key);
                if (!DoesExist)
                {
                    SendError(header, "Key doesn't exist");
                    Console.WriteLine("Key doesn't exist "+key);
                    return;
                }

                authKey = key;
                if (IsDoctor)
                {
                    SendPacket(header, new Dictionary<string, string>(){
                        { "Result", "ok" },
                        { "message", "Doctor logged in." },
                    });

                    Console.WriteLine("Doctor logged in.");
                    this.IsDoctor = true;
                    this.UUID = "DOCTOR-" + Guid.NewGuid();
                    this.Name = "Doctor";
                }
                else
                {
                    if (data.TryGetValue("id", out string id))
                    {
                        SendPacket(header, new Dictionary<string, string>()
                        {
                            { "Result", "ok" },
                            { "message", "Patient logged in." },
                        });
                        this.UUID = id;
                    }
                    else
                    {
                        Guid myuuid = Guid.NewGuid();
                        SendPacket(header, new Dictionary<string, string>()
                        {
                            { "Result", "ok" },
                            { "message", "Patient logged in." },
                            { "id", myuuid.ToString() },
                        });
                        this.UUID = myuuid.ToString();
                    }

                    data.TryGetValue("name", out string name);
                    this.Name = name;
                    manager.dataHandler.AddFile(this.UUID, name);
                    Console.WriteLine("Patient logged in.");
                    this.IsDoctor = false;
                }
                if (!manager.dataHandler.ClientData.ContainsKey(UUID))
                    manager.dataHandler.ClientData.Add(this.UUID, new ClientData());
                manager.Add(this);
            };
        }

        private Callback GetHistoryClient()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                SendPacket(header, new Dictionary<string, string>()
                {
                    { "Result", "ok" },
                    { "data", manager.dataHandler.LoadClientHistory() },
                });
            };
        }

        private Callback GetHistoryData()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                data.TryGetValue("client_id", out string id);
                SendPacket(header, new Dictionary<string, string>()
                {
                    { "Result", "ok" },
                    { "data", manager.dataHandler.LoadClientHistoryData(id).ToString() },
                });
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

                return;
            }

            while (totalBufferText.Contains("\r\n\r\n\r\n"))
            {
                (Dictionary<string, string> header, Dictionary<string, string> data) = Protocol.ParsePacket(totalBufferText);

                totalBufferText = totalBufferText[(totalBufferText.IndexOf("\r\n\r\n\r\n") + (totalBufferText.Length - totalBufferText.IndexOf("\r\n\r\n\r\n")))..];
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
                return;
            }
            SendError(header, "Method not found");
        }

        public void SendError(Dictionary<string, string> header, string message) =>
            SendPacket(header, new Dictionary<string, string>(){
                { "Result", "Error" },
                { "Message", message },
            });

        public void SendPacket(Dictionary<string, string> headers, Dictionary<string, string> data) =>
            Write($"{Protocol.StringifyHeaders(headers)}{Protocol.StringifyData(data)}");

        public void Write(string data)
        {
            byte[] message = Encoding.ASCII.GetBytes(data + "\r\n\r\n\r\n");
            stream.Write(message);
            stream.Flush();
        }
    }
}
