using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace RemoteHealthcare
{
    class ConsoleGUI
    {


        public Task SelectionHandler()
        {
            bool validSelection = false;
            SimulatorBike simulator = new SimulatorBike();
            BikeManager bike = new BikeManager();
            HRManager hr = new HRManager();
            while (!validSelection)
            {
                switch (this.mainMenu())
                {
                    case "0":
                        Console.Clear();
                        simulator.startSim();
                        break;
                    case "1":
                        Console.Clear();
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Hieronder 1 item gegenereerd door de simulator.");
                        Console.BackgroundColor = ConsoleColor.Black;
                        int i = 0;
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        simulator.RunStep(ref i, ref stopwatch);
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Druk op een knop om door te gaan.");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ReadKey();
                        break;
                    case "2":

                        Console.Clear();

                        Console.Write("Wat is het serie nummer van de fiets? : ");
                        string serie = Console.ReadLine();
                        bool enteryValid = false;
                        int amountEntry = 0;
                        while (!enteryValid)
                        {
                            Console.Clear();

                            Console.Write("Hoeveel data pakketen wil je ontvangen: ");
                            try
                            {
                                amountEntry = Int32.Parse(Console.ReadLine());
                                if (amountEntry! > 0)
                                {
                                    throw new Exception();
                                }

                                enteryValid = true;
                            }
                            catch (Exception e)
                            {
                                Console.BackgroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine("Getal invoer is niet correct probeer opnieuw (Druk op een knop)");
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ReadKey();
                            }
                        }

                        Console.Clear();
                        bike.MakeConnectionAsync(serie, amountEntry);
                        break;
                    case "3":
                        bool validEntery = false;
                        int entryAmount = 0;
                        while (!validEntery)
                        {
                            Console.Clear();

                            Console.Write("Hoeveel data pakketen wil je ontvangen: ");
                            try
                            {
                                entryAmount = Int32.Parse(Console.ReadLine());
                                if (entryAmount ! > 0)
                                {
                                    throw new Exception();
                                }

                                validEntery = true;
                            }
                            catch (Exception e)
                            {
                                Console.BackgroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine("Getal invoer is niet correct probeer opnieuw (Druk op een knop)");
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ReadKey();
                            }
                        }
                        Console.Clear();
                        hr.MakeConnection(entryAmount);
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
    

        public string mainMenu()
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

    }
}
