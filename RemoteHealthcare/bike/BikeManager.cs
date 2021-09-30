using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteHealthcare.bike
{
    class BikeManager
    {
        private readonly IBike realBike;        // Instance of the real physical bike.
        private readonly IBike simulatorBike;   // Instance of the simulated bike run by the program.
        private IBike activeBike;               // Instance of the bike currently being used. I.E. the real- or simulated bike.

        public BikeManager(Func<(int,float)> callback)
        {
            this.realBike = new RealBike(callback);
            this.simulatorBike = new SimulatorBike(callback);
        }

        public enum BikeType
        {
            REAL_BIKE,
            SIMULATOR_BIKE
        }

        public void StartBike(BikeType biketype, string bikeId = null)
        {
            if (biketype == BikeType.REAL_BIKE)
            {
                this.activeBike = this.realBike;
                this.activeBike.Start(bikeId);
                return;
            }

            this.activeBike = this.simulatorBike;
            this.activeBike.Start();
        }
    }
}
