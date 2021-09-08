using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avans.TI.BLE;

namespace RemoteHealthcare
{
    public class Bluetooth
    {
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
                Console.WriteLine($"B Service: {service}");
            }

            // Set service
            errorCode = await bleBike.SetService("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");
            // __TODO__ error check

            // Subscribe
            bleBike.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
            errorCode = await bleBike.SubscribeToCharacteristic("6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");

            // Heart rate
            errorCode = await bleHeart.OpenDevice("Decathlon Dual HR");
            var servicesHR = bleHeart.GetServices;
            foreach (var service in servicesHR)
            {
                Console.WriteLine($"HR Service: {service}");
            }

            await bleHeart.SetService("HeartRate");

            bleHeart.SubscriptionValueChanged += BleHeart_SubscriptionValueChanged;
            await bleHeart.SubscribeToCharacteristic("HeartRateMeasurement");

            Console.Read();
        }

        private static void BleHeart_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            if (e.Data[0] != 0x16)
                return;
            Console.WriteLine($"Heartrate: {e.Data[1]} BPM");
        }

        public class RealBike : IBike
        {
            public byte[] Data { get; set; }
            public string ServiceName { get; set; }

            public RealBike(BLESubscriptionValueChangedEventArgs e)
            {
                this.Data = e.Data;
                this.ServiceName = e.ServiceName;
            }
        }

        private static void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            RealBike realBike = new RealBike(e);
            BleBike_SubscriptionValueChanged(realBike);
        }

        public static void BleBike_SubscriptionValueChanged(IBike e)
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
            Console.WriteLine("Distance: " + ParseDistance(data));

            // Calculate speed.
            float speed = ParseSpeed(data);
            Console.WriteLine("\nSpeed: " + speed * 0.001 * 3.6 + "\n");
        }

        public static int ParseDistance(byte[] data)
        {
            return TwoByteToInt(data[3]);
        }

        public static float ParseElapsedTime(byte[] data)
        {
            return TwoByteToInt(data[2]) * 0.25f;
        }

        public static int ParseSpeed(byte[] data)
        {
            return TwoByteToInt(data[4], data[5]);
        }

        public static int TwoByteToInt(byte byte1, byte byte2 = 0)
        {
            byte[] bytes = new byte[2];
            bytes[0] = byte1;
            bytes[1] = byte2;
            return BitConverter.ToUInt16(bytes, 0);
        }
    }
}
