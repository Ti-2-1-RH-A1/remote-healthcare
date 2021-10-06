using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteHealthcare.bike
{
    public class SimulatorBike : IBike
    {
        private readonly IServiceProvider services;
        public SimulatorBike(IServiceProvider serviceProvider)
        {
            this.services = serviceProvider;
        }

        public void DataReceived((int, float) data)
        {
            services.GetService<DeviceManager>().HandleData(data);
        }

        public void SetResistance(int resistance)
        {
            throw new NotImplementedException();
        }

        public void Start(string bikeId = null)
        {
            // TODO Implementation
        }
    }
}
