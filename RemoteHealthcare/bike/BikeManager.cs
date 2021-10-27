using System;
using System.Threading;

namespace RemoteHealthcare.Bike
{
    public class BikeManager : IBikeManager
    {
        private readonly IBike realBike;        // Instance of the real physical bike.
        private readonly IBike simulatorBike;   // Instance of the simulated bike run by the program.
        private IBike activeBike;               // Instance of the bike currently being used. I.E. the real- or simulated bike.

        public BikeManager(IServiceProvider serviceProvider)
        {
            this.realBike = new RealBike(serviceProvider);
            this.simulatorBike = new SimulatorBike(serviceProvider);
        }

        /// <summary>
        /// This method starts the selected bike type. 
        /// <see cref="RealBike.Start(string)">For more information on the Realbike start method.</see> and 
        /// <see cref="SimulatorBike.Start(string)">for more information on the SimulatorBike start method.</see>
        /// </summary>
        /// <param name="bikeType">Specifies the type of bike that wil be started. Can be any BikeType.</param>
        /// <param name="bikeId">Is the id corresponding to the physical bike. This parameter should always
        ///                      be given if BikeType.REAL_BIKE is selected.</param>
        /// <exception cref="ArgumentNullException">Is thrown when the bikeType is set to REAL_BIKE but the 
        ///                                         bikeId is null or not given.</exception>
        public void Start(IBikeManager.BikeType bikeType = IBikeManager.BikeType.SIMULATOR_BIKE,string bikeId = null)  
        {
            if (bikeType == IBikeManager.BikeType.REAL_BIKE && bikeId == null)
            {
                throw new ArgumentNullException(bikeId, "[BikeManager.StartBike()] Param bikeId should not be null when bikeType is BikeType.REAL_BIKE.");
            }

            if (bikeType == IBikeManager.BikeType.REAL_BIKE)
            {
                this.activeBike = this.realBike;
                this.activeBike.Start(bikeId);
            }
            else // Simulator bike.
            {
                this.activeBike = this.simulatorBike;
                this.activeBike.Start();
            }
        }

        public void Stop()
        {
            activeBike.Stop();
        }

        public void SetResistance(int resistance)
        {
            this.activeBike.SetResistance((byte) ((resistance / 100) * byte.MaxValue));
        }

        public Thread GetSimThread() => (this.simulatorBike as SimulatorBike).GetSimThread();
        public bool SimIsRunning() => (this.simulatorBike as SimulatorBike).IsRunning();
        public void SimSetRunning(bool running) => (this.simulatorBike as SimulatorBike).SetRunning(running);
    }
}
