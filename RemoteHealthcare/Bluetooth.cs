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
    }
}
