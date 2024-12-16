﻿using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsing;
using HeboTech.ATLib.Messaging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.Telit
{
    public class ME910C1 : ModemBase, IModem, IME910C1
    {
        /// <summary>
        /// Telit ME910C1 chipset
        /// </summary>h
        public ME910C1(IAtChannel channel)
            : base(channel)
        {
        }

        public override async Task<bool> SetRequiredSettingsBeforePinAsync()
        {
            ModemResponse echo = await DisableEchoAsync().ConfigureAwait(false);
            ModemResponse errorFormat = await SetErrorFormatAsync(1).ConfigureAwait(false);
            return echo.Success && errorFormat.Success;
        }

        public override async Task<bool> SetRequiredSettingsAfterPinAsync()
        {
            ModemResponse currentCharacterSet = await SetCharacterSetAsync(CharacterSet.UCS2).ConfigureAwait(false);
            ModemResponse smsMessageFormat = await SetSmsMessageFormatAsync(SmsTextFormat.PDU).ConfigureAwait(false);
            return currentCharacterSet.Success && smsMessageFormat.Success;
        }

        public override Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsAsync(SmsSubmitRequest request)
        {
            return SendSmsAsync(request, false);
        }
    }
}