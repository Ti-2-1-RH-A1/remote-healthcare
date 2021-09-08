using System;
using System.Threading;

namespace RemoteHealthcare
{
    public class Simulator
    {
        readonly Thread thread;
        static void Main(string[] args)
        {
            // Display the number of command line arguments.
            Console.WriteLine(args.Length);
            Simulator simulator = new Simulator();
        }

        public Simulator()
        {
            this.thread = new Thread(new ThreadStart(Run));
            this.thread.Start();
            Console.ReadLine();
        }
        public void Run()
        {
            int i = 0;
            while (true)
            {
                RunStep(ref i);
                Thread.Sleep(1000);
            }
        }
        
        public static void RunStep(ref int i)
        {
            byte[] data = new byte[13];
            data[0] = 0xA4;
            data[1] = 0x09;
            data[2] = 0x4E;
            data[3] = 0x05;
            data[4] = 0x10;
            data[5] = 0x19;
            data[6] = 0x61;
            data[7] = 0x54;
            i++;
            double speed = 40 * (Math.Sin(i * 0.1) + 1) / 2;
            short speedcalc = (short)(speed * 1000 * (1 / 3.6));

            byte[] bytes = BitConverter.GetBytes(speedcalc);

            data[8] = bytes[0];
            data[9] = bytes[1];
            data[10] = 0xFF;
            data[11] = 0x24;
            data[12] = 0x01;
            //A4 09 4E 05 10 19 61 54 00 00 FF 24 01

            FakeBike fakeBike = new FakeBike();
            fakeBike.Data = data;
            Program.BleBike_SubscriptionValueChanged(fakeBike);
        }
    }

    class FakeBike : IBike
    {
        public byte[] Data { get; set; }
        public string ServiceName { get; set; }
    }

}
