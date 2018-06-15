using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HeboTech.ATLib
{
    public class Gsm
    {
        private Encoding encoding = Encoding.ASCII;
        private readonly Stream stream;

        public Gsm(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public Task InitializeAsync()
        {
            return WriteAsync("AT\r\n")
                .ContinueWith(completed => Task.Delay(250))
                .ContinueWith(completed => WriteAsync("AT+CMGF=1\r\n"))
                .ContinueWith(completed => Task.Delay(250));
        }

        public Task SendSmsAsync(string phoneNumber, string message)
        {
            return WriteAsync("AT+CMGS=\"")
                .ContinueWith(completed => WriteAsync(phoneNumber))
                .ContinueWith(completed => WriteAsync("\"\r"))
                .ContinueWith(completed => Task.Delay(500))
                .ContinueWith(completed => WriteAsync(message))
                .ContinueWith(completed => WriteAsync("\x1A\r\n"))
                .ContinueWith(completed => Task.Delay(2000));
        }

        private Task WriteAsync(string text)
        {
            byte[] bytesToWrite = encoding.GetBytes(text);
            return stream.WriteAsync(bytesToWrite, 0, bytesToWrite.Length);
        }
    }
}
