using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerClient.Data;
using System.IO;

namespace ServerClientTest
{
    [TestClass()]
    public class UnitTestDataStore
    {
        [TestMethod()]
        public void DataStoreTest()
        {
            // Test 1
            try
            {
                var dataStore = new DataHandler();
                dataStore.AddFile("test", "henk");
                Assert.IsTrue(File.Exists(Directory.GetCurrentDirectory() + "/clients/storage/test.json"));
                Assert.IsTrue(dataStore.ClientData.ContainsKey("test"));
            }
            finally
            {
                if (File.Exists(Directory.GetCurrentDirectory() + "/clients/storage/test.json"))
                    File.Delete(Directory.GetCurrentDirectory() + "/clients/storage/test.json");
            }

            // Test 2
            try
            {
                File.WriteAllText(Directory.GetCurrentDirectory() + "/clients/storage/test.json",
                    @"{ ""id"": ""test"", ""name"": ""henk"", ""dataJson"": [] }");
                var dataStore = new DataHandler();
                dataStore.LoadAllData();
                Assert.IsTrue(File.Exists(Directory.GetCurrentDirectory() + "/clients/storage/test.json"));
                Assert.IsTrue(dataStore.ClientData.ContainsKey("test"));
            }
            finally
            {
                if (File.Exists(Directory.GetCurrentDirectory() + "/clients/storage/test.json"))
                    File.Delete(Directory.GetCurrentDirectory() + "/clients/storage/test.json");
            }
        }
    }
}
