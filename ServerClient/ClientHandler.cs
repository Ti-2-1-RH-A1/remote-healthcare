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
using System.Threading.Tasks;

namespace ServerClient
{
    internal class ClientHandler
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private SslStream sslStream;
        private byte[] buffer = new byte[1024];
        private string totalBuffer = "";

        public bool isDoctor { get; set; }


        public ClientHandler(TcpClient tcpClient, SslStream sslStream)
        {
            this.tcpClient = tcpClient;
            this.sslStream = sslStream;

            this.stream = this.tcpClient.GetStream();
            sslStream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }
        
        private void OnRead(IAsyncResult ar)
        {
            try
            {
                int receivedBytes = stream.EndRead(ar);
                string receivedText = System.Text.Encoding.ASCII.GetString(buffer, 0, receivedBytes);
                Console.WriteLine(receivedText);
                
                totalBuffer += receivedText;
            }
            catch (IOException)
            {
                sslStream.Close();
                tcpClient.Close();
                Program.Disconnect(this);
                return;
            }

            while (totalBuffer.Contains("\r\n\r\n\r\n"))
            {

                (Dictionary<string, string> headerData, string rawData) = ParseHeaders(totalBuffer);
                Dictionary<string, string> packetData = ParseData(rawData);
                totalBuffer = totalBuffer.Substring(totalBuffer.IndexOf("\r\n\r\n\r\n") + 6);
                handleData(packetData, headerData);
            }
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        } 
        
        private static (Dictionary<string, string>, string) StringToDict(string dataString, string separator)
        {
            int length = (byte)dataString[0];
            Dictionary<string, string> resultData = new();
            string rawData = dataString[1..];
            for (int i = 0; i < length; i++)
            {
                string dataKey = rawData.Substring(0, rawData.IndexOf(separator));
                rawData = rawData[(rawData.IndexOf(separator)+separator.Length)..];
                string dataValue = rawData.Substring(0, rawData.IndexOf(separator));
                rawData = rawData[(rawData.IndexOf(separator) + separator.Length)..];
                resultData.Add(dataKey, dataValue);
            }
            return (resultData, rawData);
        }
        

        private void handleData(Dictionary<string, string> packetData, Dictionary<string, string> headerData)
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
                       
                            this.isDoctor = true;
                        }
                        else
                        {
                            this.isDoctor = false; 
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

        private bool assertPacketData(string[] packetData, int requiredLength)
        {
            if (packetData.Length < requiredLength)
            {
                Write("error");
                return false;
            }
            return true;
        }

        private static string DictToString(Dictionary<string, string> data, string separator)
        {
            string result = (char)data.Count + "";
            foreach (var item in data)
            {
                result += item.Key + separator + item.Value + separator;
            }
            return result;
        }

        private void SendPacket(Dictionary<string, string> headers, Dictionary<string, string> data) =>
            Write($"{StringifyHeaders(headers)}{StringifyData(data)}");

        public void Write(string data)
        {
            try
            {
                sslStream.AuthenticateAsServer(Program.serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                
                // Display the properties and settings for the authenticated stream.
                DisplaySecurityLevel(sslStream);
                DisplaySecurityServices(sslStream);
                DisplayCertificateInformation(sslStream);
                DisplayStreamProperties(sslStream);

                //Write a message to the client.
                byte[] message = Encoding.ASCII.GetBytes(data + "\r\n\r\n\r\n");
                sslStream.Write(message);
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                sslStream.Close();
                tcpClient.Close();
                Program.Disconnect(this);
                return;
            }
            sslStream.Flush();
        }

        
        static void DisplaySecurityLevel(SslStream stream)
        {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }
        static void DisplaySecurityServices(SslStream stream)
        {
            Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
        }
        static void DisplayStreamProperties(SslStream stream)
        {
            Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
            Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
        }
        static void DisplayCertificateInformation(SslStream stream)
        {
            Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Local certificate is null.");
            }
            // Display the properties of the client's certificate.
            X509Certificate remoteCertificate = stream.RemoteCertificate;
            if (stream.RemoteCertificate != null)
            {
                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Remote certificate is null.");
            }
        }

        private static string StringifyHeaders(Dictionary<string, string> headers) => DictToString(headers, "\r\n");
        
        private static (Dictionary<string, string>, string) ParseHeaders(string headersString) => StringToDict(headersString, "\r\n");
        
        private static string StringifyData(Dictionary<string, string> data) => DictToString(data, "\r\n\r\n");
        
        private static Dictionary<string, string> ParseData(string dataString) => StringToDict(dataString, "\r\n\r\n").Item1;
    }

}
