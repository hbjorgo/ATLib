using HeboTech.ATLib.Parsers;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HeboTech.ATLib.Tests.Parsers
{
    public class AtChannelTests
    {
        [Fact]
        public async Task CommandGetsResponseTestAsync()
        {
            using MemoryStream inputStream = new MemoryStream(Encoding.UTF8.GetBytes("\r\n+CSQ: 25,99\r\n\r\nOK\r\n\r\n")) { Position = 0 };
            using MemoryStream outputStream = new MemoryStream(1024);
            using TestableAtChannel channel = new TestableAtChannel(inputStream, outputStream);

            Task<(AtError error, AtResponse response)> commandTask = channel.SendSingleLineCommandAsync("AT+CSQ", "+CSQ");
            channel.StartReaderLoop();
            (AtError error, AtResponse response) = await commandTask;

            Assert.Equal(AtError.NO_ERROR, error);
            Assert.Equal("+CSQ: 25,99", response.Intermediates.First());
            Assert.Equal("OK", response.FinalResponse);
        }

        [Fact]
        public async Task CommandIsWrittenToOutputStreamTestAsync()
        {
            using MemoryStream inputStream = new MemoryStream(Encoding.UTF8.GetBytes("\r\nOK\r\n")) { Position = 0 };
            using MemoryStream outputStream = new MemoryStream(1024);
            using TestableAtChannel channel = new TestableAtChannel(inputStream, outputStream);

            Task<(AtError error, AtResponse response)> commandTask = channel.SendCommand("Test");
            channel.StartReaderLoop();
            (AtError error, AtResponse response) = await commandTask;

            outputStream.Position = 0;
            byte[] buffer = new byte[outputStream.Length];
            outputStream.Read(buffer, 0, buffer.Length);
            string result = Encoding.UTF8.GetString(buffer);

            Assert.Equal("Test\r", result);
            Assert.Equal(AtError.NO_ERROR, error);
        }
    }
}
