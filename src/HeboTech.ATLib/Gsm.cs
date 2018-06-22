using System;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib
{
    public class Gsm
    {
        private readonly IGsmStream stream;
        private readonly int writeDelayMs = 25;
        private const string OK_RESPONSE = "OK";

        public enum Mode { Text = 1 } // PDU = 0

        public Gsm(IGsmStream stream, int writeDelayMs = 25)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (writeDelayMs < 0)
                throw new ArgumentOutOfRangeException($"{nameof(writeDelayMs)} must be a positive number");
            this.writeDelayMs = writeDelayMs;
        }

        public Task<bool> InitializeAsync(Mode mode)
        {
            return Task.Factory.StartNew(() =>
            {
                bool status = false;
                status = stream.SendCheckReply("AT\r\n", OK_RESPONSE, 100);
                if (status)
                {
                    Thread.Sleep(writeDelayMs);
                    status = stream.SendCheckReply($"AT+CMGF={(int)mode}\r\n", OK_RESPONSE, 5_000);
                }
                return status;
            });
        }

        public Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            return Task.Factory.StartNew(() =>
            {
                bool status = false;
                status = stream.SendCheckReply($"AT+CMGS=\"{phoneNumber}\"\r", "> ", 5_000);
                if (status)
                {
                    Thread.Sleep(writeDelayMs);
                    status = stream.SendCheckReply($"{message}\x1A\r\n", OK_RESPONSE, 180_000);
                }
                return status;
            });
        }
    }
}
