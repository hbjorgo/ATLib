namespace HeboTech.ATLib.PDU
{
    internal enum MessageTypeIndicator : byte
    {
        // MS -> SC
        SMS_DELIVER = 0x00,
        // SC -> MS
        SMS_DELIVER_REPORT = 0x00,
        // MS -> SC
        SMS_SUBMIT = 0x01,
        // SC -> MS
        SMS_SUBMIT_REPORT = 0x01,
        // MS -> SC
        SMS_COMMAND = 0x02,
        // SC -> MS
        SMS_STATUS_REPORT = 0x02,
        Reserved = 0x03
    }
}
