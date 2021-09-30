using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteHealthcare.bike
{
    public class RealBike : IBike
    {
        private readonly Func<(int, float)> callback;

        public RealBike(Func<(int,float)> callback)
        {
            this.callback = callback;
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
