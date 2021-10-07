namespace RemoteHealthcare
{
    /// <summary>
    /// DataTypes are all the different types a bike can send.
    /// BIKE_SPEED          => m/s
    /// BIKE_ELAPSED_TIME   => second
    /// BIKE_DISTANCE       => meter
    /// BIKE_RPM            => revolutions per minute
    /// HMM_HEARTRATE       => beats per minute
    /// </summary>
    public enum DataTypes
    {
        BIKE_SPEED,
        BIKE_ELAPSED_TIME,
        BIKE_DISTANCE,
        BIKE_RPM,
        HRM_HEARTRATE
    }
}
