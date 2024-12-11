using HeboTech.ATLib.DTOs;
using System;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Events
{
    public class BroadcastMessageReceivedEventArgs
    {
        public BroadcastMessageReceivedEventArgs(BroadcastMessage broadcastMessage)
        {
            BroadcastMessage = broadcastMessage;
        }

        public BroadcastMessage BroadcastMessage { get; }

        public static BroadcastMessageReceivedEventArgs CreateFromResponse(string line1, string line2)
        {
            var line1Match = Regex.Match(line1, @"\+CBM:\s(?<length>\d+)", RegexOptions.Compiled);
            if (line1Match.Success)
            {
                throw new NotImplementedException();
            }

            return default;
        }
    }
}
