﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace RemoteHealthcare
{
    class ConsoleGUI
    {

        /// <summary>Method handlesc>The selection from the menu</c> and the function calls to the menu..</summary>
        ///
        public Task SelectionHandler(Program program)
        {
            bool validSelection = false;
            while (!validSelection)
            {
                switch (this.mainMenu())
                {
                    case "0":
                        Console.Clear();
                        program.simulator.startSim();
                        break;
                    case "1":
                        Console.Clear();
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Hieronder 1 item gegenereerd door de simulator.");
                        Console.BackgroundColor = ConsoleColor.Black;
                        int i = 0;
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        program.simulator.RunStep(ref i, ref stopwatch);
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
                            catch (Exception)
                            {
                                Console.BackgroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine("Getal invoer is niet correct probeer opnieuw (Druk op een knop)");
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ReadKey();
                            }
                        }

                        Console.Clear();
                        program.bikeManager.MakeConnectionAsync(serie, amountEntry).Wait();
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
                                if (entryAmount! > 0)
                                {
                                    throw new Exception();
                                }

                                validEntery = true;
                            }
                            catch (Exception)
                            {
                                Console.BackgroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine("Getal invoer is niet correct probeer opnieuw (Druk op een knop)");
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ReadKey();
                            }
                        }
                        Console.Clear();
                        program.hrManager.MakeConnection(entryAmount);
                        break;
                    case "4":
                        program.bikeManager.closeConnections();
                        program.hrManager.closeConnections();
                        break;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>Method shows <c>The menu</c> it returns <returns>(string)The key pressed by the user </returns></susmmary>
        ///
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
