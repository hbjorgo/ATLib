using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Extensions;
using System;
using System.Linq;

namespace HeboTech.ATLib.PDU
{
    internal class PhoneNumberDecoder
    {
        public static PhoneNumberDTO DecodePhoneNumber(ReadOnlySpan<byte> data)
        {
            byte ext_ton_npi = data[0];
            TypeOfNumber ton = (TypeOfNumber)((ext_ton_npi & 0b0111_0000) >> 4);

            string number = string.Empty;
            switch (ton)
            {
                case TypeOfNumber.Unknown:
                    break;
                case TypeOfNumber.International:
                    number = "+";
                    break;
                case TypeOfNumber.National:
                    break;
                case TypeOfNumber.NetworkSpecific:
                    break;
                case TypeOfNumber.Subscriber:
                    break;
                case TypeOfNumber.AlphaNumeric:
                    var decoded = Gsm7.Decode(data[1..].ToArray());
                    //var decoded = Gsm7.DecodeFromBytes(data[1..].ToArray());
                    return new PhoneNumberDTO(decoded);
                case TypeOfNumber.Abbreviated:
                    break;
                case TypeOfNumber.ReservedForExtension:
                    break;
                default:
                    throw new NotImplementedException($"TON {ton} is not supported");
            }

            number += string.Join("", data[1..].ToArray().Select(x => x.SwapNibbles().ToString("X2")));
            if (number[^1] == 'F')
                number = number[..^1];
            return new PhoneNumberDTO(number);
        }
    }
}
