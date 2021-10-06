using Avans.TI.BLE;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace RemoteHealthcare.bike
{
    public class RealBike : BLE, IBike
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
            List<(DataTypes, float)> dataReceived = BikeDataParser.ParseBikeData(e.Data);
            foreach ((DataTypes, float) dataItem in dataReceived)
            {
                DataReceived(dataItem);
            }
        }

        public void SetResistance(byte resistance)
        {
            bluetooth.SetBikeResistance(resistance);
        }

        public void Start(string bikeId = null)
        {
            // bikeId shouldn't be null, as handled before Start is called upon
            this.bikeId = bikeId;
            bluetooth.Start(this);
        }

        public void DataReceived((DataTypes, float) data)
        {
            services.GetService<DeviceManager>().HandleData(data);
        }
    }
}
