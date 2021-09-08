using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteHealthcare
{
    public interface IBike
    {
        public void SetResistance(byte Value);
        public void SetAirResistance(byte Value);
    }
}
