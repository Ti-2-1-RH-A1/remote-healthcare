using Microsoft.Extensions.DependencyInjection;
using RemoteHealthcare.Bike;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private void HandleData(Dictionary<DataTypes, float> data)
        {

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (var keyValuePair in data)
            {
                switch (keyValuePair.Key)
                {
                    case DataTypes.BIKE_SPEED:
                        dictionary.Add("speed", keyValuePair.Value.ToString());
                        break;
                    case DataTypes.BIKE_ELAPSED_TIME:
                        dictionary.Add("time", keyValuePair.Value.ToString());
                        break;
                    case DataTypes.BIKE_DISTANCE:
                        dictionary.Add("distance_traveled", keyValuePair.Value.ToString());
                        break;
                    case DataTypes.BIKE_RPM:
                        dictionary.Add("rpm", keyValuePair.Value.ToString());
                        break;
                    case DataTypes.HRM_HEARTRATE:
                        dictionary.Add("heartrate", keyValuePair.Value.ToString());
                        break;
                }
            }

            netClient.SendPost(dictionary);


            // switch (data.Item1)
            // {
            //     case DataTypes.BIKE_SPEED:
            //         netClient.SendPost("speed", data.Item2);
            //         break;
            //     case DataTypes.BIKE_ELAPSED_TIME:
            //         netClient.SendPost("time", data.Item2);
            //         break;
            //     case DataTypes.BIKE_DISTANCE:
            //         netClient.SendPost("distance_traveled", data.Item2);
            //         break;
            //     case DataTypes.BIKE_RPM:
            //         netClient.SendPost("rpm", data.Item2);
            //         break;
            //     case DataTypes.HRM_HEARTRATE:
            //         netClient.SendPost("heartrate", data.Item2);
            //         break;
            // }
        }
    }
}
