using System;
using System.Threading.Tasks;
using NetProtocol;

namespace ServerClient
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            string certificate = @"Server.pfx";
            //var server = new Server(certificate, AuthHandler.Init());

            //var client = new Client("localhost", "Fiets", false, "Robin 1");
           
            // var client = new Client("localhost", true, "name", "e5OczxmOprhbpDVUtF4JmeM7gVdqrFDl");


            //Task.Delay(30000).Wait();
            var client2 = new Client("145.49.42.127", false, "Robin", "e5OczxmOprhbpDVUtF4JmeM7gVdqrFDl");
            
            
            //  Task.Delay(10000).Wait();
            // client2.Disconnect();
            // await Task.Delay(-1);

            //Console.ReadLine();
            return Task.Delay(-1);
        }
    }
}
