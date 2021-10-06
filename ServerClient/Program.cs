using System;
using System.Threading.Tasks;

namespace ServerClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string certificate = @"Server.pfx";
            new Server(certificate, new AuthHandler(), true);

            
            await Task.Delay(3000);

            Console.ReadLine();
        }
    }
}
