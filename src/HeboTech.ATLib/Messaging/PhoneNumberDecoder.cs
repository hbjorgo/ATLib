using HeboTech.ATLib.Extensions;
using HeboTech.ATLib.Numbering;
using System;
using System.Linq;

namespace HeboTech.ATLib.Messaging
{
    internal class PhoneNumberDecoder
    {
        public static PhoneNumber DecodePhoneNumber(ReadOnlySpan<byte> data)
        {
            byte ext_ton_npi = data[0];
            TypeOfNumber ton = (TypeOfNumber)((ext_ton_npi & 0b0111_0000) >> 4);
            NumberingPlanIdentification npi = (NumberingPlanIdentification)(ext_ton_npi & 0b0000_1111);

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
                    return new AlphaNumericPhoneNumber(decoded, npi);
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

            return ton switch
            {
                TypeOfNumber.Unknown => new UnknownPhoneNumber(number, npi),
                TypeOfNumber.International => new InternationalPhoneNumber(number, npi),
                TypeOfNumber.National => new NationalPhoneNumber(number, npi),
                TypeOfNumber.NetworkSpecific => new NetworkSpecificPhoneNumber(number, npi),
                TypeOfNumber.Subscriber => new SubscriberPhoneNumber(number, npi),
                TypeOfNumber.AlphaNumeric => new AlphaNumericPhoneNumber(number, npi),
                TypeOfNumber.Abbreviated => new AbbreviatedPhoneNumber(number, npi),
                TypeOfNumber.ReservedForExtension => throw new NotSupportedException("Number type not supported"),
                _ => throw new NotSupportedException("Number type not supported")
            };
        }
    }
}
