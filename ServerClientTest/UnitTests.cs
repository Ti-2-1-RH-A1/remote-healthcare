using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerClient.Tests
{
    [TestClass()]
    public class UnitTests
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
            Program.RunServer("", false);

            await Task.Delay(1000);

            var client = new Client("fiets", false);

            await Task.Delay(3000);
            Assert.IsTrue(client.loggedIn);
        }

        [TestMethod()]
        public void TestNoCertError()
        {
            Assert.ThrowsException<Exception>(() => Program.RunServer(@"Serhdjfjhdver.pfx"));
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
