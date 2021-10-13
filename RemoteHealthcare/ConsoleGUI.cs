using System;
using System.Threading.Tasks;

namespace RemoteHealthcare
{
    internal enum MenuOptions
    {
        START_SIM = '0',
        RUN_STEP = '1',
        READ_BIKE = '2',
        READ_HEARTHRATE = '3',
        CLOSE_CONNECTION = '4',
    }

    internal class ConsoleGUI
    {
        private readonly Program program;

        public ConsoleGUI(Program program)
        {
            this.program = program;
        }

        public Task SelectionHandler()
        {
            bool validSelection = false;
            while (!validSelection)
            {
                switch (this.MainMenu())
                {
                    case (char)MenuOptions.START_SIM:
                        StartSim();
                        break;
                    case (char)MenuOptions.RUN_STEP:
                        RunStep();
                        break;
                    case (char)MenuOptions.READ_BIKE:
                        ReadBike();
                        break;
                    case (char)MenuOptions.READ_HEARTHRATE:
                        ReadHearthrate();
                        break;
                    case (char)MenuOptions.CLOSE_CONNECTION:
                        CloseConnection();
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private void CloseConnection()
        {
            this.program.bikeManager.CloseConnections();
            this.program.hrManager.CloseConnections();
        }

        private void ReadHearthrate()
        {
            bool validEntery = false;
            int entryAmount = 0;
            while (!validEntery)
            {
                Console.Clear();

                Console.Write("Hoeveel data pakketen wil je ontvangen: ");
                try
                {
                    entryAmount = int.Parse(Console.ReadLine());
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
        }

        private void ReadBike()
        {
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
                    amountEntry = int.Parse(Console.ReadLine());
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
        }

        private void RunStep()
        {
            Console.Clear();
            PrintLine(ConsoleColor.DarkRed, "Hieronder 1 item gegenereerd door de simulator.");
            this.program.bikeManager.SimRunStep();
            PrintLine(ConsoleColor.DarkRed, "Druk op een knop om door te gaan.");
            Console.ReadKey();
        }

        private void StartSim()
        {
            Console.Clear();
            this.program.bikeManager.StartSim();
        }

        private static void PrintLine(ConsoleColor color, string toPrint)
        {
            Console.BackgroundColor = color;
            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.Black;
        }

        public char MainMenu()
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
            return Console.ReadKey().KeyChar;
        }

    }
}
