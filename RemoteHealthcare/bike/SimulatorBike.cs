using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteHealthcare.bike
{
    public class SimulatorBike : IBike
    {
        public SimulatorBike(IServiceProvider serviceProvider)
        {
            
        }

        public void SetResistance(int resistance)
        {
            throw new NotImplementedException();
        }

        public void Start(string bikeId = null)
        {
            throw new NotImplementedException();
        }
    }
}
