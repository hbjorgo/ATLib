using HeboTech.ATLib.Modems.SIMCOM;
using HeboTech.ATLib.Parsing;
using HeboTech.ATLib.Messaging;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.Adafruit
{
    public class Fona3G : SIM5320, IModem, IFona3G
    {
        /// <summary>
        /// Based on SIMCOM SIM5320 chipset
        /// 
        /// Serial port settings:
        /// 9600 8N1 Handshake.None
        /// </summary>
        public Fona3G(IAtChannel channel)
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
    }
}
