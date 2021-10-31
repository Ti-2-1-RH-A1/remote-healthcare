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
        bool vrWorking = true;
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

            if (bikeType == IBikeManager.BikeType.REAL_BIKE)
            {
                Console.WriteLine("Wil je een HR meter gebruiken? [y|n]");
                string hrmChoice = Console.ReadLine().ToLower();
                if (hrmChoice.Contains("y"))
                {
                    await services.GetService<IHRMManager>().Start();
                }
            }

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
            Dictionary<DataTypes, float> roundedData = new Dictionary<DataTypes, float>();

            foreach (KeyValuePair<DataTypes, float> pair in data)
            {
                if (data.ContainsKey(DataTypes.HRM_HEARTRATE))
                {
                    roundedData.Add(DataTypes.HRM_HEARTRATE, (float) Math.Round(data[DataTypes.HRM_HEARTRATE]));
                }
            }
                foreach (KeyValuePair<DataTypes, float> pair in data)
                {
                    if (pair.Key == DataTypes.BIKE_SPEED) { continue; }
                    roundedData.Add(pair.Key, (float)Math.Round(pair.Value));
                }

                if (data.ContainsKey(DataTypes.BIKE_SPEED))
                {
                    roundedData.Add(DataTypes.BIKE_SPEED, (float) Math.Round(data[DataTypes.BIKE_SPEED] * 10) / 10);
                }
                
            

            HandelDataEvents?.Invoke(roundedData);
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
