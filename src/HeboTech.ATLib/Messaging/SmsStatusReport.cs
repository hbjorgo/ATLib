using HeboTech.ATLib.Numbering;
using System;

namespace HeboTech.ATLib.Messaging
{
    public class SmsStatusReport : Sms
    {
        public SmsStatusReport(int messageReference, PhoneNumber recipientAddress, PhoneNumber serviceCenterAddress, DateTimeOffset serviceCenterTimestamp, DateTimeOffset dischargeTime, SmsDeliveryStatus status)
            : base(MessageTypeIndicatorInbound.SMS_STATUS_REPORT, messageReference)
        {
            RecipientAddress = recipientAddress;
            ServiceCenterAddress = serviceCenterAddress;
            ServiceCenterTimestamp = serviceCenterTimestamp;
            DischargeTime = dischargeTime;
            Status = status;
        }

        public PhoneNumber RecipientAddress { get; }
        public PhoneNumber ServiceCenterAddress { get; }
        public DateTimeOffset ServiceCenterTimestamp { get; }
        public DateTimeOffset DischargeTime { get; }
        public SmsDeliveryStatus Status { get; }

        public override string ToString()
        {
            return base.ToString() + $" Delivered with status {Status}. RA: {RecipientAddress}. SCTS: {ServiceCenterTimestamp}. DT: {DischargeTime}.";
        }
    }
}
