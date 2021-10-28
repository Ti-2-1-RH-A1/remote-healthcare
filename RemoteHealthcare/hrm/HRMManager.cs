using Avans.TI.BLE;
using Microsoft.Extensions.DependencyInjection;
using RemoteHealthcare.Bike;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteHealthcare.Hrm
{
    class HRMManager : IHRMManager
    {
        public const string hrmTypeName = "Decathlon Dual HR";
        public const string heartRateServiceName = "HeartRate";
        public const string heartSubscribtionCharacteristic = "HeartRateMeasurement";

        private readonly IServiceProvider services;
        private readonly Bluetooth bluetooth;

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

        public void Start()
        {
            bluetooth.Start(hrmTypeName, heartRateServiceName, heartSubscribtionCharacteristic);
        }
    }
}
