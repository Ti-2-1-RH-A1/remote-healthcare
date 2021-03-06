using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteHealthcare.Bike
{
    public class SimulatorBike : IBike
    {
        private readonly IServiceProvider services;

        private float resistance = 40;
        private bool isRunning;
        private Thread simThread;

        public SimulatorBike(IServiceProvider serviceProvider)
        {
            this.isRunning = false;
            this.services = serviceProvider;
        }

        /// <summary>
        /// The destructor makes sure the simTread is aborted if the SimulatorBike instance is deleted.
        /// </summary>
        ~SimulatorBike()
        {
            if (this.simThread.IsAlive) { this.simThread.Abort(); }
        }

        /// <summary>
        /// This method starts the simulator for the bike. It starts a new thread that runs a continuous loop simulating
        /// all data sets that are needed.
        /// The simulator can be stopped by calling the the <see cref="Stop"/> method.
        /// </summary>
        /// <param name="bikeId">Is not needed and can be left at the default value for this implementation.</param>
        public async Task Start(string bikeId = null)
        {
            this.isRunning = true;
            this.simThread = new Thread(new ThreadStart(this.RunSimulation));
            this.simThread.Name = "simThread";
            this.simThread.Start();
        }

        /// <summary>
        /// When this method is called the boolean to keep running the simulator is set to false.
        /// This does not mean the simulator will stop immediately, if it is halfway through a simulation
        /// it will first finish that loop before ending.
        /// </summary>
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

            // Main simulation loop.
            while (this.isRunning)
            {
                float speed = this.GenerateSpeed(stopwatch.ElapsedMilliseconds);
                GenerateDistanceTravled(stopwatch.ElapsedMilliseconds, prevMilis, ref totalDistanceTravled, speed);
                float rpm = GenerateRPM(speed);

                Dictionary<DataTypes, float> bikeDataRecieved = new Dictionary<DataTypes, float>();

                bikeDataRecieved.Add(DataTypes.BIKE_SPEED, speed);
                bikeDataRecieved.Add(DataTypes.BIKE_DISTANCE, totalDistanceTravled);
                bikeDataRecieved.Add(DataTypes.BIKE_RPM, rpm);
                bikeDataRecieved.Add(DataTypes.BIKE_ELAPSED_TIME, stopwatch.ElapsedMilliseconds / 1000);
                DataReceived(bikeDataRecieved);

                prevMilis = stopwatch.ElapsedMilliseconds;
                Thread.Sleep(500); // Let the simulator wait until simulating the next dataset.
            }
        }

        public void DataReceived(Dictionary<DataTypes, float> data)
        {
            services.GetService<IDeviceManager>()?.HandleData(data);
        }

        public void SetResistance(byte resistance)
        {
            this.resistance = resistance / byte.MaxValue * 100;
        }

        private float GenerateSpeed(long elapsedMilis)
        {
            return (float)(6 + 5 * Math.Sin(elapsedMilis * 0.00008f)) * ((100 - this.resistance) / 100);
        }

        private void GenerateDistanceTravled(long elapsedMilis, long prevMilis, ref float totalDistanceTravled, float speed)
        {
            totalDistanceTravled += (elapsedMilis - prevMilis) / 1000f * speed;
        }

        private float GenerateRPM(float speed)
        {
            return speed * 10;
        }

        public bool IsRunning() => this.isRunning;
        public void SetRunning(bool whyAreYouRunning) => this.isRunning = whyAreYouRunning;

        public Thread GetSimThread() => this.simThread;
    }
}
