using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerClient;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ServerClientTest
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public async Task TestServerAsync()
        {
            var currentDir = Directory.GetCurrentDirectory();

            Program.RunServer("", false);

            var client = new Client("fiets", false);

            await Task.Delay(3000);
            Assert.IsTrue(client.loggedIn);
        }

        [TestMethod]
        public void TestNoCertError()
        {
            Assert.ThrowsException<Exception>(() => Program.RunServer(@"Server.pfx"));
        }
    }
}
