using System.Threading.Tasks;

namespace ServerClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string certificate = @"Server.pfx";
            new Server(certificate, false);

            new Client("localhost", "Fiets", false);

            await Task.Delay(3000);
        }
    }
}
