using RemoteHealthcare.Bike;
using RemoteHealthcare.ServerCom;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteHealthcare
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            var deviceManager = new DeviceManager();
            deviceManager.Start(Init());
        }

        /// <summary>
        /// Select bike id to connect to using user input
        /// </summary>
        private static (IBikeManager.BikeType, string) Init()
        {
            // TODO [Martijn] Implement fileIO so the program automatically takes the latest bike id

            // Use simulator bike or realBike?
            Console.WriteLine("Use a real bike or simulator bike? type [y] for real bike or [n] for simulator bike");
            string bikeTypeChoice = Console.ReadLine().ToLower();
            if (bikeTypeChoice.Contains("n"))
            {
                return (IBikeManager.BikeType.SIMULATOR_BIKE, null);
            }

            // Ask the user for the bike id to connect to
            bool running = true;
            Console.WriteLine("Give a 5 digit serial id for the hometrainer");
            string bikeIdInput = "";
            while (running)
            {
                bikeIdInput = Console.ReadLine();
                if (Regex.IsMatch(bikeIdInput, "[/d{5}]"))
                {
                    // if the input consists of 5 digits stop the loop, else ask for input again
                    running = false;
                }
                else
                {
                    Console.WriteLine("Input wasn't a 5 digit serial id, try again");
                }
            }
            return (IBikeManager.BikeType.REAL_BIKE ,bikeIdInput);
        }
    }
}
