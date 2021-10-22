using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteHealthcare.Bike;

namespace RemoteHealthcare.Tests
{
    [TestClass]
    public class UnitTests
    {
        readonly MockDeviceManager deviceManager = new MockDeviceManager();
        
        [TestMethod]
        public void TestSimulator()
        {
            deviceManager.Start((IBikeManager.BikeType.SIMULATOR_BIKE, ""));

            Assert.IsTrue(((short)deviceManager.CheckedTypes) == 15);
        }

        [TestMethod]
        public void TestBikeManager()
        {

        }
    }
}
