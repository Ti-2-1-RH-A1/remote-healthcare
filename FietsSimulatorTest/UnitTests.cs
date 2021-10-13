using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteHealthcare;
using RemoteHealthcare.bike;
using System;
using System.Threading;

namespace FietsSimulatorTest
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestSimulator()
        {
            MockDeviceManager deviceManager = new MockDeviceManager();
            deviceManager.Start((IBikeManager.BikeType.SIMULATOR_BIKE, ""));

            Assert.IsTrue(((short)deviceManager.CheckedTypes) == 15);
        }
    }
}
