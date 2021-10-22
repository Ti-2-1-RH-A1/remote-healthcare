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
            services.GetService<ComManager>().Start();
            services.GetService<IBikeManager>().Start(bikeTypeAndId.Item1, bikeTypeAndId.Item2);
            services.GetService<IHRMManager>().Start();
            services.GetService<IVRManager>().Start();
        }

        public void HandleData((DataTypes, float) data)
        {
            HandelDataEvents?.Invoke(data);
        }

        private IServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton<Bluetooth>(new Bluetooth(BLEInstance.BIKE))
                .AddSingleton<Bluetooth>(new Bluetooth(BLEInstance.HEARTRATE))
                .AddSingleton<IComManager, ComManager>()
                .AddSingleton<IBikeManager, BikeManager>()
                .AddSingleton<IHRMManager, HRMManager>()
                .AddSingleton<IVRManager, VRManager>()
                .AddSingleton<IDeviceManager>(this)
                .BuildServiceProvider();
        }
    }
}
