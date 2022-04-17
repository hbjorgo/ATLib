using HeboTech.ATLib.Events;
using Xunit;

namespace HeboTech.ATLib.Tests.Events
{
    public class UssdResponseEventArgsTests
    {
        [Fact]
        public void CreateFromResponse_accepts_singleline_response()
        {
            string response = "+CUSD: 1,\"Single line response\",15";

            UssdResponseEventArgs dut = UssdResponseEventArgs.CreateFromResponse(response);

            Assert.Equal(1, dut.Status);
            Assert.Equal("Single line response", dut.Response);
            Assert.Equal(15, dut.CodingScheme);
        }

        [Fact]
        public void CreateFromResponse_accepts_multiline_response()
        {
            string response = "+CUSD: 1,\"Line1\r\nLine 2\r\nLine 3\r\nLine 4\r\nLine 5\r\nLine 5\r\nLine 6\",15";
            
            UssdResponseEventArgs dut = UssdResponseEventArgs.CreateFromResponse(response);

            Assert.Equal(1, dut.Status);
            Assert.Equal("Line1\r\nLine 2\r\nLine 3\r\nLine 4\r\nLine 5\r\nLine 5\r\nLine 6", dut.Response);
            Assert.Equal(15, dut.CodingScheme);
        }
    }
}
