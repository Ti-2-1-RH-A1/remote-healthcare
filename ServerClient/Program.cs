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
            new Server(certificate, new AuthHandler(), false);

            var client = new Client("localhost", "Fiets", false);



            Console.ReadLine();
            await Task.Delay(3000);

            client.DataReceived += (e1, e2) =>
            {
                System.Console.WriteLine(e1);
                System.Console.WriteLine(e2);
            };

            client.SendPacket(new Dictionary<string, string>() {
                { "Method", "Get" }
            }, new Dictionary<string, string>());




            Console.ReadLine();
        }
    }
}
