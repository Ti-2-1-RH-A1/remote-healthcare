using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ServerClient
{
    public class Program
    {
        private static bool useSSL;
        private static TcpListener listener;
        private static List<ClientHandler> clients = new List<ClientHandler>();
        public static X509Certificate serverCertificate = null;
        // The certificate parameter specifies the name of the file
        // containing the machine certificate.
        public static void RunServer(string certificate, bool useSSL = true)
        {
            Program.useSSL = useSSL;
            try
            {
                if (useSSL) serverCertificate = X509Certificate.CreateFromCertFile(certificate);
                // Create a TCP/IP (IPv4) socket and listen for incoming connections.
                listener = new TcpListener(IPAddress.Any, 7777);
                listener.Start();

                Console.WriteLine("Waiting for a client to connect...");
                listener.BeginAcceptTcpClient(new AsyncCallback(ProcessClient), null);
            }
            catch (Exception)
            {
                throw new Exception("Setup server failure");
            }
        }

        static void ProcessClient(IAsyncResult ar)
        {
            TcpClient client = listener.EndAcceptTcpClient(ar);

            if (useSSL)
            {
                // Setup sslStream
                SslStream sslStream = new(client.GetStream(), false);

                //Authenticate the server but don't require the client to authenticate.
                sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);

                // Start handling client
                clients.Add(new ClientHandler(client, sslStream));
                listener.BeginAcceptTcpClient(new AsyncCallback(ProcessClient), null);
            }
            else
            {
                // Fallback no ssl
                NetworkStream stream = client.GetStream();
                clients.Add(new ClientHandler(client, stream));
                listener.BeginAcceptTcpClient(new AsyncCallback(ProcessClient), null);
            }
        }

        internal static void Disconnect(ClientHandler client)
        {
            clients.Remove(client);
            Console.WriteLine("Client disconnected");
        }

        public static async Task Main(string[] args)
        {
            string certificate = @"Server.pfx";
            RunServer(certificate, false);

            new Client("fiets", false);

            await Task.Delay(3000);
        }
    }
}
