using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Events
{
    public class BroadcastMessageStorageReferenceReceivedEventArgs
    {
        public BroadcastMessageStorageReferenceReceivedEventArgs(string storage, int index)
        {
            Storage = storage;
            Index = index;
        }

        public string Storage { get; }
        public int Index { get; }

        public static BroadcastMessageStorageReferenceReceivedEventArgs CreateFromResponse(string response)
        {
            var match = Regex.Match(response, @"\+CBMI:\s""(?<storage>[A-Z]+)"",(?<index>\d+)", RegexOptions.Compiled);
            if (match.Success)
            {
                string storage = match.Groups["storage"].Value;
                int index = int.Parse(match.Groups["index"].Value);
                return new BroadcastMessageStorageReferenceReceivedEventArgs(storage, index);
            }
            return default;
        }
    }
}
