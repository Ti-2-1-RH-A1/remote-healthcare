using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerClient.Data;

namespace ServerClientTest
{
    [TestClass()]
    public class UnitTestDataStore
    {
        [TestMethod()]
        public void DataStoreTest()
        {
            var dataStore = new DataHandler();
            dataStore.AddFile("test", "henk");
        }
    }
}
