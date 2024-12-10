using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Extensions;
using HeboTech.ATLib.PDU;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Events
{
    public class SmsReceivedEventArgs
    {
        public SmsReceivedEventArgs(SmsDeliver smsDeliver)
        {
            SmsDeliver = smsDeliver;
        }

        public SmsDeliver SmsDeliver { get; }

        public static SmsReceivedEventArgs CreateFromResponse(string line1, string line2)
        {
            var line1Match = Regex.Match(line1, @"\+CMT:\s(""(?<alpha>[\+0-9]*)"")?,(?<length>\d+)");
            if (line1Match.Success)
            {
                byte length = byte.Parse(line1Match.Groups["length"].Value);
                var smsDeliver = SmsDeliverDecoder.Decode(line2.ToByteArray());
                return new SmsReceivedEventArgs(smsDeliver);
            }

            return default;
        }
    }
}
