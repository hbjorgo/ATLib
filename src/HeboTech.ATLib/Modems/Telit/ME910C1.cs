using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.Telit
{
    internal class ME910C1 : ModemBase, IModem, IME910C1
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
            ModemResponse echo = await DisableEchoAsync();
            ModemResponse errorFormat = await SetErrorFormatAsync(1);
            return echo.Success && errorFormat.Success;
        }

        public override async Task<bool> SetRequiredSettingsAfterPinAsync()
        {
            ModemResponse currentCharacterSet = await SetCharacterSetAsync(CharacterSet.UCS2);
            ModemResponse smsMessageFormat = await SetSmsMessageFormatAsync(SmsTextFormat.PDU);
            return currentCharacterSet.Success && smsMessageFormat.Success;
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
