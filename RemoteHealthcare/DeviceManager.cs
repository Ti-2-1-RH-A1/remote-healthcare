using Microsoft.Extensions.DependencyInjection;
using System;
using RemoteHealthcare.bike;
using RemoteHealthcare.vr;
using RemoteHealthcare.hrm;

namespace RemoteHealthcare
{
    public class DeviceManager
    {
        private readonly IServiceProvider services;

        public DeviceManager()
        {
            this.services = BuildServiceProvider();
        }

        public void Start((BikeManager.BikeType, string) bikeTypeAndId)
        {
            services.GetService<BikeManager>().StartBike(bikeTypeAndId.Item1, bikeTypeAndId.Item2);
            services.GetService<HRMManager>().StartHRM();
            /*services.GetService<VRManager>().Start();*/
        }


        public void HandleData((DataTypes, float)? data)
        {
            // TODO [Martijn] Implementation
            if (data != null)
            {
                // Send data to vr and to the doctor client here
                Console.WriteLine(data);
                // TODO [Martijn] Test if HRM data also works
            }
        }

        private IServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton(new Bluetooth(BLEInstance.BIKE))
                .AddSingleton(new Bluetooth(BLEInstance.HEARTRATE))
                .AddSingleton<BikeManager>()
                .AddSingleton<HRMManager>()
                .AddSingleton<VRManager>()
                .AddSingleton(this)
                .BuildServiceProvider();
        }
    }
}
