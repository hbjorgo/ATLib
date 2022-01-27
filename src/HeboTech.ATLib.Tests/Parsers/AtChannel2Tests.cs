using HeboTech.ATLib.Parsers;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HeboTech.ATLib.Tests.Parsers
{
    public class AtChannel2Tests
    {
        private readonly Mock<IAtWriter> atWriter;
        private readonly DummyAtReader atReader;

        public AtChannel2Tests()
        {
            atWriter = new Mock<IAtWriter>();
            atReader = new();
        }

        [Fact]
        public void Command_returns_success()
        {
            using AtChannel2 dut = new(atReader, atWriter.Object);
            dut.Start();

            Task<AtResponse> commandTask = dut.SendCommand("TestCommand");

            atReader.QueueLine("OK");
            AtResponse response = commandTask.Result;

            Assert.True(response.Success);
        }

        [Fact]
        public void Command_returns_invalid()
        {
            using AtChannel2 dut = new(atReader, atWriter.Object);
            dut.Start();

            Task<AtResponse> commandTask = dut.SendCommand("TestCommand");

            atReader.QueueLine("ERROR");
            AtResponse response = commandTask.Result;

            Assert.False(response.Success);
            Assert.Equal("ERROR", response.FinalResponse);
        }

        [Fact]
        public async Task Command_timeout_throws()
        {
            AtChannel2 dut = new(atReader, atWriter.Object);
            dut.Start();

            await Assert.ThrowsAsync<TimeoutException>(() => dut.SendCommand("TestCommand"));
        }

        [Fact]
        public void Command_gets_response()
        {
            atWriter
                .Setup(x => x.WriteLineAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));

            using AtChannel2 dut = new(atReader, atWriter.Object);
            dut.Start();

            Task<AtResponse> commandTask = dut.SendSingleLineCommandAsync("AT+CSQ", "+CSQ");

            atReader.QueueLine("");
            atReader.QueueLine("+CSQ: 25,99");
            atReader.QueueLine("");
            atReader.QueueLine("OK");
            atReader.QueueLine("");

            AtResponse response = commandTask.Result;

            Assert.Single(response.Intermediates);
            Assert.Equal("+CSQ: 25,99", response.Intermediates.First());
            Assert.Equal("OK", response.FinalResponse);
        }

        [Fact]
        public void CommandIsWrittenToOutputStreamTestAsync()
        {
            atWriter
                .Setup(x => x.WriteLineAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));

            using AtChannel2 dut = new(atReader, atWriter.Object);
            dut.Start();

            Task<AtResponse> commandTask = dut.SendCommand("Test");

            atReader.QueueLine("OK");
            AtResponse response = commandTask.Result;

            atWriter.Verify(x => x.WriteLineAsync("Test", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Two_singlelinecommands_get_responses()
        {
            atWriter
                .Setup(x => x.WriteLineAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));

            using AtChannel2 dut = new(atReader, atWriter.Object);
            dut.Start();

            Task<AtResponse> commandTask1 = dut.SendSingleLineCommandAsync("AT+CSQ", "+CSQ");

            atReader.QueueLine("");
            atReader.QueueLine("+CSQ: 25,99");
            atReader.QueueLine("");
            atReader.QueueLine("OK");
            atReader.QueueLine("");

            AtResponse response1 = commandTask1.Result;

            Task<AtResponse> commandTask2 = dut.SendSingleLineCommandAsync("AT+CSQ", "+CSQ");

            atReader.QueueLine("");
            atReader.QueueLine("+CSQ: 50,80");
            atReader.QueueLine("");
            atReader.QueueLine("OK");
            atReader.QueueLine("");

            AtResponse response2 = commandTask2.Result;

            Assert.Single(response1.Intermediates);
            Assert.Equal("+CSQ: 25,99", response1.Intermediates.First());
            Assert.Equal("OK", response1.FinalResponse);

            Assert.Single(response2.Intermediates);
            Assert.Equal("+CSQ: 50,80", response2.Intermediates.First());
            Assert.Equal("OK", response2.FinalResponse);
        }

        [Fact]
        public void Two_singlelinecommands_and_command_get_responses()
        {
            atWriter
                .Setup(x => x.WriteLineAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));

            using AtChannel2 dut = new(atReader, atWriter.Object);
            dut.Start();

            Task<AtResponse> commandTask1 = dut.SendSingleLineCommandAsync("AT+CSQ", "+CSQ");

            atReader.QueueLine("");
            atReader.QueueLine("+CSQ: 25,99");
            atReader.QueueLine("");
            atReader.QueueLine("OK");
            atReader.QueueLine("");

            AtResponse response1 = commandTask1.Result;

            Task<AtResponse> commandTask2 = dut.SendCommand("ATA");

            atReader.QueueLine("OK");

            AtResponse response2 = commandTask2.Result;

            Task<AtResponse> commandTask3 = dut.SendSingleLineCommandAsync("AT+CSQ", "+CSQ");

            atReader.QueueLine("");
            atReader.QueueLine("+CSQ: 50,80");
            atReader.QueueLine("");
            atReader.QueueLine("OK");
            atReader.QueueLine("");

            AtResponse response3 = commandTask3.Result;

            Assert.True(response1.Success);
            Assert.Single(response1.Intermediates);
            Assert.Equal("+CSQ: 25,99", response1.Intermediates.First());
            Assert.Equal("OK", response1.FinalResponse);

            Assert.True(response2.Success);
            Assert.Empty(response2.Intermediates);
            Assert.Equal("OK", response2.FinalResponse);

            Assert.True(response3.Success);
            Assert.Single(response3.Intermediates);
            Assert.Equal("+CSQ: 50,80", response3.Intermediates.First());
            Assert.Equal("OK", response3.FinalResponse);
        }
    }
}
