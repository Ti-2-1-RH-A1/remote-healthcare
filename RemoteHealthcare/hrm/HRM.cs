using Avans.TI.BLE;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace RemoteHealthcare.Hrm
{
    public class HRM
    {
        public const string hrmTypeName = "Decathlon Dual HR";
        public const string heartRateServiceName = "HeartRate";
        public const string heartSubscribtionCharacteristic = "HeartRateMeasurement";

        private readonly IServiceProvider services;
        private readonly Bluetooth bluetooth;

        public HRM(IServiceProvider services)
        {
            this.services = services;

            this.bluetooth = services.GetServices<Bluetooth>().Where(b => b.BLEInstance == BLEInstance.BIKE).FirstOrDefault();
            bluetooth.DataReceived += Ble_DataReceived;
        }

        private void Ble_DataReceived(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            DataReceived(HRMDataParser.ParseHRMData(e.Data));
        }

        public void DataReceived((DataTypes, float) data)
        {
            services.GetService<DeviceManager>().HandleData(data);
        }

        public void Start()
        {
            bluetooth.Start(this);
        }
    }
}
