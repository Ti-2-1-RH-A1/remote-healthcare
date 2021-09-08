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
                case 0x19:
                    Page25(data);
                    break;
                default:
                    Console.WriteLine("Not 16 or 25");
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

        public static void Page25(byte[] data)
        {
            // Calculate RPM
            int rpm = ParseRPM(data);
            Console.WriteLine("RPM: " + rpm);

            // Calculate Accumulated Power
            int AccPower = ParseAccPower(data);
            Console.WriteLine("AccPower: " + AccPower);

            // Calculate Instantaneous Power
            int InsPower = ParseAccPower(data);
            Console.WriteLine("InsPower: " + InsPower);
        }

        public static int ParseAccPower(byte[] data) => TwoByteToInt(data[3], data[4]);

        public static int ParseInsPower(byte[] data) => TwoByteToInt(data[5], (byte)(data[6] >> 4));

        public static int ParseRPM(byte[] data) => TwoByteToInt(data[2]);

        public static int ParseDistance(byte[] data) => TwoByteToInt(data[3]);
        
        public static float ParseElapsedTime(byte[] data) => TwoByteToInt(data[2]) * 0.25f;

        public static int ParseSpeed(byte[] data) => TwoByteToInt(data[4], data[5]);

        public static int TwoByteToInt(byte byte1, byte byte2 = 0)
        {
            byte[] bytes = new byte[2];
            bytes[0] = byte1;
            bytes[1] = byte2;
            return BitConverter.ToUInt16(bytes, 0);
        }

        // byte resistance =- basic resistance in % (0x00 == 0% and 0xFF == 100%
        public static void SetResistance(BLE bleBike, byte resistance)
        {
            byte datapage = 0x30;
            byte zeroValue = 0x00;
            byte[] payload = { datapage, zeroValue, zeroValue, zeroValue, zeroValue, zeroValue, zeroValue, resistance };
            sendBluetoothMessage(bleBike, payload);
        }

        // byte airResistanceCoefficient = Coefficient of the air resistance input in % of total (0x00 = 0% and 0xFF = 100%), where actual coefficient goes from 0.00 to 1.86 kg/m
        // byte windspeed = windspeed input in % of total (0x00 = 0% and 0xFF = 100%), where actual windspeed goes from -127 to 127 km/h
        // byte draftingFactor = drafting factor input in % of total (0x00 = 0% and 0xFF = 100%), where actual factor goes from 0.0 to 1.00 (the lower this factor, the less air resistance matters)
        public static void SetAirResistance(BLE bleBike, byte airResistanceCoefficient, byte windspeed, byte draftingFactor)
        {
            byte datapage = 0x32;
            byte zeroValue = 0x00;
            byte[] payload = { datapage, zeroValue, zeroValue, zeroValue, zeroValue, airResistanceCoefficient, windspeed, draftingFactor };
            sendBluetoothMessage(bleBike, payload);
        }

        private static void sendBluetoothMessage(BLE bleBike, byte[] payload)
        {
            // Declare some standard values for the message.
            byte sync = 0xA4;
            byte length = 0x09;
            byte msgId = 0x4E;
            byte channelNumber = 0x05;

            // Determine checksum
            byte checksum = 0x00;
            checksum ^= sync;
            checksum ^= length;
            checksum ^= msgId;
            checksum ^= channelNumber;
            foreach (byte b in payload)
            {
                checksum ^= b;
            }

            // length is payload + sync + length + msgId + channelnumber + checksum.
            // So length is payload.Length + 5
            byte[] data = new byte[payload.Length + 5];
            data[0] = sync;
            data[1] = length;
            data[2] = msgId;
            data[3] = channelNumber;
            payload.CopyTo(data, 4);
            data[data.Length - 1] = checksum;

            Console.WriteLine("Trying to send byte array: " + string.Join(", ", data));
            bleBike.WriteCharacteristic("6e40fec3-b5a3-f393-e0a9-e50e24dcca9e", data);
        }
    }
}
