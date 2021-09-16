using System;
using System.Diagnostics;
using System.Threading;
using avansBikeData = Avans.TI.BLE.BLESubscriptionValueChangedEventArgs;

namespace RemoteHealthcare
{
    public class SimulatorBike : IBike
    {
        public double metersTraveled;
        private byte resistance;

        public void SetResistance(byte resistance)
        {
            this.resistance = resistance;
        }

        public void SetAirResistance(byte airResistanceCoefficient, byte windspeed, byte draftingFactor)
        {
            double windResistance = (draftingFactor * windspeed * airResistanceCoefficient);

            // If windresistance is 0 then total resistance is 0 as well.
            // If windresistance > 0 then determine how much windresistance there is compared to the max possible (65.62).
            // Which is determined from max wind speed * max wind resistance coefficient * max drafting factor = 127 * 1.86 * 1
            // This then gives a value of 0 to 1, which is multiplied by 256 to get a hex value of 0x00 to 0xFF,
            // which is used for the total resistance.
            this.resistance = windResistance <= 0 ? (byte)0 : (byte)(65.62 / windResistance * 256);
        }

        public void StartSim()
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
                    if (Console.ReadKey().KeyChar.ToString() == "y")
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
            avansBikeData bikeData = new avansBikeData();
            bikeData.Data = GenerateSpeedData(i, stopwatch);
            Bluetooth.BleBike_SubscriptionValueChanged(bikeData);
            i++;
        }

        private long elapsedTime = 0;
        private byte[] GenerateSpeedData(int i, Stopwatch stopwatch)
        {
            byte[] data = generateAPage(0x10);

            double speed = 40 * (256 / (this.resistance + 1)) * (Math.Sin(i * 0.1) + 1) / 2;
            short speedcalc = (short)(speed * 1000 * (1 / 3.6));

            byte[] bytes = BitConverter.GetBytes(speedcalc);

            long stopwatchElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            data[6] = (byte)(stopwatchElapsedMilliseconds / 250); // Elapsed Time
            long timeDifference = stopwatchElapsedMilliseconds - elapsedTime;
            elapsedTime = stopwatchElapsedMilliseconds;
            double timeDoubleDifference = (double)timeDifference / 1000;
            double metersPerSecond = speed * (1 / 3.6);
            metersTraveled += timeDoubleDifference * metersPerSecond;
            data[7] = (byte)metersTraveled;

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
            data[4] = pagenumber;
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

    internal class FakeBikeData : avansBikeData
    { 
        public FakeBikeData(byte[] data, string serviceName)
        {
            this.Data = data;
            this.ServiceName = serviceName;
        }
    }
}
