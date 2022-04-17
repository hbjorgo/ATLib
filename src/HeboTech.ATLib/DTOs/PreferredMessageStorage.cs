using System;

namespace HeboTech.ATLib.DTOs
{
    public class PreferredMessageStorages
    {
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

    public class PreferredMessageStorage
    {
        public PreferredMessageStorage(string storage1Name, int storage1Messages, int storage1MessageLocations)
        {
            Storage1Name = storage1Name;
            Storage1Messages = storage1Messages;
            Storage1MessageLocations = storage1MessageLocations;
        }

        public string Storage1Name { get; }
        public int Storage1Messages { get; }
        public int Storage1MessageLocations { get; }

        public override string ToString()
        {
            return $"Name:{Storage1Name}, Messages:{Storage1Messages}, Locations:{Storage1MessageLocations}";
        }
    }
}
