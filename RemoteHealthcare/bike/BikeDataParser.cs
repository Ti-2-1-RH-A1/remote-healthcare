using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteHealthcare.bike
{
    public class BikeDataParser
    {
        public static (int,float) Parse(byte[] data)
        {
            //TODO [Martijn] Implementation
            return (0, 0);
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
    }
}
