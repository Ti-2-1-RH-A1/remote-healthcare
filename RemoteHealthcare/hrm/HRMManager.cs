using Avans.TI.BLE;
using Microsoft.Extensions.DependencyInjection;
using RemoteHealthcare.Bike;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteHealthcare.Hrm
{
    class HRMManager : IHRMManager
    {
        public const string hrmTypeName = "Decathlon Dual HR";
        public const string heartRateServiceName = "HeartRate";
        public const string heartSubscribtionCharacteristic = "HeartRateMeasurement";

        private readonly IServiceProvider services;
        private readonly Bluetooth bluetooth;

        private int HRConnectionGood;

        public HRMManager(IServiceProvider services)
        {
            this.services = services;

            this.bluetooth = services.GetServices<Bluetooth>().Where(b => b.BLEInstance == BLEInstance.BIKE).FirstOrDefault();
            bluetooth.DataReceived += Ble_DataReceived;
        }

        private void Ble_DataReceived(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            DataReceived(HRMDataParser.ParseHRMData(e.Data));
        }

        public void DataReceived(Dictionary<DataTypes, float> data)
        {
            services.GetService<DeviceManager>().HandleData(data);
        }

        public async Task Start()
        {
            this.HRConnectionGood = await bluetooth.Start(hrmTypeName, heartRateServiceName, heartSubscribtionCharacteristic);
            if (HRConnectionGood != 0)
            {
                Console.WriteLine("Connectie naar HRM niet mogelijk sessie wordt zonder voortgezet.");
            }
        }
    }
}
