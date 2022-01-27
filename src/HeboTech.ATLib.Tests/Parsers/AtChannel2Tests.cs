using HeboTech.ATLib.Parsers;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HeboTech.ATLib.Tests.Parsers
{
    public class AtChannel2Tests
    {
        private readonly Mock<IAtWriter> atWriter;
        private readonly Mock<IAtReader> atReader;

        public AtChannel2Tests()
        {
            atWriter = new Mock<IAtWriter>();
            atReader = new Mock<IAtReader>();
        }

        [Fact]
        public void Not_busy_when_idle()
        {
            AtChannel2 dut = new(atReader.Object, atWriter.Object);

            Assert.False(dut.IsBusy);
        }

        [Fact]
        public async Task Command_returns_success()
        {
            atReader
                .Setup(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("OK");

            AtChannel2 dut = new(atReader.Object, atWriter.Object);
            dut.Start();

            AtResponse response = await dut.SendCommand("TestCommand");

            dut.Stop();

            Assert.True(response.Success);
        }

        [Fact]
        public async Task Command_returns_invalid()
        {
            atReader
                .Setup(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("ERROR");

            AtChannel2 dut = new(atReader.Object, atWriter.Object);
            dut.Start();

            AtResponse response = await dut.SendCommand("TestCommand");

            dut.Stop();

            Assert.False(response.Success);
            Assert.Equal("ERROR", response.FinalResponse);
        }

        [Fact]
        public async Task Command_timeout_throws()
        {
            AtChannel2 dut = new(atReader.Object, atWriter.Object);
            dut.Start();

            await Assert.ThrowsAsync<TimeoutException>(() => dut.SendCommand("TestCommand"));
        }
    }
}
