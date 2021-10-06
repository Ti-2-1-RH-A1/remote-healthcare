using System;

namespace RemoteHealthcare.bike
{
    class BikeManager
    {
        private readonly IBike realBike;        // Instance of the real physical bike.
        private readonly IBike simulatorBike;   // Instance of the simulated bike run by the program.
        private IBike activeBike;               // Instance of the bike currently being used. I.E. the real- or simulated bike.

        private readonly IServiceProvider services;

        public BikeManager(IServiceProvider serviceProvider)
        {
            this.services = serviceProvider;

            this.realBike = new RealBike(serviceProvider);
            this.simulatorBike = new SimulatorBike(serviceProvider);
        }

        public enum BikeType
        {
            REAL_BIKE,
            SIMULATOR_BIKE
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
        public void StartBike(BikeType bikeType, string bikeId = null)
        {
            if (bikeType == BikeType.REAL_BIKE && bikeId == null)
            {
                throw new ArgumentNullException(bikeId, "[BikeManager.StartBike()] Param bikeId should not be null when bikeType is BikeType.REAL_BIKE.");
            }

            if (bikeType == BikeType.REAL_BIKE)
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
    }
}
