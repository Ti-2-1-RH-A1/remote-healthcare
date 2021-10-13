using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteHealthcare;
using RemoteHealthcare.bike;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FietsSimulatorTest
{
    class MockDeviceManager : IDeviceManager
    {
        private readonly IServiceProvider services;
        public CheckedDataTypes CheckedTypes;

        public MockDeviceManager()
        {
            this.services = this.BuildServiceProvider();
            this.CheckedTypes = CheckedDataTypes.NONE;
        }

        public void Start((IBikeManager.BikeType, string) bikeTypeAndId)
        {
            this.services.GetService<IBikeManager>().StartBike(IBikeManager.BikeType.SIMULATOR_BIKE);

            Thread.Sleep(100);

            var bikeManager = services.GetService<IBikeManager>() as BikeManager;
            Assert.IsTrue(bikeManager.SimIsRunning());

            bikeManager.SimSetRunning(false);
            Thread.Sleep(150);

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
        
        public void HandleData((DataTypes, float) data)
        {
            switch(data.Item1)
            {
                case DataTypes.BIKE_SPEED:
                    CheckedTypes |= CheckedDataTypes.BIKE_SPEED;
                    Assert.IsTrue(data.Item2 >= 0);
                    break;
                case DataTypes.BIKE_ELAPSED_TIME:
                    CheckedTypes |= CheckedDataTypes.BIKE_ELAPSED_TIME;
                    Assert.IsTrue(data.Item2 >= 0);
                    break;
                case DataTypes.BIKE_DISTANCE:
                    CheckedTypes |= CheckedDataTypes.BIKE_DISTANCE;
                    Assert.IsTrue(data.Item2 >= 0);
                    break;
                case DataTypes.BIKE_RPM:
                    CheckedTypes |= CheckedDataTypes.BIKE_RPM;
                    Assert.IsTrue(data.Item2 >= 0);
                    break;
            }
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
    }
}
