using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteHealthcare.bike
{
    public class SimulatorBike : IBike
    {
        private readonly IServiceProvider services;

        private bool isRunning = false;
        private float resistance = 40;

        public SimulatorBike(IServiceProvider serviceProvider)
        {
            this.services = serviceProvider;
        }

        public void Start(string bikeId = null)
        {
            this.isRunning = true;
            Thread simThread = new Thread(new ThreadStart(this.RunSimulation));
            simThread.Start();
        }

        private void RunSimulation()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            while(this.isRunning)
            {
                // Simulate bike speed.
                this.DataReceived((DataTypes.BIKE_SPEED, this.GenerateSpeed(stopwatch.ElapsedMilliseconds)));

                Thread.Sleep(50); // Let the simulator wait until simulating the next dataset.
            }
        }

        private float GenerateSpeed(long elapsedMilis)
        {
            float convertedResistance = 1 / (this.resistance + 1);
            return (float)(5 + 5 * Math.Sin(elapsedMilis)) * convertedResistance;
        }
        
        public void DataReceived((DataTypes, float) data)
        {
            services.GetService<DeviceManager>().HandleData(data);
        }

        public void SetResistance(byte resistance)
        {
            this.resistance = resistance / byte.MaxValue * 100;
        }
    }
}
