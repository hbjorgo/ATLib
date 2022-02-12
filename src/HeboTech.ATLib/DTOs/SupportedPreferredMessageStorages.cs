using System;
using System.Collections.Generic;

namespace HeboTech.ATLib.DTOs
{
    public class SupportedPreferredMessageStorages
    {
        public SupportedPreferredMessageStorages(IEnumerable<string> storage1, IEnumerable<string> storage2, IEnumerable<string> storage3)
        {
            Storage1 = storage1 ?? new string[0];
            Storage2 = storage2 ?? new string[0];
            Storage3 = storage3 ?? new string[0];
        }

        public IEnumerable<string> Storage1 { get; }
        public IEnumerable<string> Storage2 { get; }
        public IEnumerable<string> Storage3 { get; }

        public override string ToString()
        {
            return
                $"Storage1: {string.Join(',', Storage1)}{Environment.NewLine}" +
                $"Storage2: {string.Join(',', Storage2)}{Environment.NewLine}" +
                $"Storage3: {string.Join(',', Storage3)}";
        }
    }
}
