using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static RemoteHealthcare.Bike.BikeManager;

namespace RemoteHealthcare.Bike
{
    public interface IDeviceManager
    {
        public void Start();
        public IBikeManager.BikeType bikeType { get; set; }
        public string bikeID { get; set; }

        public void HandleData(Dictionary<DataTypes, float> data);
        public event Action<Dictionary<DataTypes, float>> HandelDataEvents;
        public Task StartTraining();
        public void StopTraining();
    }

    public interface IBikeManager
    {
        public Task Start(IBikeManager.BikeType bikeType = BikeType.SIMULATOR_BIKE, string bikeId = null);
        public void Stop();
        public void SetResistance(int resistance);
        public enum BikeType
        {
            REAL_BIKE,
            SIMULATOR_BIKE
        }
    }

    public interface IVRManager
    {
        public void Start();
        public void HandleData(Dictionary<DataTypes, float> data);
        public void HandleDoctorMessage(string message);
        public void Stop();
    }

    public interface IHRMManager
    {
        public void DataReceived(Dictionary<DataTypes, float> data);
        public Task Start();
    }

    public interface IComManager
    {
        public void Start();
    }
}
