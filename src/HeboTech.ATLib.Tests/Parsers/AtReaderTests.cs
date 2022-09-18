using HeboTech.ATLib.Parsers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HeboTech.ATLib.Tests.Parsers
{
    public class AtReaderTests
    {
        [Fact]
        public async Task Lines_are_readAsync()
        {
            MemoryStream stream = new MemoryStream(512);
            AtReader dut = new(stream);

            string input = "Line1\r\nLine2\r\nLine3\r\n";
            byte[] buffer = Encoding.UTF8.GetBytes(input);
#if NET40_OR_GREATER
            stream.Write(buffer, 0, buffer.Length);
#else
            stream.Write(buffer);
#endif
            stream.Position = 0;

            dut.Open();

            string line1 = await dut.ReadAsync();
            string line2 = await dut.ReadAsync();
            string line3 = await dut.ReadAsync();

            dut.Close();

            Assert.Equal("Line1", line1);
            Assert.Equal("Line2", line2);
            Assert.Equal("Line3", line3);
        }

        [Fact]
        public async Task Lines_and_sms_prompts_are_readAsync()
        {
            MemoryStream stream = new MemoryStream(512);
            AtReader dut = new(stream);

            string input = "Line1\r\nLine2\r\n> Line3\r\n";
            byte[] buffer = Encoding.UTF8.GetBytes(input);
#if NET40_OR_GREATER
            stream.Write(buffer, 0, buffer.Length);
#else
            stream.Write(buffer);
#endif
            stream.Position = 0;

            dut.Open();

            string line1 = await dut.ReadAsync();
            string line2 = await dut.ReadAsync();
            string smsPrompt = await dut.ReadAsync();
            string line3 = await dut.ReadAsync();

            dut.Close();

            Assert.Equal("Line1", line1);
            Assert.Equal("Line2", line2);
            Assert.Equal("> ", smsPrompt);
            Assert.Equal("Line3", line3);
        }

        [Fact]
        public async Task Empty_lines_are_readAsync()
        {
            MemoryStream stream = new MemoryStream(512);
            AtReader dut = new(stream);

            string input = "\r\n\r\n\r\n";
            byte[] buffer = Encoding.UTF8.GetBytes(input);
#if NET40_OR_GREATER
            stream.Write(buffer, 0, buffer.Length);
#else
            stream.Write(buffer);
#endif
            stream.Position = 0;

            dut.Open();

            string line1 = await dut.ReadAsync();
            string line2 = await dut.ReadAsync();
            string line3 = await dut.ReadAsync();

            dut.Close();

            Assert.Equal("", line1);
            Assert.Equal("", line2);
            Assert.Equal("", line3);
        }

        [Fact]
        public async Task Cme_Error_is_readAsync()
        {
            MemoryStream stream = new MemoryStream(512);
            AtReader dut = new(stream);

            string input = "+CME ERROR: ErrorMessage\r\n";
            byte[] buffer = Encoding.UTF8.GetBytes(input);
#if NET40_OR_GREATER
            stream.Write(buffer, 0, buffer.Length);
#else
            stream.Write(buffer);
#endif
            stream.Position = 0;

            dut.Open();

            string line1 = await dut.ReadAsync();

            dut.Close();

            Assert.Equal("+CME ERROR: ErrorMessage", line1);
        }

        [Fact]
        public async Task Ring_is_readAsync()
        {
            MemoryStream stream = new MemoryStream(512);
            AtReader dut = new(stream);

            string input = "RING\r\n\r\nRING\r\n\r\nMISSED_CALL: 01:23PM 12345678\r\n";
            byte[] buffer = Encoding.UTF8.GetBytes(input);
#if NET40_OR_GREATER
            stream.Write(buffer, 0, buffer.Length);
#else
            stream.Write(buffer);
#endif
            stream.Position = 0;

            dut.Open();

            string line1 = await dut.ReadAsync();
            string line2 = await dut.ReadAsync();
            string line3 = await dut.ReadAsync();
            string line4 = await dut.ReadAsync();
            string line5 = await dut.ReadAsync();

            dut.Close();

            Assert.Equal("RING", line1);
            Assert.Equal("", line2);
            Assert.Equal("RING", line3);
            Assert.Equal("", line4);
            Assert.Equal("MISSED_CALL: 01:23PM 12345678", line5);
        }
    }
}
