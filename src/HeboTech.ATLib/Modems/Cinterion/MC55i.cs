﻿using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Extensions;
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
        /// </summary>
        public MC55i(IAtChannel channel)
            : base(channel)
        {
        }

        public override async Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message)
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

        public override async Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message, CodingScheme codingScheme)
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

        //public override async Task<ModemResponse<Sms>> ReadSmsAsync(int index, SmsTextFormat smsTextFormat)
        //{
        //    switch (smsTextFormat)
        //    {
        //        case SmsTextFormat.PDU:
        //            AtResponse pduResponse = await channel.SendMultilineCommand($"AT+CMGR={index}", null);

        //            if (pduResponse.Success)
        //            {
        //                string line1 = pduResponse.Intermediates[0];
        //                var line1Match = Regex.Match(line1, @"\+CMGR:\s(?<status>\d),(""(?<alpha>\w*)"")*,(?<length>\d+)");
        //                string line2 = pduResponse.Intermediates[1];
        //                var line2Match = Regex.Match(line2, @"(?<status>[0-9A-Z]*)");
        //                if (line1Match.Success && line2Match.Success)
        //                {
        //                    int statusCode = int.Parse(line1Match.Groups["status"].Value);
        //                    SmsStatus status = (SmsStatus)statusCode;

        //                    string pdu = line2Match.Groups["status"].Value;
        //                    SmsDeliver pduMessage = SmsDeliverDecoder.Decode(pdu.ToByteArray());

        //                    return ModemResponse.IsResultSuccess(new Sms(status, pduMessage.SenderNumber, pduMessage.Timestamp, pduMessage.Message));
        //                }
        //                if (AtErrorParsers.TryGetError(pduResponse.FinalResponse, out Error pduError))
        //                    return ModemResponse.HasResultError<Sms>(pduError);
        //            }
        //            break;
        //        case SmsTextFormat.Text:
        //            AtResponse textResponse = await channel.SendMultilineCommand($"AT+CMGR={index}", null);

        //            if (textResponse.Success && textResponse.Intermediates.Count > 0)
        //            {
        //                string line = textResponse.Intermediates.First();
        //                var match = Regex.Match(line, @"\+CMGR:\s""(?<status>[A-Z\s]+)"",""(?<sender>\+\d+)"",,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))"",(?<addressType>\d+),(?<tpduFirstOctet>\d),(?<pid>\d),(?<dcs>\d),(?<serviceCenterAddress>""\+\d+""),(?<serviceCenterAddressType>\d+),(?<length>\d+)");
        //                if (match.Success)
        //                {
        //                    SmsStatus status = SmsStatusHelpers.ToSmsStatus(match.Groups["status"].Value);
        //                    PhoneNumberDTO sender = new PhoneNumberDTO(match.Groups["sender"].Value);
        //                    int year = int.Parse(match.Groups["year"].Value);
        //                    int month = int.Parse(match.Groups["month"].Value);
        //                    int day = int.Parse(match.Groups["day"].Value);
        //                    int hour = int.Parse(match.Groups["hour"].Value);
        //                    int minute = int.Parse(match.Groups["minute"].Value);
        //                    int second = int.Parse(match.Groups["second"].Value);
        //                    int zone = int.Parse(match.Groups["zone"].Value);

        //                    int addressType = int.Parse(match.Groups["addressType"].Value);
        //                    int tpduFirstOctet = int.Parse(match.Groups["tpduFirstOctet"].Value);
        //                    int protocolIdentifier = int.Parse(match.Groups["pid"].Value);
        //                    int dataCodingScheme = int.Parse(match.Groups["dcs"].Value);
        //                    string serviceCenterAddress = match.Groups["serviceCenterAddress"].Value;
        //                    int serviceCenterAddressType = int.Parse(match.Groups["serviceCenterAddressType"].Value);
        //                    int length = int.Parse(match.Groups["length"].Value);

        //                    DateTimeOffset received = new DateTimeOffset(2000 + year, month, day, hour, minute, second, TimeSpan.FromMinutes(15 * zone));
        //                    string message = textResponse.Intermediates.Last();

        //                    CodingScheme dcs = (CodingScheme)dataCodingScheme;
        //                    if (dcs == CodingScheme.UCS2)
        //                        message = UCS2.Decode(message);

        //                    return ModemResponse.IsResultSuccess(new Sms(status, sender, received, message));
        //                }
        //            }
        //            if (AtErrorParsers.TryGetError(textResponse.FinalResponse, out Error textError))
        //                return ModemResponse.HasResultError<Sms>(textError);
        //            break;
        //        default:
        //            throw new NotSupportedException("The format is not supported");
        //    }
        //    return ModemResponse.HasResultError<Sms>();
        //}

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
            return ModemResponse.HasResultError<BatteryStatus>();
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
            return ModemResponse.HasResultError<MC55iBatteryStatus>();
        }
    }
}
