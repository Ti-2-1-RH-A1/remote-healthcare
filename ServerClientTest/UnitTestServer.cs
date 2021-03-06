using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetProtocol;

namespace ServerClient.Tests
{
    [TestClass()]
    public class UnitTestServer
    {
        [TestMethod()]
        public void StringifyParseHeadersTest()
        {
            Dictionary<string, string> headers = new()
            {
                { "Test", "Header" },
                { "Test2", "Header2" }
            };
            var stringHeader = Protocol.StringifyHeaders(headers);
            var dictHeader = Protocol.ParseHeaders(stringHeader).Item1;
            CollectionAssert.AreEqual(headers, dictHeader);
        }

        [TestMethod()]
        public void StringifyParseDataTest()
        {
            Dictionary<string, string> data = new()
            {
                { "Test", "Data" },
                { "Test2", "Data2" }
            };
            var stringData = Protocol.StringifyData(data);
            var dictData = Protocol.ParseData(stringData);
            CollectionAssert.AreEqual(data, dictData);
        }

        [TestMethod()]
        public async Task TestServerAsync()
        {
            new Server("", AuthHandler.Init(), false);

            await Task.Delay(1000);

            var client = new Client("localhost", false, "Test", "e5OczxmOprhbpDVUtF4JmeM7gVdqrFDl");

            await Task.Delay(3500);
            Assert.IsTrue(client.loggedIn);
            client.SendPacket(new Dictionary<string, string>() {
                { "Method", "Get" },
                { "GetKeys", "speed" },
            }, new Dictionary<string, string>(), (e1, e2) =>
            {
                Assert.IsTrue(true);
            });
        }

        [TestMethod()]
        public void TestNoCertError()
        {
            Assert.ThrowsException<Exception>(() => new Server(@"Serhdjfjhdver.pfx", AuthHandler.Init()));
        }

        [TestMethod()]
        public void ParsePacketTest()
        {
            Dictionary<string, string> headers = new()
            {
                { "Test", "Header" },
                { "Test2", "Header2" }
            };
            Dictionary<string, string> data = new()
            {
                { "Test", "Data" },
                { "Test2", "Data2" }
            };
            var packetString = $"{Protocol.StringifyHeaders(headers)}{Protocol.StringifyData(data)}";
            var packetDicts = Protocol.ParsePacket(packetString);
            CollectionAssert.AreEqual(headers, packetDicts.Item1);
            CollectionAssert.AreEqual(data, packetDicts.Item2);
        }

        [TestMethod()]
        public void ParseHeadersTest()
        {
            Assert.ThrowsException<ArgumentException>(() => Protocol.ParseHeaders("kdfkjfdkjfkddkfkdkjfkjdkjfkjdkfkjd"));
        }

        [TestMethod()]
        public void ParseDataTest()
        {
            Assert.ThrowsException<ArgumentException>(() => Protocol.ParseData("kdfkjfdkjfkddkfkdkjfkjdkjfkjdkfkjd"));
        }
    }
}
