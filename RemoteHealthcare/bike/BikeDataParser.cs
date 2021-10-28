using System;
using System.Collections.Generic;

namespace RemoteHealthcare.Bike
{
    public class BikeDataParser
    {
        public static Dictionary<DataTypes, float> ParseBikeData(byte[] data)
        {
            return ParseBikeMessageData(ParseBikeByteArrayData(data));
        }

        private static byte[] ParseBikeByteArrayData(byte[] data)
        {
            //var sync = data[0];
            int msgLength = data[1];
            //var msgID = data[2];
            //int channelNumber = data[3];
            //var cs = data[msgLength + 3];
            byte[] msg = new byte[msgLength];
            Array.Copy(data, 4, msg, 0, msgLength);
            //int dataPageNumber = msg[0];

            // return the msg part of the data
            return msg;
        }

        private static Dictionary<DataTypes, float> ParseBikeMessageData(byte[] data)
        {
            switch (data[0])
            {
                case 0x10:
                    return ParseBikeDataPage16(data);
                case 0x19:
                    return ParseBikeDataPage25(data);
                default:
                    // return an empty list if there's no data
                    return new Dictionary<DataTypes, float>();
            }
        }

        private static Dictionary<DataTypes,float> ParseBikeDataPage16(byte[] data)
        {
            Dictionary<DataTypes, float> convertedData = new Dictionary<DataTypes, float>();
            convertedData.Add(DataTypes.BIKE_ELAPSED_TIME, ParseElapsedTime(data));
            convertedData.Add(DataTypes.BIKE_DISTANCE, ParseDistance(data));
            // For the speed convert it from km/h to m/s
            convertedData.Add(DataTypes.BIKE_SPEED, (ParseSpeed(data) * 0.0036f));
            return convertedData;
        }

        private static Dictionary<DataTypes, float> ParseBikeDataPage25(byte[] data)
        {
            Dictionary<DataTypes, float> convertedData = new Dictionary<DataTypes, float>();
            convertedData.Add(DataTypes.BIKE_RPM, ParseRPM(data));

            return convertedData;
        }

        private static int ParseRPM(byte[] data) => TwoByteToInt(data[2]);

        private static int ParseDistance(byte[] data) => TwoByteToInt(data[3]);

        private static float ParseElapsedTime(byte[] data) => TwoByteToInt(data[2]) * 0.25f;

        private static int ParseSpeed(byte[] data) => TwoByteToInt(data[4], data[5]);

        private static int TwoByteToInt(byte byte1, byte byte2 = 0)
        {
            byte[] bytes = new byte[2];
            bytes[0] = byte1;
            bytes[1] = byte2;
            return BitConverter.ToUInt16(bytes, 0);
        }
    }
}
