using System;
using System.Threading;

namespace RemoteHealthcare
{
    class Simulator
    {

        static void Main(string[] args)
        {
            // Display the number of command line arguments.
            Console.WriteLine(args.Length);
            Simulator simulator = new Simulator();
        }

        public Simulator()
        {
            Thread t = new Thread(new ThreadStart(Run));
            t.Start();
            Console.ReadLine();

        }
        public void Run()
        {
            Random random = new Random();
            int i = 0;
            while (true)
            {
               

                FakeBike fakeBike = new FakeBike();
                fakeBike.Data = GenerateSpeedData(i);
                Program.BleBike_SubscriptionValueChanged(fakeBike);

                Thread.Sleep(1000);

                i++;
            }
        }

        private static byte[] GenerateSpeedData(int i)
        {
            byte[] data = generateAPage(0x10);
           
            double speed = 40 * (Math.Sin(i * 0.1) + 1) / 2;
            short speedcalc = (short)(speed * 1000 * (1 / 3.6));

            byte[] bytes = BitConverter.GetBytes(speedcalc);

            data[8] = bytes[0];
            data[9] = bytes[1];
            
            //A4 09 4E 05 10 19 61 54 00 00 FF 24 01

            return data;
        }

        public static byte[] generateAPage(byte pagenumber)
        {
            byte[] data = new byte[13];
            data[0] = 0xA4;
            data[1] = 0x09;
            data[2] = 0x4E;
            data[3] = 0x05;
            data[4] = pagenumber; //page number
            data[5] = 0x19;
            data[6] = 0x00;
            data[7] = 0x00;
            data[8] = 0x00;
            data[9] = 0x00;
            data[10] = 0xFF;
            data[11] = 0x24;
            data[12] = 0x01;

            return data;
        }



    }

    class FakeBike : IBike
    {
        public byte[] Data { get; set; }
        public string ServiceName { get; set; }
    }

}
