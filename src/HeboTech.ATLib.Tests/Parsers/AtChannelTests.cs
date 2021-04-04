using HeboTech.ATLib.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text;

namespace HeboTech.ATLib.Tests.Parsers
{
    [TestClass]
    public class AtChannelTests
    {
        [TestMethod]
        public async System.Threading.Tasks.Task CommandIsWrittenToOutputStreamTestAsync()
        {
            using MemoryStream inputStream = new MemoryStream();
            using MemoryStream outputStream = new MemoryStream(128);
            using AtChannel channel = new AtChannel(inputStream, outputStream);

            (AtError error, AtResponse response) = await channel.SendCommand("Test");
            inputStream.Write(Encoding.UTF8.GetBytes("OK\r\n"));

            outputStream.Position = 0;
            byte[] buffer = new byte[outputStream.Length];
            outputStream.Read(buffer, 0, buffer.Length);
            string result = Encoding.UTF8.GetString(buffer);
            channel.Close();

            Assert.AreEqual("Test\r", result);
            Assert.AreEqual(AtError.NO_ERROR, error);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task CommandGetsResponseTestAsync()
        {
            using MemoryStream inputStream = new MemoryStream();
            using MemoryStream outputStream = new MemoryStream(128);
            using AtChannel channel = new AtChannel(inputStream, outputStream);

            (AtError error, AtResponse response) = await channel.SendSingleLineCommandAsync("Test", "+ABCD");
            inputStream.Write(Encoding.UTF8.GetBytes("+ABCD\r\nOK\r\n"));
            channel.Close();

            Assert.AreEqual(AtError.NO_ERROR, error);
            Assert.AreEqual("+ABCD", response.Intermediates.First());
            Assert.AreEqual("OK", response.FinalResponse);
        }
    }
}
