using System;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib
{
    public class Gsm
    {
        private readonly IGsmStream stream;
        private const int DELAY_MS = 25;
        private const string OK_RESPONSE = "OK";

        public enum Mode { Text = 1 } // PDU = 0

        public Gsm(IGsmStream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public Task<bool> InitializeAsync(Mode mode)
        {
            return Task.Factory.StartNew(() =>
            {
                bool status = false;
                status = stream.SendCheckReply("AT\r\n", OK_RESPONSE, 100);
                if (status)
                {
                    Thread.Sleep(DELAY_MS);
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
                    Thread.Sleep(DELAY_MS);
                    status = stream.SendCheckReply($"{message}\x1A\r\n", OK_RESPONSE, 180_000);
                }
                return status;
            });
        }
    }
}
