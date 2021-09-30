using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace ServerClient
{
    public class Server
    {
        private readonly bool useSSL;
        private readonly AuthHandler auth;
        private readonly TcpListener listener;
        private readonly ClientsManager clientsManager;
        private readonly X509Certificate serverCertificate;
        // The certificate parameter specifies the name of the file
        // containing the machine certificate.
        public Server(string certificate, AuthHandler auth, bool useSSL = true)
        {
            this.useSSL = useSSL;
            this.auth = auth;
            clientsManager = new ClientsManager();
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

        private void ProcessClient(IAsyncResult ar)
        {
            TcpClient client = listener.EndAcceptTcpClient(ar);

            if (useSSL)
            {
                // Setup sslStream
                SslStream sslStream = new(client.GetStream(), false);

                //Authenticate the server but don't require the client to authenticate.
                sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);

                // Start handling client
                clientsManager.Add(new ClientHandler(client, sslStream, auth, clientsManager));
                listener.BeginAcceptTcpClient(new AsyncCallback(ProcessClient), null);
            }
            else
            {
                // Fallback no ssl
                NetworkStream stream = client.GetStream();
                clientsManager.Add(new ClientHandler(client, stream, auth, clientsManager));
                listener.BeginAcceptTcpClient(new AsyncCallback(ProcessClient), null);
            }
        }

    }
}
