using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteHealthcare
{
    public interface IBike
    {
        public void SetResistance(byte resistance);
        public void SetAirResistance(byte airResistanceCoefficient, byte windspeed, byte draftingFactor);
    }
}
