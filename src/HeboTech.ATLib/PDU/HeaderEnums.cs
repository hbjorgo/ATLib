namespace HeboTech.ATLib.PDU
{
    public enum MTI : byte
    {
        SMS_DELIVER_REPORT = 0x00,
        SMS_DELIVER = 0x00,
        SMS_SUBMIT = 0x01,
        SMS_SUBMIT_REPORT = 0x01,
        SMS_COMMAND = 0x10,
        SMS_STATUS_REPORT = 0x10,
        Reserved = 0x11
    }
}
