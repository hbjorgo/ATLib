using System.Text;
using Xunit;

namespace HeboTech.ATLib.Tests
{
    public class GsmTests
    {
        [Fact]
        public void InitializeTest()
        {
            Encoding enc = Encoding.ASCII;
            using (TestStream stream = new TestStream(enc))
            using (GsmStream gsmStream = new GsmStream(stream, enc))
            {
                stream.DataWritten += (s, e) =>
                {
                    if (e.Data == "AT\r\n")
                        stream.SetReply("\r\nOK\r\n");
                };

                Gsm g = new Gsm(gsmStream);
                bool result = g.InitializeAsync().Result;

                Assert.True(result);
            }
        }

        [Fact]
        public void UnlockSimTest()
        {
            Pin pin = new Pin(1, 2, 3, 4);

            Encoding enc = Encoding.ASCII;
            using (TestStream stream = new TestStream(enc))
            using (GsmStream gsmStream = new GsmStream(stream, enc))
            {
                stream.DataWritten += (s, e) =>
                {
                    if (e.Data == $"AT+CPIN={pin.ToString()}")
                        stream.SetReply("\r\nOK\r\n");
                };

                Gsm g = new Gsm(gsmStream);
                bool result = g.UnlockSimAsync(pin).Result;

                Assert.True(result);
            }
        }

        [Fact]
        public void SetModeTest()
        {
            Mode mode = Mode.Text;

            Encoding enc = Encoding.ASCII;
            using (TestStream stream = new TestStream(enc))
            using (GsmStream gsmStream = new GsmStream(stream, enc))
            {
                stream.DataWritten += (s, e) =>
                {
                    if (e.Data == $"AT+CMGF={(int)mode}\r\n")
                        stream.SetReply("\r\nOK\r\n");
                };

                Gsm g = new Gsm(gsmStream);
                bool result = g.SetModeAsync(mode).Result;

                Assert.True(result);
            }
        }

        [Fact]
        public void SendSmsTest()
        {
            PhoneNumber phoneNumber = new PhoneNumber("12345678");
            string message = "Msg";

            Encoding enc = Encoding.ASCII;
            using (TestStream stream = new TestStream(enc))
            using (GsmStream gsmStream = new GsmStream(stream, enc))
            {
                stream.DataWritten += (s, e) =>
                {
                    if (e.Data == $"AT+CMGS=\"{phoneNumber.ToString()}\"\r")
                        stream.SetReply("> ");
                    if (e.Data == $"{message}\x1A\r\n")
                        stream.SetReply("\r\nOK\r\n");
                };

                Gsm g = new Gsm(gsmStream);
                bool result = g.SendSmsAsync(phoneNumber, message).Result;

                Assert.True(result);
            }
        }
    }
}
