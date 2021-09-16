namespace RemoteHealthcare
{
    class Program
    {
        public BikeManager bikeManager = new BikeManager();
        public HRManager hrManager = new HRManager();

        static void Main(string[] args)
        {
            Program program = new Program();
            ConsoleGUI cGUI = new ConsoleGUI(program);
            cGUI.SelectionHandler();
        }
    }
}
