using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RemoteHealthcare
{
    class Program
    {
        static Task Main(string[] args)
        {
            bool validSelection = false;
            SimulatorBike simulator = new SimulatorBike();
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
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        simulator.RunStep(ref i, ref stopwatch);
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Press enter to continue.");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ReadLine();
                        break;
                    case "2":
                        Console.Clear();
                        
                        Console.Write("Wat is het serie nummer van de fiets: ");
                        string serie = Console.ReadLine();
                        Console.Write("Hoeveel data pakketen wil je ontvangen: ");
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

            return Task.CompletedTask;
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
            return Console.ReadLine();

        }
    }
}
