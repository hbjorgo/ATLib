using System.Collections.Generic;
using System.Linq;

namespace HeboTech.ATLib.DTOs
{
    public enum SmsStatus
    {
        REC_UNREAD = 0,
        REC_READ = 1,
        STO_UNSENT = 2,
        STO_SENT = 3,
        ALL = 4
    }

    public static class SmsStatusHelpers
    {
        private static readonly Dictionary<SmsStatus, string> LUT = new Dictionary<SmsStatus, string>
        {
            { SmsStatus.ALL, "ALL" },
            { SmsStatus.REC_READ, "REC READ" },
            { SmsStatus.REC_UNREAD, "REC UNREAD" },
            { SmsStatus.STO_SENT, "STO SENT" },
            { SmsStatus.STO_UNSENT, "STO UNSENT" },
        };

        public static SmsStatus ToSmsStatus(string text)
        {
            return LUT.First(x => x.Value == text).Key;
        }

        public static string ToString(SmsStatus smsStatus)
        {
            return LUT.First(x => x.Key == smsStatus).Value;
        }
    }
}