﻿using System;

namespace HeboTech.ATLib.CodingSchemes
{
    internal static class CharacterSetHelpers
    {
        public static CharacterSet FromString(string characterSet)
        {
            if (characterSet == "GSM")
                return CharacterSet.Gsm7;
            else if (characterSet == "UCS2")
                return CharacterSet.UCS2;

            throw new ArgumentException("Unknown characterset");
        }

        public static string FromEnum(CharacterSet characterSet)
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
