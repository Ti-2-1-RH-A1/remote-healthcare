using System;
using System.Text;
using System.Threading.Tasks;
using Avans.TI.BLE;
using RemoteHealthcare.bike;

using avansBikeData = Avans.TI.BLE.BLESubscriptionValueChangedEventArgs;

namespace RemoteHealthcare
{
    public enum BLEInstance
    {
        HEARTRATE,
        BIKE,
    };

    public class Bluetooth : IDisposable
    {

        private BLE ble;
        private const string ServiceName = "6e40fec1-b5a3-f393-e0a9-e50e24dcca9e";
        private const string Characteristic = "6e40fec2-b5a3-f393-e0a9-e50e24dcca9e";

        public event BLESubscriptionValueChangedEventHandler DataReceived;

        public BLEInstance BLEInstance { get; set; }

        public Bluetooth(BLEInstance type)
        {
            this.BLEInstance = type;
            this.ble = new BLE();
        }

        public void Dispose()
        {
            // TODO [Martijn] Disconnect the bluetooth connection
        }

        public async Task<int> Start(bike.RealBike realBike)
        {
            return await Start("Tacx flux" + realBike.bikeId);
        }

        /*public async Task<int> Start(HRM hrm)
        {
            return await Start("Tacx flux" + realBike.bikeId);
        }*/

        public async Task<int> Start(string deviceId)
        {
            int errorCode = 0; // set default to 0;
            errorCode += await ble.OpenDevice(deviceId);
            errorCode += await ble.SetService(ServiceName);
            ble.SubscriptionValueChanged += DataReceived;
            errorCode += await ble.SubscribeToCharacteristic(Characteristic);
            return errorCode;
        }



        private void BleHeart_SubscriptionValueChanged(object sender, avansBikeData e)
        {
            if (e.Data[0] != 0x16) { return; }
            Console.WriteLine($"Heartrate: {e.Data[1]} BPM");
        }

        
        /*
        public bool ParseData(byte[] data)
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

        public void Page16(byte[] data)
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

        public void Page25(byte[] data)
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

        public int ParseAccPower(byte[] data) => TwoByteToInt(data[3], data[4]);

        public int ParseInsPower(byte[] data) => TwoByteToInt(data[5], (byte)(data[6] >> 4));

        public int ParseRPM(byte[] data) => TwoByteToInt(data[2]);

        public int ParseDistance(byte[] data) => TwoByteToInt(data[3]);

        public float ParseElapsedTime(byte[] data) => TwoByteToInt(data[2]) * 0.25f;

        public int ParseSpeed(byte[] data) => TwoByteToInt(data[4], data[5]);

        public int TwoByteToInt(byte byte1, byte byte2 = 0)
        {
            byte[] bytes = new byte[2];
            bytes[0] = byte1;
            bytes[1] = byte2;
            return BitConverter.ToUInt16(bytes, 0);
        }*/


    }
}
