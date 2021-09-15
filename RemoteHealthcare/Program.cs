using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RemoteHealthcare
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleGUI cGUI = new ConsoleGUI();
            cGUI.SelectionHandler();
        }
    }
}
