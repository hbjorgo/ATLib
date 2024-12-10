using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Events
{
    public class BreadcastMessageStorageReferenceReceivedEventArgs
    {
        public BreadcastMessageStorageReferenceReceivedEventArgs(string storage, int index)
        {
            Storage = storage;
            Index = index;
        }

        public string Storage { get; }
        public int Index { get; }

        public static BreadcastMessageStorageReferenceReceivedEventArgs CreateFromResponse(string response)
        {
            var match = Regex.Match(response, @"\+CBMI:\s""(?<storage>[A-Z]+)"",(?<index>\d+)");
            if (match.Success)
            {
                string storage = match.Groups["storage"].Value;
                int index = int.Parse(match.Groups["index"].Value);
                return new BreadcastMessageStorageReferenceReceivedEventArgs(storage, index);
            }
            return default;
        }
    }
}
