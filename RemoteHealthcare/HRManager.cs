using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avans.TI.BLE;

namespace RemoteHealthcare
{
    class HRManager


    {

        private static int amountDataSend = 0;
        private static int ThresholdDataAmount = 0;
        private static int OriginalrequestedDataAmount = 0;
        private static bool exit = false;
        private static bool reachedThreshold = false;

        private static BLE bleHeart = null;
        public HRManager()
        {
        }

        public void MakeConnection(int dataBlocks)
        {
            OriginalrequestedDataAmount = dataBlocks;
            ThresholdDataAmount = dataBlocks;
            int errorCode = 0;
            Console.WriteLine($"Connectie met HRM maken!");

            bleHeart = new BLE();
            Thread.Sleep(1000); // We need some time to list available devices
            List<String> bleHList = bleHeart.ListDevices();

            // Heart rate
            Task<int> task = bleHeart.OpenDevice("Decathlon Dual HR");
             errorCode = task.Result;
            if (errorCode == 1)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Connectie met HRM kan niet worden gemaakt. Druk op een toets om door te gaan!");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ReadLine();
                return;
            }
            var servicesHR = bleHeart.GetServices;
            
            Task<int> taskService = bleHeart.SetService("HeartRate");
            errorCode = taskService.Result;
            if (errorCode == 1)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Service instelling van HRM kan niet worden gezet. Druk op een toets om door te gaan!");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ReadLine();
                return;
            }
            bleHeart.SubscriptionValueChanged += BleHeart_SubscriptionValueChanged;
            
            Task<int> taskSub = bleHeart.SubscribeToCharacteristic("HeartRateMeasurement");
            errorCode = taskSub.Result;
            if (errorCode == 1)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Subscription van HRM kan niet worden gemaakt. Druk op een toets om door te gaan!");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ReadLine();
                return;
            }
            while (!exit)
            {
                if (amountDataSend >= ThresholdDataAmount)
                {
                    reachedThreshold = !reachedThreshold;
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.Write(
                        $"Er zijn {amountDataSend} ontvangen van de {ThresholdDataAmount} verzochte. Wil je nogmaals {OriginalrequestedDataAmount} ontvangen? (y/n)");
                    Console.BackgroundColor = ConsoleColor.Black;

                    if (Console.ReadLine() == "y")
                    {
                        ThresholdDataAmount = ThresholdDataAmount + OriginalrequestedDataAmount;

                        reachedThreshold = !reachedThreshold;
                    }
                    else
                    {
                        bleHeart.CloseDevice();
                        exit = true;
                    }


                }
            }

            return;
        }

        private static void BleHeart_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            if (!reachedThreshold)
            {
                if (e.Data[0] == 0x16)
                {
                    Console.WriteLine($"Heartrate: {e.Data[1]} BPM");
                    amountDataSend++;
                }
            }
        }

        public bool closeConnections()
        {
            if (bleHeart != null)
            {
                bleHeart.CloseDevice();
                return true;
            }

            return false;
        }


    }
}
