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
        public void TestNoCertError()
        {
            Assert.ThrowsException<Exception>(() => Program.RunServer(@"Server.pfx"));
        }
    }
}
