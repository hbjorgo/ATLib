using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeboTech.ATLib.CodingSchemes
{
    /// <summary>
    /// Encode / decode GSM 7-bit strings (GSM 03.38 or 3GPP 23.038)
    /// </summary>
    public static class Gsm7
    {
        public const CodingScheme DataCodingSchemeCode = CodingScheme.Gsm7;

        // ` is not a conversion, just a untranslatable letter
        private const string strGSMTable = "@£$¥èéùìòÇ`Øø`ÅåΔ_ΦΓΛΩΠΨΣΘΞ`ÆæßÉ !\"#¤%&'()*=,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ`¿abcdefghijklmnopqrstuvwxyzäöñüà";
        private const string strExtendedTable = "````````````````````^```````````````````{}`````\\````````````[~]`|````````````````````````````````````€``````````````````````````";

        public static byte[] Pack(byte[] data, int paddingBits = 0)
        {
            // Array for all packed bits (n x 7)
            BitArray packedBits = new BitArray((int)Math.Ceiling(data.Length * 7 / 8.0) * 8 + paddingBits);
            
            // Loop through all characters
            for (int i = 0; i < data.Length; i++)
            {
                // Only 7 bits in each byte is data
                for (int j = 0; j < 7; j++)
                {
                    // For each 7 bits in each byte, add it to the bit array
                    int index = (i * 7) + j + paddingBits;
                    bool isSet = (data[i] & (1 << j)) != 0;
                    packedBits.Set(index, isSet);
                }
            }

            // Convert the bit array to a byte array
            byte[] packed = new byte[(int)Math.Ceiling(packedBits.Length / 8.0)];
            packedBits.CopyTo(packed, 0);

            // Return the septets packed as octets
            // If the last character is empty - skip it
            //if (packed[^1] == 0)
            //    return packed[..^1];
            return packed;
        }

        public static byte[] Unpack(byte[] data)
        {
            BitArray packedBits = new BitArray(data);
            byte[] unpacked = new byte[packedBits.Length / 7];

            byte value = 0;
            for (int i = 0; i < unpacked.Length * 7; i += 7)
            {
                for (int j = 0; j < 7; j++)
                {
                    value |= packedBits[i + j] ? (byte)(1 << j) : (byte)(0 << j);
                }
                unpacked[i / 7] = value;
                value = 0;
            }

            // If the last character is empty - skip it.
            // It means that one bit of the last octet was used by the last character and the last 7 bits weren't used
            if (unpacked[^1] == 0)
                return unpacked[..^1];
            return unpacked;
        }

        public static byte[] EncodeToBytes(string text)
        {
            return EncodeToBytes(text.ToCharArray());
        }

        public static bool IsGsm7Compatible(IEnumerable<char> text)
        {
            for (int i = 0; i < text.Count(); i++)
            {
                char c = text.ElementAt(i);

                int intGSMTable = strGSMTable.IndexOf(c);
                if (intGSMTable != -1)
                    continue;

                int intExtendedTable = strExtendedTable.IndexOf(c);
                if (intExtendedTable == -1)
                    return false;
            }

            return true;
        }

        public static byte[] EncodeToBytes(IEnumerable<char> text)
        {
            List<byte> byteGSMOutput = new List<byte>();

            for (int i = 0; i < text.Count(); i++)
            {
                char c = text.ElementAt(i);

                int intGSMTable = strGSMTable.IndexOf(c);
                if (intGSMTable != -1)
                {
                    byteGSMOutput.Add((byte)intGSMTable);
                    continue;
                }

                int intExtendedTable = strExtendedTable.IndexOf(c);
                if (intExtendedTable != -1)
                {
                    byteGSMOutput.Add(27);
                    byteGSMOutput.Add((byte)intExtendedTable);
                }
            }

            return byteGSMOutput.ToArray();
        }
    }
}
