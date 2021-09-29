using System.IO.Compression;
using ServerClient;

namespace RemoteHealthcare
{
    class Program
    {
        public BikeManager bikeManager;
        public HRManager hrManager;

        public Program(BikeManager bikeManager, HRManager hrManager)
        {
            this.bikeManager = bikeManager;
            this.hrManager = hrManager;
        }


        static void Main(string[] args)
        {
            BikeManager bikeManager = new BikeManager();
            bikeManager.MakeConnectionAsync("01249").Wait();
            //bikeManager.StartSim();
            HRManager hrManager = new HRManager();

            Program program = new Program(bikeManager,hrManager);
            //ConsoleGUI cGUI = new ConsoleGUI(program);
            //cGUI.SelectionHandler();
            ClientDataHandler client = new ClientDataHandler(bikeManager, hrManager);



        }
    }
}
