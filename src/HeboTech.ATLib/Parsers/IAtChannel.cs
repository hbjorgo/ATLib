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
        Task<AtMultiLineResponse> SendMultilineCommand(string command, string responsePrefix, TimeSpan? timeout = null);
        Task<AtSingleLineResponse> SendSingleLineCommandAsync(string command, string responsePrefix, TimeSpan? timeout = null);
        Task<AtMultiLineResponse> SendSmsAsync(string command, string pdu, string responsePrefix, TimeSpan? timeout = null);
        void Open();
        void Close();
    }
}