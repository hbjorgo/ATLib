using HeboTech.ATLib.PDU;
using System;

namespace HeboTech.ATLib.DTOs
{
    public class SmsStatusReport : Sms
    {
        public SmsStatusReport(int messageReference, PhoneNumberDTO recipientAddress, PhoneNumberDTO serviceCenterAddress, DateTimeOffset serviceCenterTimestamp, DateTimeOffset dischargeTime, SmsDeliveryStatus status)
            : base(MessageTypeIndicatorInbound.SMS_STATUS_REPORT, messageReference)
        {
            RecipientAddress = recipientAddress;
            ServiceCenterAddress = serviceCenterAddress;
            ServiceCenterTimestamp = serviceCenterTimestamp;
            DischargeTime = dischargeTime;
            Status = status;
        }

        public PhoneNumberDTO RecipientAddress { get; }
        public PhoneNumberDTO ServiceCenterAddress { get; }
        public DateTimeOffset ServiceCenterTimestamp { get; }
        public DateTimeOffset DischargeTime { get; }
        public SmsDeliveryStatus Status { get; }

        public override string ToString()
        {
            return base.ToString() + $" Delivered with status {Status}. RA: {RecipientAddress}. SCTS: {ServiceCenterTimestamp}. DT: {DischargeTime}.";
        }
    }
}
