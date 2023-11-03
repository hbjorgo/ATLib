using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Extensions;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.PDU;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnitsNet;
using UnitsNet.Units;

namespace HeboTech.ATLib.Modems.Cinterion
{
    public class MC55i : ModemBase, IModem
    {
        /// <summary>
        /// Cinterion MC55i chipset
        /// 
        /// Serial port settings:
        /// 9600 8N1 Handshake.None
        /// </summary>
        public MC55i(AtChannel channel)
            : base(channel)
        {
        }

        protected async Task SetSmsTextModeParameters(MessageTypeIndicator mti, ValidityPeriod vp, CodingScheme dcs)
        {
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CSMP=\"{(byte)mti}\",\"{0}\",\"{(byte)dcs}\"", "+CSMP:");

            if (response.Success && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.First();
                //var match = Regex.Match(line, @"\+CSMP:\s(?<s1Used>\d+),(?<s1Total>\d+),(?<s2Used>\d+),(?<s2Total>\d+),(?<s3Used>\d+),(?<s3Total>\d+)");
                //if (match.Success)
                //{
                //    int s1Used = int.Parse(match.Groups["s1Used"].Value);
                //    int s1Total = int.Parse(match.Groups["s1Total"].Value);
                //    int s2Used = int.Parse(match.Groups["s2Used"].Value);
                //    int s2Total = int.Parse(match.Groups["s2Total"].Value);
                //    int s3Used = int.Parse(match.Groups["s3Used"].Value);
                //    int s3Total = int.Parse(match.Groups["s3Total"].Value);

                //    return ModemResponse.ResultSuccess(new PreferredMessageStorages(
                //        new PreferredMessageStorage(storage1Name, s1Used, s1Total),
                //        new PreferredMessageStorage(storage2Name, s2Used, s2Total),
                //        new PreferredMessageStorage(storage3Name, s3Used, s3Total)));
                //}
            }
            //return ModemResponse.ResultError<PreferredMessageStorages>();
            return;
        }

        public override async Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message)
        {
            //await SetSmsTextModeParameters(MessageTypeIndicator.SMS_SUBMIT, ValidityPeriod.NotPresent(), CodingScheme.UCS2);
            return await base.SendSmsInPduFormatAsync(phoneNumber, message, false);
        }

        public override async Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message, CodingScheme codingScheme)
        {
            //await SetSmsTextModeParameters(MessageTypeIndicator.SMS_SUBMIT, ValidityPeriod.NotPresent(), CodingScheme.UCS2);
            return await base.SendSmsInPduFormatAsync(phoneNumber, message, codingScheme, false);
        }

        public override async Task<ModemResponse<BatteryStatus>> GetBatteryStatusAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT^SBC?", "^SBC:");

            if (response.Success)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\^SBC:\s(?<bcs>\d+),(?<bcl>\d+),(?<mpc>\d+)");
                if (match.Success)
                {
                    int bcs = int.Parse(match.Groups["bcs"].Value);
                    int bcl = int.Parse(match.Groups["bcl"].Value);
                    int mpc = int.Parse(match.Groups["mpc"].Value);
                    return ModemResponse.ResultSuccess(new BatteryStatus((BatteryChargeStatus)bcs, Ratio.FromPercent(bcl)));
                }
            }
            return ModemResponse.ResultError<BatteryStatus>();
        }

        public async Task<ModemResponse<MC55iBatteryStatus>> MC55i_GetBatteryStatusAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT^SBC?", "^SBC:");

            if (response.Success)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\^SBC:\s(?<bcs>\d+),(?<bcl>\d+),(?<mpc>\d+)");
                if (match.Success)
                {
                    int bcs = int.Parse(match.Groups["bcs"].Value);
                    int bcl = int.Parse(match.Groups["bcl"].Value);
                    int mpc = int.Parse(match.Groups["mpc"].Value);
                    return ModemResponse.ResultSuccess(new MC55iBatteryStatus(new ElectricCurrent(mpc, ElectricCurrentUnit.Milliampere)));
                }
            }
            return ModemResponse.ResultError<MC55iBatteryStatus>();
        }

        public class MC55iBatteryStatus
        {
            public MC55iBatteryStatus(ElectricCurrent powerConsumption)
            {
                PowerConsumption = powerConsumption;
            }

            public ElectricCurrent PowerConsumption { get; }

            public override string ToString()
            {
                return $"Average power consumption: {PowerConsumption}";
            }
        }
    }
}
