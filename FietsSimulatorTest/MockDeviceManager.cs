using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteHealthcare.Bike;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteHealthcare.Tests
{
    class MockDeviceManager : IDeviceManager
    {
        private readonly IServiceProvider services;
        public CheckedDataTypes CheckedTypes;

        public MockDeviceManager()
        {
            services = this.BuildServiceProvider();
            CheckedTypes = CheckedDataTypes.NONE;
        }

        event Action<Dictionary<DataTypes, float>> IDeviceManager.HandelDataEvents
        {
            add
            {
                // Not Implemented 
            }

            remove
            {
                // Not Implemented 
            }
        }

        public void Start((IBikeManager.BikeType, string) bikeTypeAndId)
        {
            this.services.GetService<IBikeManager>().Start(IBikeManager.BikeType.SIMULATOR_BIKE);

            Thread.Sleep(100);

            var bikeManager = services.GetService<IBikeManager>() as BikeManager;
            Assert.IsTrue(bikeManager.SimIsRunning());

            bikeManager.SimSetRunning(false);
            Thread.Sleep(1000);

            Assert.IsFalse(bikeManager.SimIsRunning());
            Assert.IsFalse(bikeManager.GetSimThread().IsAlive);
        }

        [Flags]
        public enum CheckedDataTypes : short
        {
            NONE = 0,
            BIKE_SPEED = 1,
            BIKE_ELAPSED_TIME = 2,
            BIKE_DISTANCE = 4,
            BIKE_RPM = 8,
        }

        public void Start()
        {
            
        }

        public IBikeManager.BikeType bikeType { get; set; }
        public string bikeID { get; set; }

        public void HandleData(Dictionary<DataTypes, float> data)
        {
            foreach (var item in data)
            {
                switch (item.Key)
                {
                    case DataTypes.BIKE_SPEED:
                        CheckedTypes |= CheckedDataTypes.BIKE_SPEED;
                        Assert.IsTrue(item.Value >= 0);
                        break;
                    case DataTypes.BIKE_ELAPSED_TIME:
                        CheckedTypes |= CheckedDataTypes.BIKE_ELAPSED_TIME;
                        Assert.IsTrue(item.Value >= 0);
                        break;
                    case DataTypes.BIKE_DISTANCE:
                        CheckedTypes |= CheckedDataTypes.BIKE_DISTANCE;
                        Assert.IsTrue(item.Value >= 0);
                        break;
                    case DataTypes.BIKE_RPM:
                        CheckedTypes |= CheckedDataTypes.BIKE_RPM;
                        Assert.IsTrue(item.Value >= 0);
                        break;
                }
            }
        }

        public event Action<(DataTypes, float)> HandelDataEvents;
        public void StartTraining()
        {
            // Not Implemented 
        }

        public void StopTraining()
        {
            // Not Implemented 
        }

        private IServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton(new Bluetooth(BLEInstance.BIKE))
                .AddSingleton(new Bluetooth(BLEInstance.HEARTRATE))
                .AddSingleton<IBikeManager, BikeManager>()
                .AddSingleton<IDeviceManager>(this)
                .BuildServiceProvider();
        }

        Task IDeviceManager.StartTraining()
        {
            // Not Implemented 
            return Task.Delay(2);
        }
    }
}
