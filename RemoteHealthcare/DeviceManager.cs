using Microsoft.Extensions.DependencyInjection;
using RemoteHealthcare.bike;
using RemoteHealthcare.hrm;
using RemoteHealthcare.vr;
using System;

namespace RemoteHealthcare
{
    public class DeviceManager : IDeviceManager
    {
        private IServiceProvider services;

        public DeviceManager()
        {
            this.services = BuildServiceProvider();
        }

        public void Start((IBikeManager.BikeType, string) bikeTypeAndId)
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
            if (data == null) { return; }

            // Rest of the handel code.
        }

        private IServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton(new Bluetooth(BLEInstance.BIKE))
                .AddSingleton(new Bluetooth(BLEInstance.HEARTRATE))
                .AddSingleton<BikeManager>()
                .AddSingleton<HRMManager>()
                .AddSingleton<VRManager>()
                .AddSingleton<IDeviceManager>(this)
                .BuildServiceProvider();
        }
    }
}
