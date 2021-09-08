using System;
using System.Collections.Generic;
using System.Text;
using Avans.TI.BLE;

namespace RemoteHealthcare
{
    class RealBike : BLE, IBike
    {
        public void SetAirResistance(byte Value)
        {
            throw new NotImplementedException();
        }

        public void SetResistance(byte Value)
        {
            throw new NotImplementedException();
        }
    }
}
