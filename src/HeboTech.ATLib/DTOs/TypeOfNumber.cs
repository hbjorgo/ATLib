namespace HeboTech.ATLib.DTOs
{
    public enum TypeOfNumber : byte
    {
        Unknown = 0x00,
        International = 0x01,
        National = 0x02,
        NetworkSpecific = 0x03,
        Subscriber = 0x04,
        AlphaNumeric = 0x05,
        Abbreviated = 0x06,
        ReservedForExtension = 0x07
    }
}
