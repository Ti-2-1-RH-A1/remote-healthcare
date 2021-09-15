using System;
using System.Net.Sockets;
using System.Threading;

namespace VirtualReality
{
    class Program
    {
        

        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            client.Connect("145.48.6.10", 6666);
            string data = @"{""id"" : ""session/list""}";
            byte[] byteData = System.Text.Encoding.ASCII.GetBytes(data);
            int length = byteData.Length;

            NetworkStream networkStream = client.GetStream();           
            networkStream.Write(BitConverter.GetBytes(length));
            networkStream.Write(byteData);

            

            byte[] buffer = new byte[4];
            int rc = networkStream.Read(buffer, 0, 4);
            int packetLength = BitConverter.ToInt32(buffer);
            byte[] packetBuffer = new byte[packetLength];
            int receivedTotal = 0;
            while (receivedTotal < packetLength)
            {
                rc = networkStream.Read(packetBuffer, receivedTotal, packetLength - receivedTotal);
                receivedTotal += rc;
            }
            Console.WriteLine(System.Text.Encoding.ASCII.GetString(packetBuffer));
        }
    }
}
