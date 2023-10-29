using HeboTech.ATLib.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace HeboTech.ATLib.PDU
{
    public class Pdu
    {
        public static IEnumerable<string> EncodeSmsSubmit(SmsSubmitRequest smsSubmit)
        {
            // Build TPDU
            var messageParts = SmsSubmitBuilder
                                    .Initialize()
                                    .DestinationAddress(smsSubmit.PhoneNumber)
                                    .ValidityPeriod(smsSubmit.ValidityPeriod)
                                    .DataCodingScheme(smsSubmit.CodingScheme)
                                    .Message(smsSubmit.Message)
                                    .MessageReferenceNumber(smsSubmit.MessageReferenceNumber)
                                    .Build();

            foreach (var messagePart in messageParts)
            {
                StringBuilder sb = new StringBuilder();

                // Length of SMSC information
                if (smsSubmit.IncludeEmptySmscLength)
                    sb.Append("00");

                sb.Append(messagePart);

                yield return sb.ToString();
            }
        }

        public static SmsDeliver DecodeSmsDeliver(ReadOnlySpan<char> text, int timestampYearOffset = 2000)
        {
            return SmsDeliverDecoder.DecodeSmsDeliver(text, timestampYearOffset);
        }
    }
}
