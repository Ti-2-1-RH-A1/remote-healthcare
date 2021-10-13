using System;
using System.Threading;
using System.Threading.Tasks;
using Avans.TI.BLE;
using RemoteHealthcare.bike;
using RemoteHealthcare.hrm;
using Avans.TI.BLE;
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

        public event BLESubscriptionValueChangedEventHandler DataReceived;

        public BLEInstance BLEInstance { get; set; }

        public Bluetooth(BLEInstance type)
        {
            this.BLEInstance = type;
            this.ble = new BLE();
        }

        public void Dispose()
        {
            ble.CloseDevice();
        }

        public async Task<int> Start(RealBike realBike)
        {
            return await Start(RealBike.bikeTypeName + " " + realBike.bikeId, RealBike.bikeServiceName, RealBike.bikeSubscribtionCharacteristic);
        }

        public async Task<int> Start(HRM hrm)
        {
            return await Start(HRM.hrmTypeName, HRM.heartRateServiceName, HRM.heartSubscribtionCharacteristic);
        }

        public async Task<int> Start(string deviceId, string serviceName, string subscribtionCharacteristic)
        {
            // Wait for half a second in case time is needed to recognise bluetooth devices
            Thread.Sleep(500);

            int errorCode = 0; // set default to 0;
            errorCode += await ble.OpenDevice(deviceId);
            errorCode += await ble.SetService(serviceName);
            ble.SubscriptionValueChanged += DataReceived;
            errorCode += await ble.SubscribeToCharacteristic(subscribtionCharacteristic);
            // if errorcode > 0 then connection wasn't made properly
            // TODO [Martijn] Implement using errorcode to detect if connection was made
            return errorCode;
        }

        public void SetBikeResistance(byte resistance)
        {
            // datapage is 0x30 for setting resistance
            byte datapage = 0x30;
            byte zeroValue = 0x00;
            byte[] payload = { datapage, zeroValue, zeroValue, zeroValue, zeroValue, zeroValue, zeroValue, resistance};
            SendMessageToBike(payload);
        }

        private void SendMessageToBike(byte[] payload)
        {
            // Declare some standard values for the message.
            byte sync = 0xA4;
            byte length = 0x09;
            // 0x4E is for sending data to the bike
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

            ble.WriteCharacteristic(RealBike.bikeSendingCharacteristic, data);
        }
    }
}
