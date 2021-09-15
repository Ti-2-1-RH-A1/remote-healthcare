using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avans.TI.BLE;

namespace RemoteHealthcare
{
    class BikeManager
    {
        private static int amountDataSend = 0;
        private static int ThresholdDataAmount = 0;
        private static int OriginalrequestedDataAmount = 0;
        private static bool exit = false;
        private static bool reachedThreshold = false;

        private static BLE bleBike = null;

        class RealBike : IBikeData
        {
            public byte[] Data { get; set; }
            public string ServiceName { get; set; }

            public RealBike(BLESubscriptionValueChangedEventArgs e)
            {
                this.Data = e.Data;
                this.ServiceName = e.ServiceName;
            }
        }
        public BikeManager()
        {
        }

        public void MakeConnection(string deviceName, int dataBlocks)
        {
            OriginalrequestedDataAmount = dataBlocks;
            ThresholdDataAmount = dataBlocks;
            int errorCode = 0;
            bleBike = new BLE();
            Console.WriteLine($"Connectie met fiets {deviceName} wordt gemaakt");
            Thread.Sleep(1000); // We need some time to list available devices

            // List available devices
            List<String> bleBikeList = bleBike.ListDevices();
            // Console.WriteLine("Devices found: ");
            // foreach (var name in bleBikeList)
            // {
            //     Console.WriteLine($"Device: {name}");
            // }

            // Connecting
            Task<int> task = bleBike.OpenDevice($"Tacx Flux {deviceName}");
            errorCode = task.Result;
            // __TODO__ Error check
            if (errorCode == 1)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Connectie met fiets {deviceName} kan niet worden gemaakt. Druk op een toets om door te gaan!");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ReadLine();
                return;
            }
            var services = bleBike.GetServices;
            // foreach (var service in services)
            // {
            //     Console.WriteLine($"B Service: {service}");
            // }

            // Set service
            Task<int> taskService = bleBike.SetService("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");
            errorCode = taskService.Result;
            // __TODO__ error check
            if (errorCode == 1)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Service voor fiets {deviceName} is fout gegaan. Druk op enter om door te gaan!");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ReadLine();
                return;
            }
            Console.WriteLine($"Service voor fiets {deviceName} is ingesteld.");
            // Subscribe
            bleBike.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
            Task<int> taskSub = bleBike.SubscribeToCharacteristic("6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");
            
            errorCode = taskSub.Result;

            if (errorCode == 1)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Subscribe met fiets {deviceName} kan niet worden gemaakt. Druk op enter om door te gaan!");
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
                        bleBike.CloseDevice();
                        exit = true;
                    }
                }
            }
            return;
        }


        public bool closeConnections()
        {
            if (bleBike != null)
            {
                bleBike.CloseDevice();
                return true;
            }

            return false;
        }


        private static void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            RealBike realBike = new RealBike(e);
            BleBike_SubscriptionValueChanged(realBike);
        }

        public static void BleBike_SubscriptionValueChanged(IBikeData e)
        {
            if (!reachedThreshold)
            {

                // Console.WriteLine("Received from {0}: {1}, {2}", e.ServiceName,
                // BitConverter.ToString(e.Data).Replace("-", " "),
                // Encoding.UTF8.GetString(e.Data));
                var sync = e.Data[0];
                int msgLength = e.Data[1];
                var msgID = e.Data[2];
                int channelNumber = e.Data[3];
                var cs = e.Data[msgLength + 3];
                var msg = new Byte[msgLength];
                Array.Copy(e.Data, 4, msg, 0, msgLength);
                int dataPageNumber = msg[0];

                //logging
                // Console.WriteLine("sync: " + sync.ToString());
                // Console.WriteLine("msgLength" + msgLength.ToString());
                // Console.WriteLine("msgID: " + msgID.ToString());
                // Console.WriteLine("channelNumber: " + channelNumber.ToString());
                // Console.WriteLine("dataPageNumber: " + dataPageNumber.ToString());
                // Console.WriteLine("cs: " + cs.ToString());
                // Console.WriteLine(BitConverter.ToString(msg).Replace("-", " "));

                //Parse msg data
                if (Bluetooth.ParseData(msg))
                {
                    amountDataSend++;
                }

            }
        }
    }
}
