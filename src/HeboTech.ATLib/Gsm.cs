using System;
using System.Threading.Tasks;

namespace HeboTech.ATLib
{
    public class Gsm
    {
        private readonly IGsmStream stream;
        private const int DELAY_MS = 25;

        private const string ERROR_READING_RESPONSE = "Error reading response";
        private const string INVALID_RESPONSE = "Invalid response";

        public enum Mode { Text = 1 } // PDU = 0

        public Gsm(IGsmStream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public Task InitializeAsync(Mode mode)
        {
            return stream.WriteAsync("AT\r\n")
                .ContinueWith(async completed =>
                {
                    (Status status, string payload) = await stream.GetStandardReplyAsync(100);
                    ThrowIfNotOk(status);
                })
                .ContinueWith(completed => Task.Delay(DELAY_MS))
                .ContinueWith(completed => stream.WriteAsync($"AT+CMGF={(int)mode}\r\n"))
                .ContinueWith(async completed =>
                {
                    (Status status, string payload) = await stream.GetStandardReplyAsync(5_000);
                    ThrowIfNotOk(status);
                });
        }

        public Task SendSmsAsync(string phoneNumber, string message)
        {
            return stream.WriteAsync($"AT+CMGS=\"{phoneNumber}\"\r")
                .ContinueWith(async completed =>
                {
                    (Status status, string payload) = await stream.GetStandardReplyAsync(5_000);
                    ThrowIfNotOk(status);
                    if (status == Status.OK && payload != "> ")
                        throw new GsmException(INVALID_RESPONSE);
                })
                .ContinueWith(completed => Task.Delay(DELAY_MS))
                .ContinueWith(completed => stream.WriteAsync($"{message}\x1A\r\n"))
                .ContinueWith(async completed =>
                {
                    (Status status, string payload) = await stream.GetStandardReplyAsync(180_000);
                    ThrowIfNotOk(status);
                })
                .ContinueWith(completed => Task.Delay(DELAY_MS));
        }

        private static void ThrowIfNotOk(Status status)
        {
            if (status == Status.ERROR)
                throw new GsmException(ERROR_READING_RESPONSE);
        }
    }
}
