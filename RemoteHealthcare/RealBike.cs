using System;
using System.Collections.Generic;
using System.Text;
using Avans.TI.BLE;

namespace RemoteHealthcare
{
    class RealBike : BLE, IBike
    {
        public RealBike() : base()
        {
                
        }

        // byte airResistanceCoefficient = Coefficient of the air resistance input in % of total (0x00 = 0% and 0xFF = 100%), where actual coefficient goes from 0.00 to 1.86 kg/m
        // byte windspeed = windspeed input in % of total (0x00 = 0% and 0xFF = 100%), where actual windspeed goes from -127 to 127 km/h
        // byte draftingFactor = drafting factor input in % of total (0x00 = 0% and 0xFF = 100%), where actual factor goes from 0.0 to 1.00 (the lower this factor, the less air resistance matters)
        public void SetResistance(byte resistance)
        {
            byte datapage = 0x30;
            byte zeroValue = 0x00;
            byte[] payload = { datapage, zeroValue, zeroValue, zeroValue, zeroValue, zeroValue, zeroValue, resistance };
            SendBluetoothMessage(payload);
        }

        // byte resistance =- basic resistance in % (0x00 == 0% and 0xFF == 100%
        public void SetAirResistance(byte airResistanceCoefficient, byte windspeed, byte draftingFactor)
        {
            byte datapage = 0x32;
            byte zeroValue = 0x00;
            byte[] payload = { datapage, zeroValue, zeroValue, zeroValue, zeroValue, airResistanceCoefficient, windspeed, draftingFactor };
            SendBluetoothMessage(payload);
        }

        private void SendBluetoothMessage(byte[] payload)
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
            this.WriteCharacteristic("6e40fec3-b5a3-f393-e0a9-e50e24dcca9e", data);
        }
    }
}
