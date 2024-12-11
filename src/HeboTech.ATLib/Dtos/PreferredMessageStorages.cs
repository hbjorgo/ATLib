using System;

namespace HeboTech.ATLib.DTOs
{
    public class PreferredMessageStorages
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="storage1Name">Storage area to be used when reading or deleting SMS messages</param>
        /// <param name="storage2Name">Storage area to be used when sending SMS messages from message storage or writing SMS messages</param>
        /// <param name="storage3Name">Storage area to be used when storing newly received SMS messages</param>
        public PreferredMessageStorages(
            PreferredMessageStorage storage1Name,
            PreferredMessageStorage storage2Name,
            PreferredMessageStorage storage3Name)
        {
            Storage1Name = storage1Name;
            Storage2Name = storage2Name;
            Storage3Name = storage3Name;
        }

        public PreferredMessageStorage Storage1Name { get; }
        public PreferredMessageStorage Storage2Name { get; }
        public PreferredMessageStorage Storage3Name { get; }

        public override string ToString()
        {
            return
                $"Storage1: {Storage1Name}{Environment.NewLine}" +
                $"Storage2: {Storage2Name}{Environment.NewLine}" +
                $"Storage3: {Storage3Name}";
        }
    }
}
