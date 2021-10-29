using System.Collections.Generic;

namespace RemoteHealthcare.Hrm
{
    class HRMDataParser
    {
        public static Dictionary<DataTypes, float> ParseHRMData(byte[] data)
        {

            Dictionary<DataTypes, float> hrmData = new Dictionary<DataTypes, float>();
            // 0x16 gives heartrate data
            if (data[0] == 0x16) hrmData.Add(DataTypes.HRM_HEARTRATE, (float)data[1]);
            return hrmData;
        }
    }
}
