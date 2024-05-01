using HeboTech.ATLib.PDU;
using System;

namespace HeboTech.ATLib.DTOs
{
    public class SmsStatusReport : SmsBase
    {
        public SmsStatusReport(int messageReference, PhoneNumberDTO recipientAddress, DateTimeOffset serviceCenterTimestamp, DateTimeOffset dischargeTime, SmsDeliveryStatus status)
            : base(MessageTypeIndicatorInbound.SMS_STATUS_REPORT, messageReference)
        {
            RecipientAddress = recipientAddress;
            ServiceCenterTimestamp = serviceCenterTimestamp;
            DischargeTime = dischargeTime;
            Status = status;
        }

        public PhoneNumberDTO RecipientAddress { get; }
        public DateTimeOffset ServiceCenterTimestamp { get; }
        public DateTimeOffset DischargeTime { get; }
        public SmsDeliveryStatus Status { get; }

        public override string ToString()
        {
            return base.ToString() + $" Delivered with status {Status}. RA: {RecipientAddress}. SCTS: {ServiceCenterTimestamp}. DT: {DischargeTime}.";
        }
    }
}
