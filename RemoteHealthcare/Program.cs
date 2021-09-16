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
            Program program = new Program();
            cGUI.SelectionHandler(program);
        }
    }
}
