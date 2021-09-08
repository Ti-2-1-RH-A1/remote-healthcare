using System;
using System.Threading.Tasks;

namespace RemoteHealthcare
{
    class Program
    {
        static Task Main(string[] args)
        {
            bool validSelection = false;
            while (!validSelection)
            {
                switch (consoleMenu())
                {
                    case "0":
                        validSelection = true;
                        Console.WriteLine("Selected 0");
                        break;
                    case "1":
                        validSelection = true;
                        Console.WriteLine("Selected 1");
                        break;
                    case "2":
                        validSelection = true;
                        Console.WriteLine("Selected 2");
                        break;
                    case "3":
                        validSelection = true;
                        Console.WriteLine("Selected 3");
                        break;
                    case "4":
                        validSelection = true;
                        Console.WriteLine("Selected 4");
                        break;
                    case "5":
                        validSelection = true;
                        Console.WriteLine("Selected 5");
                        break;
                }
            }

            return Task.CompletedTask;
        }

        static string consoleMenu()
        {
            Console.Clear();
            string menuTitle = @"
========================================================================================================================
                                         |      _|  _|_|_|_|  _|      _|  _|    _|  
                                        _|_|  _|_|  _|        _|_|    _|  _|    _|  
                                        _|  _|  _|  _|_|_|    _|  _|  _|  _|    _|  
                                        _|      _|  _|        _|    _|_|  _|    _|  
                                        _|      _|  _|_|_|_|  _|      _|    _|_|    
========================================================================================================================
";

            Console.WriteLine(menuTitle);
            string menuOption = @"
[0] - Option 0
[1] - Option 1
[2] - Option 2
[3] - Option 3
[4] - Option 4
[5] - Option 5
    ";
            Console.WriteLine(menuOption);
            Console.Write("Select option: ");
            return Console.ReadLine();

        }

    }
}
