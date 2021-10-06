using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using RemoteHealthcare.bike;

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

        public void Start(BikeManager.BikeType bikeType, string bikeId)
        {
            var bikeManager = services.GetService<BikeManager>();
            bikeManager.StartBike(bikeType);
            
        }

        public void HandleData((int,float) data)
        {
            // TODO [Martijn] Implementation
        }


        private IServiceProvider buildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton(new Bluetooth(BLEInstance.BIKE))
                .AddSingleton(new Bluetooth(BLEInstance.HEARTRATE))
                .AddSingleton<BikeManager>()
                .AddSingleton(this)
                .BuildServiceProvider();
        }
    }
}
