using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DoctorApplication
{
    public class Client : ObservableObject
    {
        public string clientAuthKey { get; set; }
        public string clientName { get; set; }
        public string clientSerial { get; set; }

        private string speed;
        public string Speed
        {
            get => speed;
            set => SetProperty(ref speed, value);
        }

        private string time;
        public string Time
        {
            get => time; 
            set => SetProperty(ref time,value);
        }

        private string distanceTraveled;
        public string DistanceTraveled
        {
            get => distanceTraveled; 
            set => SetProperty(ref distanceTraveled,value);
        }

        private string rpm;

        public string Rpm
        {    
            get => rpm; 
            set => SetProperty(ref rpm, value);
        }

        private string heartrate;
        public string Heartrate
        {
            get => heartrate; 
            set => SetProperty(ref heartrate,value);
        }
    }
}
