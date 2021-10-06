using Avans.TI.BLE;
using System;
using System.Threading.Tasks;
using avansBikeData = Avans.TI.BLE.BLESubscriptionValueChangedEventArgs;

namespace RemoteHealthcare
{
    public class Bluetooth
    {
        public static async Task<int> SetConnectionAsync(BLE ble, string device, string service, BLESubscriptionValueChangedEventHandler sub, string characteristic)
        {
            int errorCode = 0; // set default to 0;
            errorCode += await ble.OpenDevice(device);
            errorCode += await ble.SetService(service);
            ble.SubscriptionValueChanged += sub;
            errorCode += await ble.SubscribeToCharacteristic(characteristic);
            return errorCode;
        }

        private static void BleHeart_SubscriptionValueChanged(object sender, avansBikeData e)
        {
            if (e.Data[0] != 0x16) { return; }
            Console.WriteLine($"Heartrate: {e.Data[1]} BPM");
        }

        public static void BleBike_SubscriptionValueChanged(avansBikeData bikeData)
        {
            var sync = bikeData.Data[0];
            int msgLength = bikeData.Data[1];
            var msgID = bikeData.Data[2];
            int channelNumber = bikeData.Data[3];
            var cs = bikeData.Data[msgLength + 3];
            var msg = new byte[msgLength];
            Array.Copy(bikeData.Data, 4, msg, 0, msgLength);
            int dataPageNumber = msg[0];

            // Parse msg data
            ParseData(msg);
        }

        public static bool ParseData(byte[] data)
        {
            switch (data[0])
            {
                case 0x10:
                    Page16(data);
                    return true;
                case 0x19:
                    Page25(data);
                    return true;
                default:
                    return false;
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
            int InsPower = ParseInsPower(data);
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
    }
}
