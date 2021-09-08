using System;
using System.Threading;
using System.Diagnostics;

namespace RemoteHealthcare
{
    public class Simulator
    {
        public double metersTraveled;

        public Simulator()
        {
            
        }

        public void startSim()
        {
            Run();
            return;
        }


        public void Run()
        {
            Boolean running = true;
            int count = 1;
            Stopwatch stopwatch = Stopwatch.StartNew();
            int i = 0;
            while (running)
            {
                RunStep(ref i, ref stopwatch);
                Thread.Sleep(1000);
                count++;
                if (count > 15)
                {
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.Write("Wil je verder gaan met de simulatie? (y/n)");

                        Console.BackgroundColor = ConsoleColor.Black;
                        if (Console.ReadLine()=="y")
                        {
                            count = 0;
                        }
                        else
                        {
                            running = false;
                        }
                }
            }
        }
        
        public void RunStep(ref int i, ref Stopwatch stopwatch)
        {              
            FakeBike fakeBike = new FakeBike();
            fakeBike.Data = GenerateSpeedData(i);
            BikeManager.BleBike_SubscriptionValueChanged(fakeBike);
            i++;
        }

        private long elapsedTime = 0;
        private byte[] GenerateSpeedData(int i, Stopwatch stopwatch)
        {
            byte[] data = generateAPage(0x10);
           
            double speed = 40 * (Math.Sin(i * 0.1) + 1) / 2;
            short speedcalc = (short)(speed * 1000 * (1 / 3.6));

            byte[] bytes = BitConverter.GetBytes(speedcalc);

            long stopwatchElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            data[6] = (byte)(stopwatchElapsedMilliseconds/ 250); // Elapsed Time
            long timeDifference = stopwatchElapsedMilliseconds - elapsedTime;
            elapsedTime = stopwatchElapsedMilliseconds;
            double timeDoubleDifference = (double)timeDifference/1000;
            double metersPerSecond = speed*(1/3.6);
            metersTraveled += timeDoubleDifference * metersPerSecond;
            data[7] = (byte) metersTraveled;

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
