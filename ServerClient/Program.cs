using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ServerClient
{
    class Program
    {
        private static TcpListener listener;
        private static List<ClientHandler> clients = new List<ClientHandler>();
        public static X509Certificate serverCertificate = null;
        // The certificate parameter specifies the name of the file
        // containing the machine certificate.
        public static void RunServer(string certificate, string privateKeyPath)
        {
            serverCertificate = X509Certificate2.CreateFromPemFile(certificate, privateKeyPath);
            // Create a TCP/IP (IPv4) socket and listen for incoming connections.
            listener = new TcpListener(IPAddress.Any, 7777);
            listener.Start();
            
                Console.WriteLine("Waiting for a client to connect...");
                // Application blocks while waiting for an incoming connection.
                // Type CNTL-C to terminate the server.
                listener.BeginAcceptTcpClient(new AsyncCallback(ProcessClient), null);
                Console.ReadLine();

        }


        static void ProcessClient(IAsyncResult ar)
        {
            var client = listener.EndAcceptTcpClient(ar);
            // A client has connected. Create the
            // SslStream using the client's network stream.
            // SslStream sslStream = new SslStream(
            //     client.GetStream(), true);
            // Authenticate the server but don't require the client to authenticate.
            //sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);
            clients.Add(new ClientHandler(client));
            listener.BeginAcceptTcpClient(new AsyncCallback(ProcessClient), null);
        }

        internal static void Disconnect(ClientHandler client)
        {
            clients.Remove(client);
            Console.WriteLine("Client disconnected");
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("To start the server specify:");
            Console.WriteLine("serverSync certificateFile.cer");
            Environment.Exit(1);
        }
        public static int Main(string[] args)
        {
             string certificate = @"C:\Users\robin\Documents\Avans\TI2.1\Proftaak\cert.cer";
             string privatekey = @"C:\Users\robin\Documents\Avans\TI2.1\Proftaak\private.key";
            // if (args == null || args.Length < 1)
            // {
            //     DisplayUsage();
            // }
           // certificate = args[0];
            RunServer(certificate, privatekey);
            return 0;
        }

        


    }
}