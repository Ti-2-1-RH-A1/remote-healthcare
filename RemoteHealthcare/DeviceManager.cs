using Microsoft.Extensions.DependencyInjection;
using System;
using RemoteHealthcare.bike;
using RemoteHealthcare.vr;
using RemoteHealthcare.hrm;

namespace RemoteHealthcare
{
    public class DeviceManager
    {
        private IServiceProvider services;

        public DeviceManager()
        {
            this.services = buildServiceProvider();
        }

        public void Start((BikeManager.BikeType, string) bikeTypeAndId)
        {
            var bikeManager = services.GetService<BikeManager>();
            bikeManager.StartBike(bikeTypeAndId.Item1, bikeTypeAndId.Item2);
            var hrmManager = services.GetService<HRMManager>();
            hrmManager.StartHRM();
            var vrManager = services.GetService<VRManager>();
            vrManager.Start();
        }


        public void HandleData((DataTypes, float)? data)
        {
            // TODO [Martijn] Implementation
            if (data != null)
            {

            }
        }


        private IServiceProvider buildServiceProvider()
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
