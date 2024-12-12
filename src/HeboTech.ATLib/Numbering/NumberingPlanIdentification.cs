namespace HeboTech.ATLib.Numbering
{
    public enum NumberingPlanIdentification : byte
    {
        Unknown = 0x00,
        ISDN = 0x01,
        DataNumbering = 0x03,
        Telex = 0x04,
        ServiceCentreSpecific1 = 0x05,
        ServiceCentreSpecific2 = 0x06,
        NationalNumbering = 0x08,
        PrivateNumbering = 0x09,
        ErmesNumbering = 0x0A,
        ReservedForExtension = 0x0F
    }
}
