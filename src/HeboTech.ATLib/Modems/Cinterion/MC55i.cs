﻿using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.PDU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnitsNet;
using UnitsNet.Units;

namespace HeboTech.ATLib.Modems.Cinterion
{
    public class MC55i : ModemBase, IModem, IMC55i
    {
        /// <summary>
        /// Cinterion MC55i chipset
        /// 
        /// Serial port settings:
        /// 115200 8N1 Handshake.None
        /// </summary>h
        public MC55i(IAtChannel channel)
            : base(channel)
        {
        }

        public async Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsAsync(PhoneNumber phoneNumber, string message)
        {
            if (phoneNumber is null)
                throw new ArgumentNullException(nameof(phoneNumber));
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            IEnumerable<string> pdus = SmsSubmitEncoder.Encode(new SmsSubmitRequest(phoneNumber, message) { IncludeEmptySmscLength = true });
            List<ModemResponse<SmsReference>> references = new List<ModemResponse<SmsReference>>();
            foreach (string pdu in pdus)
            {
                string cmd1 = $"AT+CMGS={(pdu.Length - 2) / 2}"; // Subtract 2 (one octet) for SMSC.
                string cmd2 = pdu;
                AtResponse response = await channel.SendSmsAsync(cmd1, cmd2, "+CMGS:", TimeSpan.FromSeconds(30));

                if (response.Success)
                {
                    string line = response.Intermediates.First();
                    var match = Regex.Match(line, @"\+CMGS:\s(?<mr>\d+)");
                    if (match.Success)
                    {
                        int mr = int.Parse(match.Groups["mr"].Value);
                        references.Add(ModemResponse.IsResultSuccess(new SmsReference(mr)));
                    }
                }
                else
                {
                    if (AtErrorParsers.TryGetError(response.FinalResponse, out Error error))
                        references.Add(ModemResponse.HasResultError<SmsReference>(error));
                }
            }
            return references;
        }

        public async Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsAsync(PhoneNumber phoneNumber, string message, CharacterSet codingScheme)
        {
            if (phoneNumber is null)
                throw new ArgumentNullException(nameof(phoneNumber));
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            IEnumerable<string> pdus = SmsSubmitEncoder.Encode(new SmsSubmitRequest(phoneNumber, message, codingScheme) { IncludeEmptySmscLength = true });
            List<ModemResponse<SmsReference>> references = new List<ModemResponse<SmsReference>>();
            foreach (string pdu in pdus)
            {
                string cmd1 = $"AT+CMGS={(pdu.Length - 2) / 2}"; // Subtract 2 (one octet) for SMSC.
                string cmd2 = pdu;
                AtResponse response = await channel.SendSmsAsync(cmd1, cmd2, "+CMGS:", TimeSpan.FromSeconds(30));

                if (response.Success)
                {
                    string line = response.Intermediates.First();
                    var match = Regex.Match(line, @"\+CMGS:\s(?<mr>\d+)");
                    if (match.Success)
                    {
                        int mr = int.Parse(match.Groups["mr"].Value);
                        references.Add(ModemResponse.IsResultSuccess(new SmsReference(mr)));
                    }
                }
                else
                {
                    if (AtErrorParsers.TryGetError(response.FinalResponse, out Error error))
                        references.Add(ModemResponse.HasResultError<SmsReference>(error));
                }
            }
            return references;
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
                    return ModemResponse.IsResultSuccess(new BatteryStatus((BatteryChargeStatus)bcs, Ratio.FromPercent(bcl)));
                }
            }
            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<BatteryStatus>(error);
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
                    return ModemResponse.IsResultSuccess(new MC55iBatteryStatus(new ElectricCurrent(mpc, ElectricCurrentUnit.Milliampere)));
                }
            }

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<MC55iBatteryStatus>(error);
        }
    }
}
