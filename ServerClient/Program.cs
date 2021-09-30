using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string certificate = @"Server.pfx";
            new Server(certificate, new AuthHandler(), true);

            var client = new Client("localhost", "Fiets", true);


            await Task.Delay(3000);

            // client.SendPacket(new Dictionary<string, string>() {
            //     { "Method", "Get" }
            // }, new Dictionary<string, string>(), (e1, e2) =>
            // {
            //     Console.WriteLine(e1);
            //     Console.WriteLine(e2);
            // });

            Console.ReadLine();
        }
    }
}
