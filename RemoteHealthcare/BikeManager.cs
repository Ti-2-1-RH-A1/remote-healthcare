using System;
using System.Threading;
using System.Threading.Tasks;
using Avans.TI.BLE;

using avansBikeData = Avans.TI.BLE.BLESubscriptionValueChangedEventArgs;

namespace RemoteHealthcare
{
    class BikeManager
    {
        private int amountDataSend = 0;
        private int ThresholdDataAmount = 0;
        private int OriginalrequestedDataAmount = 0;
        private bool exit = false;
        private bool reachedThreshold = false;

        private RealBike realBike = null;

        //class RealBike : IBikeData
        //{
        //    public byte[] Data { get; set; }
        //    public string ServiceName { get; set; }

        //    public RealBike(BLESubscriptionValueChangedEventArgs e)
        //    {
        //        this.Data = e.Data;
        //        this.ServiceName = e.ServiceName;
        //    }
        //}
        public BikeManager()
        {
        }

        public async Task MakeConnectionAsync(string deviceName, int dataBlocks)
        {
            OriginalrequestedDataAmount = dataBlocks;
            ThresholdDataAmount = dataBlocks;
            int errorCode = 0;

            realBike = new BLE();
            Console.WriteLine($"Connectie met fiets {deviceName} wordt gemaakt");
            Thread.Sleep(1000); // We need some time to list available devices

            errorCode = await Bluetooth.SetConnectionAsync(realBike, "Tacx Flux 00472", "6e40fec1-b5a3-f393-e0a9-e50e24dcca9e", BleBike_SubscriptionValueChanged, "6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");
            if (errorCode >= 1)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Connectie met fiets {deviceName} kan niet worden gemaakt. Druk op een toets om door te gaan!");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ReadLine();
                return;
            }
            Console.WriteLine($"Subscription op fiets {deviceName} is ingesteld.");

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
                        realBike.CloseDevice();
                        exit = true;
                    }
                }
            }
            return;
        }


        public bool CloseConnections()
        {
            if (realBike != null)
            {
                realBike.CloseDevice();  
                return true;
            }

            return false;
        }

        private static void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            Bluetooth.BleBike_SubscriptionValueChanged(e, false);
        }
    }
}
