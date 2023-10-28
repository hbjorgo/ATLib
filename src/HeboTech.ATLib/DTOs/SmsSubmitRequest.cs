﻿using HeboTech.ATLib.CodingSchemes;

namespace HeboTech.ATLib.DTOs
{
    public class SmsSubmitRequest
    {
        public SmsSubmitRequest(
            PhoneNumber phoneNumber,
            string message)
            : this(
                  phoneNumber,
                  message,
                  Gsm7.IsGsm7Compatible(message.ToCharArray()) ? CodingScheme.Gsm7 : CodingScheme.UCS2)
        {
        }

        public SmsSubmitRequest(
            PhoneNumber phoneNumber,
            string message,
            CodingScheme codingScheme)
            : this(
                  phoneNumber,
                  message,
                  codingScheme,
                  ValidityPeriod.NotPresent())
        {
        }

        public SmsSubmitRequest(
            PhoneNumber phoneNumber,
            string message,
            CodingScheme codingScheme,
            ValidityPeriod validityPeriod)
        {
            PhoneNumber = phoneNumber;
            Message = message;
            CodingScheme = codingScheme;
            ValidityPeriod = validityPeriod;
        }

        public PhoneNumber PhoneNumber { get; }
        public string Message { get; }
        public CodingScheme CodingScheme { get; }
        public bool IncludeEmptySmscLength { get; set; }
        public byte MessageReferenceNumber { get; set; }
        public ValidityPeriod ValidityPeriod { get; set; }
    }
}
