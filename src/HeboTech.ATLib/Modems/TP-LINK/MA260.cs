﻿using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.TP_LINK
{
    public class MA260 : ModemBase, IModem, IMA260
    {
        /// <summary>
        /// Based on some Qualcomm chipset
        /// 
        /// Serial port settings:
        /// 9600 8N1 Handshake.RequestToSend
        /// </summary>
        public MA260(AtChannel channel)
            : base(channel)
        {
        }

        public override Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message)
        {
            return base.SendSmsInPduFormatAsync(phoneNumber, message, false);
        }

        public override Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message, CodingScheme codingScheme)
        {
            return base.SendSmsInPduFormatAsync(phoneNumber, message, codingScheme, false);
        }
    }
}
