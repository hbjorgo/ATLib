using HeboTech.ATLib.Storage;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Events
{
    public class SmsStatusReportStorageReferenceEventArgs
    {
        public SmsStatusReportStorageReferenceEventArgs(MessageStorage storage, int index)
        {
            Storage = storage;
            Index = index;
        }

        public MessageStorage Storage { get; }
        public int Index { get; }

        public static SmsStatusReportStorageReferenceEventArgs CreateFromResponse(string line1)
        {
            var match = Regex.Match(line1, @"\+CDSI:\s""(?<storage>[a-zA-Z]+)"",(?<index>\d+)", RegexOptions.Compiled);
            if (match.Success)
            {
                string storage = match.Groups["storage"].Value;
                int index = int.Parse(match.Groups["index"].Value);
                return new SmsStatusReportStorageReferenceEventArgs(MessageStorage.Parse(storage), index);
            }

            return default;
        }
    }
}
