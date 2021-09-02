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
            /** Test code voor TwoByteToInt
             Byte[] SpeedBytes = new Byte[8];

            SpeedBytes[4] = 0b11001011;
            SpeedBytes[5] = 0b00010001;

            float test = ParseSpeed(SpeedBytes);
            Console.WriteLine(test);

            Byte[] SpeedBytes = new Byte[8];
            SpeedBytes[2] = 0b11001011;
            Page16(SpeedBytes);
            float test = ParseElapsedTime(SpeedBytes[2]);
            Console.WriteLine(test);
            **/

            Byte[] SpeedBytes = new Byte[8];

            SpeedBytes[3] = 0b11001011;

            float test = parseDistance(SpeedBytes);
            Console.WriteLine(test);

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
            
            //logging
            Console.WriteLine("sync: " + sync.ToString());
            Console.WriteLine("msgLength" + msgLength.ToString());
            Console.WriteLine("msgID: " + msgID.ToString());
            Console.WriteLine("channelNumber: " + channelNumber.ToString());
            Console.WriteLine("dataPageNumber: " + dataPageNumber.ToString());
            Console.WriteLine("cs: " + cs.ToString());
            Console.WriteLine(BitConverter.ToString(msg).Replace("-", " "));
            
            //Parse msg data
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
            // Calculate Elapsed Time.
            float time = ParseElapsedTime(data);
            Console.WriteLine("Elapsed Time: " + time);

            // Calculate Distance Traveled.
            Console.WriteLine("Distance: " + parseDistance(data));

            // Calculate speed.
            float speed = ParseSpeed(data);
            Console.WriteLine("Speed: " + speed);
        }

        private static int parseDistance(byte[] data)
        {
            return TwoByteToInt(data[3]);
        }
        private static float ParseElapsedTime(byte[] data)
        {
            int timeInt = TwoByteToInt(data[2]);
            return timeInt * 0.25f;
        }

        public static float ParseSpeed(Byte[] data)
        {
            int speedInt = TwoByteToInt(data[4], data[5]);
            return speedInt;
        }

        public static int TwoByteToInt(byte byte1, byte byte2 = 0)
        {
            Byte[] bytes = new Byte[2];
            bytes[0] = byte1;
            bytes[1] = byte2;
            return BitConverter.ToUInt16(bytes, 0);
        }
    }
}
