﻿using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
using System;
using System.Collections.Generic;

namespace HeboTech.ATLib.Modems.Qualcomm
{
    public class MDM9225 : IMDM9225
    {
        private readonly AtChannel channel;
        private readonly _V_25TER _V_25TER;
        private readonly _3GPP_TS_27_005 _3GPP_TS_27_005;
        private readonly _3GPP_TS_27_007 _3GPP_TS_27_007;

        public event EventHandler<IncomingCallEventArgs> IncomingCall;
        public event EventHandler<MissedCallEventArgs> MissedCall;
        public event EventHandler<SmsReceivedEventArgs> SmsReceived;

        public MDM9225(AtChannel channel)
        {
            this.channel = channel;

            _V_25TER = new _V_25TER(channel);
            _3GPP_TS_27_005 = new _3GPP_TS_27_005(channel);
            _3GPP_TS_27_007 = new _3GPP_TS_27_007(channel);

            RegisterHandlers();
        }

        protected void RegisterHandlers()
        {
            SmsReceived += (s, e) => SmsReceived?.Invoke(this, e);
        }

        public virtual void Close()
        {
            channel.Close();
        }

        #region _V_25TER
        public CommandStatus DisableEcho() => _V_25TER.DisableEcho();
        public ProductIdentificationInformation GetProductIdentificationInformation() => _V_25TER.GetProductIdentificationInformation();

        #endregion

        #region _3GPP_TS_27_005
        public SmsReference SendSMS(PhoneNumber phoneNumber, string message) => _3GPP_TS_27_005.SendSMS(phoneNumber, message);
        public Sms ReadSMS(int index) => _3GPP_TS_27_005.ReadSMS(index);
        public IList<SmsWithIndex> ListSMSs(SmsStatus smsStatus) => _3GPP_TS_27_005.ListSMSs(smsStatus);
        public CommandStatus SetSmsMessageFormat(SmsTextFormat format) => _3GPP_TS_27_005.SetSmsMessageFormat(format);
        public CommandStatus DeleteSMS(int index) => _3GPP_TS_27_005.DeleteSMS(index);

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
