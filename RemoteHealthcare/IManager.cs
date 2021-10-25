﻿using System;
using System.Collections.Generic;
using System.Text;
using static RemoteHealthcare.Bike.BikeManager;

namespace RemoteHealthcare.Bike
{
    public interface IDeviceManager
    {
        public void Start();
        public IBikeManager.BikeType bikeType { get; set; }
        public string bikeID { get; set; }

        public void HandleData((DataTypes, float) data);
        public event Action<(DataTypes, float)> HandelDataEvents;
        public void StartTraining();
        public void StopTraining();
    }

    public interface IBikeManager
    {
        public void Start(IBikeManager.BikeType bikeType = BikeType.SIMULATOR_BIKE, string bikeId = null);
        public void Stop();
        public enum BikeType
        {
            REAL_BIKE,
            SIMULATOR_BIKE
        }
    }

    public interface IVRManager
    {
        public void Start();
        public void HandleData((DataTypes, float) data);
        public void Stop();
    }

    public interface IHRMManager
    {
        public void DataReceived((DataTypes, float) data);
        public void Start();
    }

    public interface IComManager
    {
        public void Start();
    }
}
