using System;
using System.Threading.Tasks;

namespace ServerClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string certificate = @"Server.pfx";
            new Server(certificate, AuthHandler.Init(), false);

            var client = new Client("localhost", "Fiets", false);

            //var client = new Client("localhost", "Fiets", true, "name");

            await Task.Delay(3000);
            await Task.Delay(-1);

            Console.ReadLine();
        }
    }
}
