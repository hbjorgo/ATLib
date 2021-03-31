using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using HeboTech.ATLib.Parsers;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Modems.Generic
{
    public class _V_25TER
    {
        private readonly AtChannel channel;

        public event EventHandler<IncomingCallEventArgs> IncomingCall;
        public event EventHandler<MissedCallEventArgs> MissedCall;

        public _V_25TER(AtChannel channel)
        {
            this.channel = channel;
            channel.UnsolicitedEvent += Channel_UnsolicitedEvent;
        }

        private void Channel_UnsolicitedEvent(object sender, UnsolicitedEventArgs e)
        {
            if (e.Line1 == "RING")
                IncomingCall?.Invoke(this, new IncomingCallEventArgs());
            else if (e.Line1.StartsWith("MISSED_CALL: "))
                MissedCall?.Invoke(this, MissedCallEventArgs.CreateFromResponse(e.Line1));
        }

        public virtual CommandStatus AnswerIncomingCall()
        {
            var error = channel.SendCommand("ATA");

            if (error == AtError.NO_ERROR)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public virtual CommandStatus DisableEcho()
        {
            var error = channel.SendCommand("ATE0");

            if (error == AtError.NO_ERROR)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public virtual ProductIdentificationInformation GetProductIdentificationInformation()
        {
            var error = channel.SendMultilineCommand("ATI", null, out AtResponse response);

            if (error == AtError.NO_ERROR)
            {
                StringBuilder builder = new StringBuilder();
                foreach (string line in response.Intermediates)
                {
                    builder.AppendLine(line);
                }

                return new ProductIdentificationInformation(builder.ToString());
            }
            return null;
        }

        public virtual CallDetails Hangup()
        {
            var error = channel.SendSingleLineCommand("AT+CHUP", "VOICE CALL:", out AtResponse response);

            if (error == AtError.NO_ERROR)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"VOICE CALL: END: (?<duration>\d+)");
                if (match.Success)
                {
                    int duration = int.Parse(match.Groups["duration"].Value);
                    return new CallDetails(TimeSpan.FromSeconds(duration));
                }
            }
            return null;
        }
    }
}
