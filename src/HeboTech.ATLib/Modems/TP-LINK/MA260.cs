﻿using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
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
        public MA260(IAtChannel channel)
            : base(channel)
        {
        }

        public Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsAsync(PhoneNumber phoneNumber, string message)
        {
            return base.SendSmsAsync(phoneNumber, message, false);
        }

        public Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsAsync(PhoneNumber phoneNumber, string message, CharacterSet codingScheme)
        {
            return base.SendSmsAsync(phoneNumber, message, codingScheme, false);
        }
    }
}
