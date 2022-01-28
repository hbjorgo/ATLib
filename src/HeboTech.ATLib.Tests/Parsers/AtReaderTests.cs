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
            stream.Write(buffer);
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
            stream.Write(buffer);
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
    }
}
