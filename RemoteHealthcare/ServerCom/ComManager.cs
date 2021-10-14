using Microsoft.Extensions.DependencyInjection;
using RemoteHealthcare.Bike;
using System;

namespace RemoteHealthcare.ServerCom
{
    class ComManager : IComManager
    {
        private readonly NetClient netClient;
        private readonly IServiceProvider services;

        public ComManager(IServiceProvider services)
        {
            this.services = services;
            netClient = new NetClient();
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
                    netClient.SendData("speed", data.Item2);
                    break;
                case DataTypes.BIKE_ELAPSED_TIME:
                    netClient.SendData("time", data.Item2);
                    break;
                case DataTypes.BIKE_DISTANCE:
                    netClient.SendData("distance_traveled", data.Item2);
                    break;
                case DataTypes.BIKE_RPM:
                    netClient.SendData("rpm", data.Item2);
                    break;
                case DataTypes.HRM_HEARTRATE:
                    netClient.SendData("heartrate", data.Item2);
                    break;
            }
        }
    }
}
