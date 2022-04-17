using HeboTech.ATLib.Parsers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HeboTech.ATLib.Tests.Parsers
{
    public class AtWriterTests
    {
        [Fact]
        public async Task WriteLineAsync_writes_line_with_newline()
        {
            MemoryStream stream = new MemoryStream(512);
            AtWriter dut = new(stream);

            await dut.WriteLineAsync("Line1", default);

            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            int lenght = stream.Read(buffer, 0, buffer.Length);
            Assert.Equal("Line1\r", Encoding.UTF8.GetString(buffer));
        }

        [Fact]
        public async Task WriteSmsPduAndCtrlZAsync_writes_line_with_CtrlZ()
        {
            MemoryStream stream = new MemoryStream(512);
            AtWriter dut = new(stream);

            await dut.WriteSmsPduAndCtrlZAsync("Line1", default);

            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            int lenght = stream.Read(buffer, 0, buffer.Length);
            Assert.Equal("Line1\x1A", Encoding.UTF8.GetString(buffer));
        }
    }
}
