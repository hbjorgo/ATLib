using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Events
{
    public class SmsStorageReferenceReceivedEventArgs
    {
        public SmsStorageReferenceReceivedEventArgs(string storage, int index)
        {
            Storage = storage;
            Index = index;
        }

        public string Storage { get; }
        public int Index { get; }

        public static SmsStorageReferenceReceivedEventArgs CreateFromResponse(string response)
        {
            var match = Regex.Match(response, @"\+CMTI:\s""(?<storage>[A-Z]+)"",(?<index>\d+)");
            if (match.Success)
            {
                string storage = match.Groups["storage"].Value;
                int index = int.Parse(match.Groups["index"].Value);
                return new SmsStorageReferenceReceivedEventArgs(storage, index);
            }
            return default;
        }
    }
}
