using System;
using System.Threading;
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

        public async void MakeConnection(int dataBlocks)
        {
            OriginalrequestedDataAmount = dataBlocks;
            ThresholdDataAmount = dataBlocks;
            int errorCode = 0;
            Console.WriteLine($"Connectie met HRM maken!");

            bleHeart = new BLE();
            Thread.Sleep(1000); // We need some time to list available devices

            // connect
            errorCode = await Bluetooth.SetConnectionAsync(bleHeart, "Decathlon Dual HR", "HeartRate", BleHeart_SubscriptionValueChanged, "HeartRateMeasurement");
            if (errorCode >= 1)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Connectie met HRM kan niet worden gemaakt. Druk op een toets om door te gaan!");
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

        public bool CloseConnections()
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
