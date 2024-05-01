namespace HeboTech.ATLib.PDU
{
    /// <summary>
    /// Inbound (SMSC/SC -> MS)
    /// SMSC: Short Message Service Center
    /// SC: SMS Center
    /// MS: Mobile Station
    /// </summary>
    public enum MessageTypeIndicatorInbound : byte
    {
        // SC -> MS
        SMS_DELIVER = 0x00,
        // SC -> MS
        SMS_SUBMIT_REPORT = 0x01,
        // SC -> MS
        SMS_STATUS_REPORT = 0x02,

        Reserved = 0x03
    }

    /// <summary>
    /// Outbound (MS -> SMSC/SC)
    /// SMSC: Short Message Service Center
    /// SC: SMS Center
    /// MS: Mobile Station
    /// </summary>
    internal enum MessageTypeIndicatorOutbound : byte
    {
        // MS -> SC
        SMS_DELIVER_REPORT = 0x00,
        // MS -> SC
        SMS_SUBMIT = 0x01,
        // MS -> SC
        SMS_COMMAND = 0x02,

        Reserved = 0x03
    }
}
