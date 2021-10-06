using Avans.TI.BLE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteHealthcare.bike
{
    public class BikeDataParser
    {
        public static (int, float) ParseBikeData(byte[] data)
        {
            return ParseBikeMessageData(ParseByteArrayData(data));
        }

        public static byte[] ParseByteArrayData(byte[] data)
        {
            var sync = data[0];
            int msgLength = data[1];
            var msgID = data[2];
            int channelNumber = data[3];
            var cs = data[msgLength + 3];
            byte[] msg = new byte[msgLength];
            Array.Copy(data, 4, msg, 0, msgLength);
            int dataPageNumber = msg[0];

            // return the msg part of the data
            return msg;
        }

        public static (int,float) ParseBikeMessageData(byte[] data)
        {
            // TODO [Martijn] Implementation
            return (0, 0);
        }
    }
}
