﻿using VirtualReality;

namespace RemoteHealthcare
{
    class Program
    {
        public BikeManager bikeManager = new BikeManager();
        public HRManager hrManager = new HRManager();

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
