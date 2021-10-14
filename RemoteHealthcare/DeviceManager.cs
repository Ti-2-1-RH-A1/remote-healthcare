using Microsoft.Extensions.DependencyInjection;
using RemoteHealthcare.Bike;
using RemoteHealthcare.Hrm;
using RemoteHealthcare.ServerCom;
using RemoteHealthcare.VR;
using System;

namespace RemoteHealthcare
{
    public class DeviceManager : IDeviceManager
    {
        private readonly IServiceProvider services;
        public event Action<(DataTypes, float)> HandelDataEvents;

        public DeviceManager()
        {
            services = BuildServiceProvider();
        }

        public void Start((IBikeManager.BikeType, string) bikeTypeAndId)
        {
            ComManager comManager = services.GetService<ComManager>();
            comManager.Start();
            var bikeManager = services.GetService<BikeManager>();
            bikeManager.StartBike(bikeTypeAndId.Item1, bikeTypeAndId.Item2);
            var hrmManager = services.GetService<HRMManager>();
            hrmManager.StartHRM();
            var vrManager = services.GetService<VRManager>();
            vrManager.Start();
        }

        public void HandleData((DataTypes, float) data)
        {
            HandelDataEvents?.Invoke(data);
        }

        private IServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton(new Bluetooth(BLEInstance.BIKE))
                .AddSingleton(new Bluetooth(BLEInstance.HEARTRATE))
                .AddSingleton<ComManager>()
                .AddSingleton<BikeManager>()
                .AddSingleton<HRMManager>()
                .AddSingleton<VRManager>()
                .AddSingleton<IDeviceManager>(this)
                .BuildServiceProvider();
        }
    }
}
