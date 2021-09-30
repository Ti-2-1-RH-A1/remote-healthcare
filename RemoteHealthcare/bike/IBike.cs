using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteHealthcare.bike
{
    interface IBike
    {
        public void Start();
        public void SetResistance(int resistance);
    }
}
