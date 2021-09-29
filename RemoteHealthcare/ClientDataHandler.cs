using System;
using ServerClient;

namespace RemoteHealthcare
{
    public class ClientDataHandler
    {
        public BikeManager bikeManager;
        public HRManager hrManager;
        public Client client;

        public ClientDataHandler(BikeManager bikeManager, HRManager hrManager)
        {
            this.bikeManager = bikeManager;
            this.hrManager = hrManager;
            bikeManager.sendData = sendBikeData;
            this.client = new Client("fiets");
        }

        public void sendBikeData(BikeDataThing bikeData)
        {
            Console.WriteLine(bikeData.ToString());
            var bikeDataDistance = bikeData.distance;
            var bikeDataSpeed = bikeData.speed;
            var bikeDataTime = bikeData.time;
            //client.SendPacket();

        }


    }


    public class BikeDataThing
    {
        public BikeDataThing(float time, int distance, float speed)
        {
            this.time = time;
            this.distance = distance;
            this.speed = speed;
        }
        public float speed { get; set; }
        public int distance { get; set; }
        public float time { get; set; }
    }


}