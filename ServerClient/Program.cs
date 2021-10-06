using System;
using System.Threading.Tasks;

namespace ServerClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string certificate = @"Server.pfx";
            new Server(certificate, AuthHandler.Init(), true);

            //var client = new Client("localhost", "Fiets", true, "name");

            await Task.Delay(3000);

            
            // client.SendPacket(new Dictionary<string, string>() {
            //     { "Method", "Get" }
            // }, new Dictionary<string, string>(), (header, data) =>
            // {
            //     Console.WriteLine(e1);
            //     Console.WriteLine(e2);
            // });

            Console.ReadLine();
        }
    }
}
