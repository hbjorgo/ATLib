using System;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib
{
    public class Gsm : IGsm
    {
        private readonly IGsmStream stream;
        private readonly int writeDelayMs = 25;
        private const string OK_RESPONSE = "OK";

        public Gsm(IGsmStream stream, int writeDelayMs = 25)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (writeDelayMs < 0)
                throw new ArgumentOutOfRangeException($"{nameof(writeDelayMs)} must be a positive number");
            this.writeDelayMs = writeDelayMs;
        }

        public Task<bool> InitializeAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                return stream.SendCheckReply("AT\r\n", OK_RESPONSE, 100);
            });
        }

        public Task<bool> SetModeAsync(Mode mode)
        {
            return Task.Factory.StartNew(() =>
            {
                return stream.SendCheckReply($"AT+CMGF={(int)mode}\r\n", OK_RESPONSE, 5_000);
            });
        }

        public Task<bool> SendSmsAsync(PhoneNumber phoneNumber, string message)
        {
            return Task.Factory.StartNew(() =>
            {
                bool status = false;
                //TODO: This times out because it waits for '\r\n', which is not coming (actual response is '> ')
                status = stream.SendCheckReply($"AT+CMGS=\"{phoneNumber.ToString()}\"\r", "> ", 5_000);
                if (status)
                {
                    Thread.Sleep(writeDelayMs);
                    status = stream.SendCheckReply($"{message}\x1A\r\n", OK_RESPONSE, 180_000);
                }
                return status;
            });
        }

        public Task<bool> UnlockSimAsync(Pin pin)
        {
            return Task.Factory.StartNew(() =>
            {
                return stream.SendCheckReply($"AT+CPIN={pin.ToString()}", OK_RESPONSE, 20_000);
            });
        }
    }
}
