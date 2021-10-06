using Avans.TI.BLE;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace RemoteHealthcare.hrm
{
    class HRM
    {
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
            
        }

        public void Start()
        {

        }
    }
}
