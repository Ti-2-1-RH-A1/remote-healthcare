using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avans.TI.BLE;

namespace RemoteHealthcare
{
    class Program
    {
        static async Task Main(string[] args)
        {
            bool validSelection = false;
            Simulator simulator = new Simulator();
            BikeManager bike = new BikeManager();
            HRManager hr = new HRManager();
            while (!validSelection)
            {
                switch (consoleMenu())
                {
                    case "0":
                        Console.Clear();

                        simulator.startSim();
                        

                        break;
                    case "1":
                        Console.Clear();
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Below 1 entry from the simulated data is shown.");
                        Console.BackgroundColor = ConsoleColor.Black;
                        int i = 0;
                        Simulator.RunStep(ref i);
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Press any key to continue.");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ReadKey();
                        break;
                    case "2":
                        Console.Clear();
                        
                        Console.Write("Wat is het serie nummer van de fiets? : ");
                        string serie = Console.ReadLine();
                        Console.Write("Hoeveel data pakketen wil je ontvangen? : ");
                        int amount = 1;
                        try
                        {
                            amount = Int32.Parse(Console.ReadLine());
                        }
                        catch (Exception e)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine("Getal invoer is niet correct probeer opnieuw (Druk op enter)");
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ReadLine();
                            break;
                        }
                        Console.Clear();
                        bike.MakeConnection(serie, amount);
                        break;
                    case "3":
                        Console.Clear();
                        
                        Console.Write("Hoeveel data pakketen wil je ontvangen: ");
                        int amountHR = 1;
                        try
                        {
                            amountHR = Int32.Parse(Console.ReadLine());
                        }
                        catch (Exception e)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine("Getal invoer is niet correct probeer opnieuw (Druk op enter)");
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ReadLine();
                            break;
                        }
                        Console.Clear();
                        hr.MakeConnection(amountHR);
                        break;
                    case "4":
                        bike.closeConnections();
                        hr.closeConnections();
                        break;
                    case "5":
                        // validSelection = true;
                        // Console.WriteLine("Selected 5");
                        break;
                }
            }
            // await Task.Run(MainBLE);
        }


        static string consoleMenu()
        {
            Console.Clear();
            string menuTitle = @"
════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════  
                                        █   █   ████    █   █   █   █
                                        ██ ██   █       ██  █   █   █
                                        █ █ █   ████    █ █ █   █   █
                                        █   █   █       █  ██   █   █
                                        █   █   ████    █   █    ███
════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════
";

            Console.WriteLine(menuTitle);
            string menuOption = @"
[0] - Start Simulator
[1] - 1 data entry simulator
[2] - Start reading from bike
[3] - Start Reading from Heartrate Monitor
[4] - Force close connection from bike
    ";
            Console.WriteLine(menuOption);
            Console.Write("Select option: ");
            return Console.ReadKey().KeyChar.ToString();

        }



        

        public static bool ParseData(byte[] data)
        {
            switch (data[0])
            {
                case 0x10:
                    Page16(data);
                    return true;
                default:
                    // Console.WriteLine("Not 16");
                    return false;
            }
        }

        public static void Page16(byte[] data)
        {
            // Calculate Elapsed Time.
            float time = ParseElapsedTime(data);
            Console.WriteLine("Elapsed Time: " + time);

            // Calculate Distance Traveled.
            Console.WriteLine("Distance: " + parseDistance(data));

            // Calculate speed.
            float speed = ParseSpeed(data);
            Console.WriteLine("\nSpeed: " + speed * 0.001 * 3.6 + "\n");
        }

        private static int parseDistance(byte[] data)
        {
            return TwoByteToInt(data[3]);
        }

        private static float ParseElapsedTime(byte[] data)
        {
            int timeInt = TwoByteToInt(data[2]);
            return timeInt * 0.25f;
        }

        public static float ParseSpeed(Byte[] data)
        {
            int speedInt = TwoByteToInt(data[4], data[5]);
            return speedInt;
        }

        public static int TwoByteToInt(byte byte1, byte byte2 = 0)
        {
            Byte[] bytes = new Byte[2];
            bytes[0] = byte1;
            bytes[1] = byte2;
            return BitConverter.ToUInt16(bytes, 0);
        }
    }
}
