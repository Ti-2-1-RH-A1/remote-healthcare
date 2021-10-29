using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avans.TI.BLE;
using RemoteHealthcare.Bike;
using RemoteHealthcare.Hrm;

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

        public async Task<int> Start(string deviceId, string serviceName, string subscribtionCharacteristic)
        {
            // Wait for half a second in case time is needed to recognise bluetooth devices
            Thread.Sleep(500);

            List<string> lstdev = ble.ListDevices();

            lstdev.ForEach(Console.WriteLine);
            int timesTried = 0;
            int errorCode = 0; // set default to 0;
            do
            {
                errorCode = 0;
                Console.WriteLine("Connecting to: " + deviceId);
                errorCode += await ble.OpenDevice(deviceId);
                timesTried++;
            } while (errorCode != 0 && timesTried < 5);

            if (timesTried != 5 && errorCode == 0)
            {
                errorCode += await ble.SetService(serviceName);
                errorCode += await ble.SubscribeToCharacteristic(subscribtionCharacteristic);
                ble.SubscriptionValueChanged += DataReceived;
                Console.WriteLine("Connected to: "+ deviceId);

            }
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
