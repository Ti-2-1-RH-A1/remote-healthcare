using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteHealthcare.bike
{
    public class SimulatorBike : IBike
    {
        private readonly IServiceProvider services;
        private bool isRunning = false;

        public SimulatorBike(IServiceProvider serviceProvider)
        {
            this.services = serviceProvider;
        }

        public void Start(string bikeId = null)
        {
            // TODO Implementation
        }

        private void RunSimulation()
        {
            while(this.isRunning)
            {

            }
        }
        
        public void DataReceived((DataTypes, float) data)
        {
            services.GetService<DeviceManager>().HandleData(data);
        }

        public void SetResistance(byte resistance)
        {
            throw new NotImplementedException();
        }
    }
}
