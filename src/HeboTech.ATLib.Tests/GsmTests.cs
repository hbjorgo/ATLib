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
    }
}
