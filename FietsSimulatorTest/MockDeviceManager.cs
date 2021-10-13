using Microsoft.Extensions.DependencyInjection;
using RemoteHealthcare;
using RemoteHealthcare.bike;
using System;
using System.Collections.Generic;
using System.Text;

namespace FietsSimulatorTest
{
    class MockDeviceManager : IDeviceManager
    {
        private IServiceProvider services;

        public MockDeviceManager()
        {
            this.BuildServiceProvider();
        }

        public void Start((IBikeManager.BikeType, string) bikeTypeAndId)
        {
            this.services.GetService<IBikeManager>().StartBike(IBikeManager.BikeType.SIMULATOR_BIKE);
        }
        
        public void HandleData((DataTypes, float)? data)
        {
            throw new NotImplementedException();
        }

        private IServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton<IBikeManager>()
                .AddSingleton<IDeviceManager>(this)
                .BuildServiceProvider();
        }
    }
}
