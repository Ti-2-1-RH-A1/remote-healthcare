using Avans.TI.BLE;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace RemoteHealthcare.bike
{
    public class RealBike : Avans.TI.BLE.BLE, IBike
    {
        private static Func<(int, float)> callback;
        public string bikeId { get; set; }

        private readonly IServiceProvider services;

        public RealBike(IServiceProvider serviceProvider)
        {
            this.services = serviceProvider;

            Bluetooth bluetooth = services.GetServices<Bluetooth>().Where(b => b.BLEInstance == BLEInstance.Bike).FirstOrDefault();
            bluetooth.DataReceived += Ble_DataReceived;
        }

        private void Ble_DataReceived(object sender, Avans.TI.BLE.BLESubscriptionValueChangedEventArgs e)
        {
            Console.WriteLine($"Received: {e.Data}");
        }

        public RealBike(Func<(int,float)> callbackGiven)
        {
            callback = callbackGiven;
        }

        public void SetResistance(int resistance)
        {
            throw new NotImplementedException();
        }

        public void Start(string bikeId = null)
        {
            bikeId = bikeId;
            throw new NotImplementedException();
        }

        public void BleBike_SubscriptionValueChanged(BLESubscriptionValueChangedEventArgs bikeData)
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

        //private static void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        //{

        //    Bluetooth.BleBike_SubscriptionValueChanged(e);
        //}
    }
}
