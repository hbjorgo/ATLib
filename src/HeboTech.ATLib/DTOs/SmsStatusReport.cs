using System;

namespace HeboTech.ATLib.DTOs
{
    public class SmsStatusReport
    {
        public SmsStatusReport(int messageReference, PhoneNumberDTO recipientAddress, DateTimeOffset serviceCenterTimestamp, DateTimeOffset dischargeTime, SmsDeliveryStatus status)
        {
            MessageReference = messageReference;
            RecipientAddress = recipientAddress;
            ServiceCenterTimestamp = serviceCenterTimestamp;
            DischargeTime = dischargeTime;
            Status = status;
        }

        public int MessageReference { get; }
        public PhoneNumberDTO RecipientAddress { get; }
        public DateTimeOffset ServiceCenterTimestamp { get; }
        public DateTimeOffset DischargeTime { get; }
        public SmsDeliveryStatus Status { get; }

        public override string ToString()
        {
            return $"SMS no. {MessageReference} delivered with status {Status}. RA: {RecipientAddress}. SCTS: {ServiceCenterTimestamp}. DT: {DischargeTime}.";
        }
    }
}
