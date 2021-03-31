using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Modems.SIMCOM
{
    public class SIM5320 : ISIM5320
    {
        private readonly AtChannel channel;
        private readonly _V_25TER _V_25TER;
        private readonly _3GPP_TS_27_005 _3GPP_TS_27_005;
        private readonly _3GPP_TS_27_007 _3GPP_TS_27_007;

        public event EventHandler<IncomingCallEventArgs> IncomingCall;
        public event EventHandler<MissedCallEventArgs> MissedCall;
        public event EventHandler<SmsReceivedEventArgs> SmsReceived;

        public SIM5320(AtChannel channel)
        {
            this.channel = channel;

            _V_25TER = new _V_25TER(channel);
            _3GPP_TS_27_005 = new _3GPP_TS_27_005(channel);
            _3GPP_TS_27_007 = new _3GPP_TS_27_007(channel);

            RegisterHandlers();
        }

        protected void RegisterHandlers()
        {
            _V_25TER.IncomingCall += (s, e) => IncomingCall?.Invoke(this, e);
            _V_25TER.MissedCall += (s, e) => MissedCall?.Invoke(this, e);
            _3GPP_TS_27_005.SmsReceived += (s, e) => SmsReceived?.Invoke(this, e);
        }

        public virtual void Close()
        {
            channel.Close();
        }

        #region Custom
        public virtual RemainingPinPukAttempts GetRemainingPinPukAttempts()
        {
            var error = channel.SendSingleLineCommand("AT+SPIC", "+SPIC:", out AtResponse response);

            if (error == AtError.NO_ERROR)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+SPIC:\s(?<pin1>\d+),(?<pin2>\d+),(?<puk1>\d+),(?<puk2>\d+)");
                if (match.Success)
                {
                    int pin1 = int.Parse(match.Groups["pin1"].Value);
                    int pin2 = int.Parse(match.Groups["pin2"].Value);
                    int puk1 = int.Parse(match.Groups["puk1"].Value);
                    int puk2 = int.Parse(match.Groups["puk2"].Value);
                    return new RemainingPinPukAttempts(pin1, pin2, puk1, puk2);
                }
            }
            return null;
        }
        #endregion

        #region _V_25TER
        public CommandStatus AnswerIncomingCall() => _V_25TER.AnswerIncomingCall();
        public CallDetails Hangup() => _V_25TER.Hangup();
        public CommandStatus DisableEcho() => _V_25TER.DisableEcho();
        public ProductIdentificationInformation GetProductIdentificationInformation() => _V_25TER.GetProductIdentificationInformation();
        #endregion

        #region _3GPP_TS_27_005
        public CommandStatus SetSmsMessageFormat(SmsTextFormat format) => _3GPP_TS_27_005.SetSmsMessageFormat(format);
        public SmsReference SendSMS(PhoneNumber phoneNumber, string message) => _3GPP_TS_27_005.SendSMS(phoneNumber, message);
        public virtual CommandStatus DeleteSMS(int index) => _3GPP_TS_27_005.DeleteSMS(index);

        public virtual Sms ReadSMS(int index)
        {
            var error = channel.SendMultilineCommand($"AT+CMGR={index}", null, out AtResponse response);

            if (error == AtError.NO_ERROR && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CMGR:\s""(?<status>[A-Z\s]+)"",""(?<sender>\+?\d+)"",("""")?,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))""");
                if (match.Success)
                {
                    SmsStatus status = SmsStatusHelpers.ToSmsStatus(match.Groups["status"].Value);
                    PhoneNumber sender = new PhoneNumber(match.Groups["sender"].Value);
                    int year = int.Parse(match.Groups["year"].Value);
                    int month = int.Parse(match.Groups["month"].Value);
                    int day = int.Parse(match.Groups["day"].Value);
                    int hour = int.Parse(match.Groups["hour"].Value);
                    int minute = int.Parse(match.Groups["minute"].Value);
                    int second = int.Parse(match.Groups["second"].Value);
                    int zone = int.Parse(match.Groups["zone"].Value);
                    DateTimeOffset received = new DateTimeOffset(2000 + year, month, day, hour, minute, second, TimeSpan.FromMinutes(15 * zone));
                    string message = response.Intermediates.Last();
                    return new Sms(status, sender, received, message);
                }
            }
            return null;
        }

        public IList<SmsWithIndex> ListSMSs(SmsStatus smsStatus)
        {
            var error = channel.SendMultilineCommand($"AT+CMGL=\"{SmsStatusHelpers.ToString(smsStatus)}\"", null, out AtResponse response);

            List<SmsWithIndex> smss = new List<SmsWithIndex>();
            if (error == AtError.NO_ERROR)
            {
                for (int i = 0; i < response.Intermediates.Count; i += 2)
                {
                    string metaData = response.Intermediates[i];
                    var match = Regex.Match(metaData, @"\+CMGL:\s(?<index>\d+),""(?<status>[A-Z\s]+)"",""(?<sender>\+?\d+)"",("""")?,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))""");
                    if (match.Success)
                    {
                        int index = int.Parse(match.Groups["index"].Value);
                        SmsStatus status = SmsStatusHelpers.ToSmsStatus(match.Groups["status"].Value);
                        PhoneNumber sender = new PhoneNumber(match.Groups["sender"].Value);
                        int year = int.Parse(match.Groups["year"].Value);
                        int month = int.Parse(match.Groups["month"].Value);
                        int day = int.Parse(match.Groups["day"].Value);
                        int hour = int.Parse(match.Groups["hour"].Value);
                        int minute = int.Parse(match.Groups["minute"].Value);
                        int second = int.Parse(match.Groups["second"].Value);
                        int zone = int.Parse(match.Groups["zone"].Value);
                        DateTimeOffset received = new DateTimeOffset(2000 + year, month, day, hour, minute, second, TimeSpan.FromMinutes(15 * zone));
                        string message = response.Intermediates[i + 1];
                        smss.Add(new SmsWithIndex(index, status, sender, received, message));
                    }
                }
            }
            return smss;
        }
        #endregion

        #region _3GPP_TS_27_007
        public SimStatus GetSimStatus() => _3GPP_TS_27_007.GetSimStatus();

        public CommandStatus EnterSimPin(PersonalIdentificationNumber pin) => _3GPP_TS_27_007.EnterSimPin(pin);

        public BatteryStatus GetBatteryStatus() => _3GPP_TS_27_007.GetBatteryStatus();

        public SignalStrength GetSignalStrength() => _3GPP_TS_27_007.GetSignalStrength();

        public CommandStatus SetDateTime(DateTimeOffset value) => _3GPP_TS_27_007.SetDateTime(value);

        public DateTimeOffset? GetDateTime() => _3GPP_TS_27_007.GetDateTime();
        #endregion
    }
}
