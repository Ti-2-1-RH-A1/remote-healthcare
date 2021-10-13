namespace RemoteHealthcare.Hrm
{
    class HRMDataParser
    {
        public static (DataTypes, float) ParseHRMData(byte[] data)
        {
            // 0x16 gives heartrate data
            if (data[0] == 0x16)
            {
                return (DataTypes.HRM_HEARTRATE, (float)data[1]);
            }
            return (DataTypes.NONE, 0f);
        }
    }
}
