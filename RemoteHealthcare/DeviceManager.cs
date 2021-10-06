using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RemoteHealthcare
{

    public 
    class DeviceManager
    {
        private IServiceProvider services;

        public DeviceManager()
        {
            this.services = buildServiceProvider();
        }

        public async Task Start()
        {
        }

        private void Ble_DataReceived(object sender, Avans.TI.BLE.BLESubscriptionValueChangedEventArgs e)
        {
            Console.WriteLine($"Received: {e.Data}");
        }

        public void HandleData((int,float) data)
        {

        }


        private IServiceProvider buildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton<Bluetooth>()
                .AddSingleton<BikeManager>()
                .AddSingleton(this)
                .BuildServiceProvider();
        }
    }
}
