using HeboTech.ATLib.Parsers;
using System.IO;
using System.Threading;

namespace HeboTech.ATLib.Tests
{
    public class TestableAtChannel : AtChannel
    {
        public TestableAtChannel(Stream inputStream, Stream outputStream)
            : base(inputStream, outputStream, new SemaphoreSlim(0, 1))
        {
        }

        public void StartReaderLoop()
        {
            readLoopStartSemaphore.Release();
        }
    }
}
