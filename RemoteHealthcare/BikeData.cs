using Avans.TI.BLE;

namespace RemoteHealthcare
{
    public class BikeData
    {
        public byte[] Data { get; set; }
        public string ServiceName { get; set; }

        public BikeData(BLESubscriptionValueChangedEventArgs args)
        {
            this.Data = args.Data;
            this.ServiceName = args.ServiceName;
        }
    }
}
