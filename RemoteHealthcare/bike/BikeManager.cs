using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteHealthcare.bike
{
    class BikeManager
    {
        private IBike realBike;
        private IBike simulatorBike;
        private IBike activeBike;

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
                this.activeBike = realBike;
                realBike.Start();
            }
        }
    }
}
