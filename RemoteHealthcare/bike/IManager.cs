﻿using System;
using System.Collections.Generic;
using System.Text;
using static RemoteHealthcare.bike.BikeManager;

namespace RemoteHealthcare.bike
{
    public interface IDeviceManager
    {
        public void Start((IBikeManager.BikeType, string) bikeTypeAndId);
        public void HandleData((DataTypes, float)? data);
    }

    public interface IBikeManager
    {
        public void StartBike(BikeType bikeType, string bikeId = null);
        public enum BikeType
        {
            REAL_BIKE,
            SIMULATOR_BIKE
        }
    }
}
