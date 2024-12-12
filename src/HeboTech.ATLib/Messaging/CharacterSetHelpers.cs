using System;

namespace HeboTech.ATLib.Messaging
{
    public static class CharacterSetHelpers
    {
        public static CharacterSet FromString(string characterSet)
        {
            if (characterSet == "GSM")
                return CharacterSet.Gsm7;
            else if (characterSet == "UCS2")
                return CharacterSet.UCS2;

            throw new ArgumentException("Unknown characterset");
        }

        public static string ToString(CharacterSet characterSet)
        {
            return characterSet switch
            {
                CharacterSet.Gsm7 => "GSM",
                CharacterSet.UCS2 => "UCS2",
                _ => throw new ArgumentException("Unknown characterset"),
            };
        }
    }
}
