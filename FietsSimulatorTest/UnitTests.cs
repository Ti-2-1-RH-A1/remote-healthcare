using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteHealthcare;
using System;
using System.Diagnostics;

namespace FietsSimulatorTest
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestSimulator()
        {
            Simulator simulator = new Simulator();
            Stopwatch stopwatch = Stopwatch.StartNew();
            int i = 0;
            try
            {
                simulator.RunStep(ref i, ref stopwatch);
            }
            catch (Exception)
            {
                Assert.Fail();
                throw;
            }
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestBluetoothSpeed()
        {
            byte[] SpeedBytes = new byte[8];
            SpeedBytes[4] = 0b11001011;
            SpeedBytes[5] = 0b00010001;
            float test = Bluetooth.ParseSpeed(SpeedBytes);
            Assert.AreEqual(test, 4555f);
        }

        [TestMethod]
        public void TestBluetoothTime()
        {
            byte[] TimeBytes = new byte[8];
            TimeBytes[2] = 0b11001011;
            float test2 = Bluetooth.ParseElapsedTime(TimeBytes);
            Assert.AreEqual(test2, 50.75f);
        }

        [TestMethod]
        public void TestBluetoothDistance()
        {
            byte[] DistanceBytes = new byte[8];
            DistanceBytes[3] = 0b11001011;
            float test3 = Bluetooth.ParseDistance(DistanceBytes);
            Assert.AreEqual(test3, 203f);
        }

        [TestMethod]
        public void TestTwoByteToInt()
        {
            int test = Bluetooth.TwoByteToInt(0b11001011, 0b00010001);
            Assert.AreEqual(test, 4555);
        }
    }
}
