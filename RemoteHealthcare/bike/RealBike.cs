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
        public string bikeId { get; set; }

        private readonly IServiceProvider services;
        private readonly Bluetooth bluetooth;

        public RealBike(IServiceProvider serviceProvider)
        {
            this.services = serviceProvider;

            this.bluetooth = services.GetServices<Bluetooth>().Where(b => b.BLEInstance == BLEInstance.BIKE).FirstOrDefault();
            bluetooth.DataReceived += Ble_DataReceived;
        }

        private void Ble_DataReceived(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            services.GetService<DeviceManager>().HandleData(BikeDataParser.ParseBikeData(e.Data));
        }

        public void SetResistance(int resistance)
        {
            // TODO [Martijn] Implementation
            throw new NotImplementedException();
        }

        public void Start(string bikeId = null)
        {
            // bikeId shouldn't be null, as handled before Start is called upon
            this.bikeId = bikeId;
            bluetooth.Start(this);
        }
    }
}
