using System;
using System.Diagnostics;
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

        /// <summary>
        /// This method starts the simulator for the bike. It starts a new thread that runs a continuous loop simulating
        /// all data sets that are needed.
        /// The simulator can be stopped by calling the the <see cref="Stop"/> method.
        /// </summary>
        /// <param name="bikeId">Is not needed and can be left at the default value for this implementation.</param>
        public void Start(string bikeId = null)
        {
            this.isRunning = true;
            Thread simThread = new Thread(new ThreadStart(this.RunSimulation));
            simThread.Start();
        }

        public void Stop()
        {
            this.isRunning = false;
        }

        private void RunSimulation()
        {
            // Simulate elapsed time.
            Stopwatch stopwatch = Stopwatch.StartNew();
            long prevMilis = stopwatch.ElapsedMilliseconds;
            float totalDistanceTravled = 0;

            while(this.isRunning)
            {
                float speed = this.GenerateSpeed(stopwatch.ElapsedMilliseconds);
                GenerateDistanceTravled(stopwatch.ElapsedMilliseconds, prevMilis, ref totalDistanceTravled, speed);
                float rpm = GenerateRPM(speed);

                this.DataReceived((DataTypes.BIKE_SPEED, speed));
                this.DataReceived((DataTypes.BIKE_DISTANCE, totalDistanceTravled));
                this.DataReceived((DataTypes.BIKE_RPM, rpm));
                this.DataReceived((DataTypes.BIKE_ELAPSED_TIME, stopwatch.ElapsedMilliseconds / 1000));

                prevMilis = stopwatch.ElapsedMilliseconds;
                Thread.Sleep(50); // Let the simulator wait until simulating the next dataset.
            }
        }
        
        public void DataReceived((DataTypes, float) data)
        {
            services.GetService<DeviceManager>().HandleData(data);
        }

        public void SetResistance(byte resistance)
        {
            this.resistance = resistance / byte.MaxValue * 100;
        }

        private float GenerateSpeed(long elapsedMilis)
        {
            float convertedResistance = 1 / (this.resistance + 1);
            return (float)(5 + 5 * Math.Sin(elapsedMilis)) * convertedResistance;
        }

        private void GenerateDistanceTravled(long elapsedMilis, long prevMilis, ref float totalDistanceTravled, float speed)
        {
            totalDistanceTravled += (elapsedMilis - prevMilis) / 1000 * speed;
        }

        private float GenerateRPM(float speed)
        {
            return speed * 10;
        }
    }
}
