using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avans.TI.BLE;

namespace RemoteHealthcare
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            await Task.Run(MainBLE);
        }

        static async Task MainBLE()
        {
            int errorCode = 0;
            BLE bleBike = new BLE();
            BLE bleHeart = new BLE();
            Thread.Sleep(1000); // We need some time to list available devices

            // List available devices
            List<String> bleBikeList = bleBike.ListDevices();
            Console.WriteLine("Devices found: ");
            foreach (var name in bleBikeList)
            {
                Console.WriteLine($"Device: {name}");
            }

            // Connecting
            errorCode = errorCode = await bleBike.OpenDevice("Tacx Flux 00472");
            // __TODO__ Error check

            var services = bleBike.GetServices;
            foreach (var service in services)
            {
                Console.WriteLine($"Service: {service}");
            }

            // Set service
            errorCode = await bleBike.SetService("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");
            // __TODO__ error check

            // Subscribe
            bleBike.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
            errorCode = await bleBike.SubscribeToCharacteristic("6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");

            // Heart rate
            //errorCode =  await bleHeart.OpenDevice("Decathlon Dual HR");

            //await bleHeart.SetService("HeartRate");

            //bleHeart.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
            //await bleHeart.SubscribeToCharacteristic("HeartRateMeasurement");


            Console.Read();
        }

        private static void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            Console.WriteLine("Received from {0}: {1}, {2}", e.ServiceName,
                BitConverter.ToString(e.Data).Replace("-", " "),
                Encoding.UTF8.GetString(e.Data));
            var sync = e.Data[0];
            int msgLength = e.Data[1];
            var msgID = e.Data[2];
            int channelNumber = e.Data[3];
            var cs = e.Data[msgLength + 3];
            var msg = new Byte[msgLength];
            Array.Copy(e.Data, 4, msg, 0, msgLength);
            int dataPageNumber = msg[0];
            Console.WriteLine("sync: " + sync.ToString());
            Console.WriteLine("msgLength" + msgLength.ToString());
            Console.WriteLine("msgID: " + msgID.ToString());
            Console.WriteLine("channelNumber: " + channelNumber.ToString());
            Console.WriteLine("dataPageNumber: " + dataPageNumber.ToString());
            Console.WriteLine("cs: " + cs.ToString());
            Console.WriteLine(BitConverter.ToString(msg).Replace("-", " "));
            ParseData(msg);
        }

        public static void ParseData(byte[] data)
        {
            switch (data[0])
            {
                case 0x10:
                    Page16(data);
                    break;
                default:
                    Console.WriteLine("Not 16");
                    break;
            }
        }


        public static void Page16(byte[] data)
        {
            //byte byteTemp = data[5];

            //var output = byteTemp || data[4];
            //Console.WriteLine(output);
            Byte[] SpeedBytes = new Byte[4];
            SpeedBytes[3] = data[4];
            SpeedBytes[2] = data[5];
            var byte1 = BitConverter.ToSingle(SpeedBytes, 0);
            Console.WriteLine(byte1);


        }
    }
}
