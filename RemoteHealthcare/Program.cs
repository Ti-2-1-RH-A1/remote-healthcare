using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RemoteHealthcare
{
    class Program
    {
        public SimulatorBike simulator = new SimulatorBike();
        public BikeManager bikeManager = new BikeManager();
        public HRManager hrManager = new HRManager();
        static void Main(string[] args)
        {
            ConsoleGUI cGUI = new ConsoleGUI();
            cGUI.SelectionHandler(this);
        }


    }
}
