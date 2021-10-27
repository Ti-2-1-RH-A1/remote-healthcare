using Microsoft.Extensions.DependencyInjection;
using RemoteHealthcare.Bike;
using System;

namespace RemoteHealthcare.ServerCom
{
    class ComManager : IComManager
    {
        private readonly NetClient netClient;
        public readonly IServiceProvider services;

        public ComManager(IServiceProvider services)
        {
            this.services = services;
            netClient = new NetClient(this.services);
        }

        public void Start()
        {
            netClient.Start();
            services.GetService<IDeviceManager>().HandelDataEvents += HandleData;
        }

        private void HandleData((DataTypes, float) data)
        {
            switch (data.Item1)
            {
                case DataTypes.BIKE_SPEED:
                    netClient.SendPost("speed", data.Item2);
                    break;
                case DataTypes.BIKE_ELAPSED_TIME:
                    netClient.SendPost("time", data.Item2);
                    break;
                case DataTypes.BIKE_DISTANCE:
                    netClient.SendPost("distance_traveled", data.Item2);
                    break;
                case DataTypes.BIKE_RPM:
                    netClient.SendPost("rpm", data.Item2);
                    break;
                case DataTypes.HRM_HEARTRATE:
                    netClient.SendPost("heartrate", data.Item2);
                    break;
            }
        }
    }
}
