using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteHealthcare.bike;

namespace RemoteHealthcare.Tests
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
