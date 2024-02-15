using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Extensions;
using HeboTech.ATLib.PDU;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Events
{
    public class SmsStatusReportEventArgs
    {
        public SmsStatusReportEventArgs(SmsStatusReport smsStatusReport)
        {
            SmsStatusReport = smsStatusReport;
        }

        public SmsStatusReport SmsStatusReport { get; }

        public static SmsStatusReportEventArgs CreateFromResponse(string line1, string line2)
        {
            var line1Match = Regex.Match(line1, @"\+CDS:\s(?<length>\d+)");
            if (line1Match.Success)
            {
                byte length = byte.Parse(line1Match.Groups["length"].Value);
                var report = SmsStatusReportDecoder.Decode(line2.ToByteArray(), length);
                return new SmsStatusReportEventArgs(report);
            }

            return default;
        }
    }
}
