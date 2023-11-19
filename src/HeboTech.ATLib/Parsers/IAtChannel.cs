using System;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsers
{
    public interface IAtChannel
    {
        TimeSpan DefaultCommandTimeout { get; set; }

        event EventHandler<UnsolicitedEventArgs> UnsolicitedEvent;

        void Dispose();
        Task<AtResponse> SendCommand(string command, TimeSpan? timeout = null);
        Task<AtResponse> SendMultilineCommand(string command, string responsePrefix, TimeSpan? timeout = null);
        Task<AtResponse> SendSingleLineCommandAsync(string command, string responsePrefix, TimeSpan? timeout = null);
        Task<AtResponse> SendSmsAsync(string command, string pdu, string responsePrefix, TimeSpan? timeout = null);
        void Open();
        void Close();
    }
}