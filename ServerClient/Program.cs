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
            new Server(certificate, AuthHandler.Init(), false);

            //var client = new Client("localhost", "Fiets", false, "Robin 1");

            //var client = new Client("localhost", "Fiets", true, "name");

            await Task.Delay(30000);
            var client2 = new Client("localhost", "Fiets", false, "Robin 2");


            await Task.Delay(10000);
            client2.Disconnect();
            await Task.Delay(-1);

            Console.ReadLine();
        }
    }
}
