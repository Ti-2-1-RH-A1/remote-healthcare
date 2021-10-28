using Microsoft.Extensions.DependencyInjection;
using RemoteHealthcare.Bike;
using RemoteHealthcare.Hrm;
using RemoteHealthcare.ServerCom;
using RemoteHealthcare.VR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteHealthcare
{
    public class DeviceManager : IDeviceManager
    {
        private readonly IServiceProvider services;
        public event Action<Dictionary<DataTypes, float>> HandelDataEvents;
        bool vrWorking = false;
        public IBikeManager.BikeType bikeType { get; set; }
        public string bikeID { get; set; }

        public DeviceManager()
        {
            services = BuildServiceProvider();
        }

        public void Start()
        {
            services.GetService<IComManager>().Start();
        }

        public async Task StartTraining()
        {
            await services.GetService<IBikeManager>().Start(bikeType, bikeID);
            
            
            if (vrWorking)
            {
                services.GetService<IVRManager>().Start();
            }
            
            await services.GetService<IHRMManager>().Start();
        }

        public void StopTraining()
        {
            if (vrWorking)
            {
                services.GetService<IVRManager>().Stop();
            }
            services.GetService<IBikeManager>().Stop();
        }


        public void HandleData(Dictionary<DataTypes, float> data)
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
