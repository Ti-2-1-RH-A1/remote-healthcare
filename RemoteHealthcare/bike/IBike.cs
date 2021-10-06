using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteHealthcare.bike
{
    interface IBike
    {
        public void Start(string bikeId = null);
        public void SetResistance(int resistance);
        public void DataReceived((DataTypes, float) data);
    }
}
