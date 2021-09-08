using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteHealthcare;

namespace FietsSimulatorTest
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestSimulator()
        {
            int i = 0;
            try
            {
                Simulator.RunStep(ref i);
            }
            catch (System.Exception)
            {
                Assert.Fail();
                throw;
            }
            Assert.IsTrue(true);
        }
    }
}
