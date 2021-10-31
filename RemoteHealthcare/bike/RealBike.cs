using Avans.TI.BLE;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteHealthcare.Bike
{
    public class RealBike : BLE, IBike
    {
        public const string bikeTypeName = "Tacx flux";
        public const string bikeServiceName = "6e40fec1-b5a3-f393-e0a9-e50e24dcca9e";
        public const string bikeSubscribtionCharacteristic = "6e40fec2-b5a3-f393-e0a9-e50e24dcca9e";
        public const string bikeSendingCharacteristic = "6e40fec3-b5a3-f393-e0a9-e50e24dcca9e";

        public string bikeId { get; set; }

        private readonly IServiceProvider services;
        private readonly Bluetooth bluetooth;

        private int BikeConnectionGood;

        public RealBike(IServiceProvider serviceProvider)
        {
            this.services = serviceProvider;
            this.bluetooth = services.GetServices<Bluetooth>().Where(b => b.BLEInstance == BLEInstance.BIKE).FirstOrDefault();
            bluetooth.DataReceived += Ble_DataReceived;
        }

        private void Ble_DataReceived(object sender, BLESubscriptionValueChangedEventArgs e)
        {

            
                Dictionary<DataTypes, float> dataReceived = BikeDataParser.ParseBikeData(e.Data);

                DataReceived(dataReceived);
            
        }

        public void Stop()
        {
            bluetooth.Dispose();
            this.Dispose();
        }

        public void SetResistance(byte resistance)
        {
            bluetooth.SetBikeResistance(resistance);
        }

        public async Task Start(string bikeId = null)
        {
            // bikeId shouldn't be null, as handled before Start is called upon
            this.bikeId = bikeId;
            this.BikeConnectionGood = await bluetooth.Start(bikeTypeName + " " + bikeId, bikeServiceName, bikeSubscribtionCharacteristic);
            Console.WriteLine(BikeConnectionGood);
            if (BikeConnectionGood != 0)
            {
                Console.WriteLine("Connectie naar fiets niet mogelijk sessie wordt gestopt.");
                services.GetService<DeviceManager>().StopTraining();
            }
        }

        public void DataReceived(Dictionary<DataTypes, float> data)
        {
            services.GetService<DeviceManager>().HandleData(data);
        }
    }
}
