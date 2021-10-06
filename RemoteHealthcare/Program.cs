using VirtualReality;

namespace RemoteHealthcare
{
    class Program
    {
        public DeprecatedBikeManager bikeManager = new DeprecatedBikeManager();
        public DeprecatedHRManager hrManager = new DeprecatedHRManager();

        static void Main(string[] args)
        {
            //VrManager vrManager = new VrManager();
            //vrManager.Start();

            //Program program = new Program();
            //ConsoleGUI cGUI = new ConsoleGUI(program);
            //cGUI.SelectionHandler();


            var deviceManager = new DeviceManager();

        }
    }
}
