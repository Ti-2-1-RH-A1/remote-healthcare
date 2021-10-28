using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DoctorApplication
{
    public class Client : ObservableObject
    {
        public string clientAuthKey { get; set; }
        public string clientName { get; set; }
        public string clientSerial { get; set; }

        public ClientData ClientData = new ClientData();


        public string speed
        {
            get => ClientData.speed;
            set => SetProperty(ref ClientData.Speed, value);
        }


        public string time
        {
            get => ClientData.time;
            set => SetProperty(ref ClientData.Time, value);
        }


        public string distanceTraveled
        {
            get => ClientData.distanceTraveled;
            set => SetProperty(ref ClientData.DistanceTraveled, value);
        }


        public string rpm
        {
            get => ClientData.rpm;
            set => SetProperty(ref ClientData.Rpm, value);
        }


        public string heartRate
        {
            get => ClientData.heartRate;
            set => SetProperty(ref ClientData.HeartRate, value);
        }
    }

    public struct ClientData
    {
        public ClientData(string speed, string time, string distanceTraveled, string rpm, string heartRate)
        {
            Speed = speed;
            Time = time;
            DistanceTraveled = distanceTraveled;
            Rpm = rpm;
            HeartRate = heartRate;
        }

        internal string Rpm;
        internal string Time;
        internal string DistanceTraveled;
        internal string Speed;
        internal string HeartRate;

        public string rpm => Rpm;
        public string time => Time;
        public string distanceTraveled => DistanceTraveled;
        public string speed => Speed;
        public string heartRate => HeartRate;
    }
}