using System;

namespace HeboTech.ATLib.Messaging
{
    internal class DataCodingScheme
    {
        private static readonly DataCodingScheme dcs_0x00 = new DataCodingScheme(CharacterSet.Gsm7, MessageClass.Default, CodingGroup.GeneralDataCoding);
        private static readonly DataCodingScheme dcs_0x08 = new DataCodingScheme(CharacterSet.UCS2, MessageClass.Default, CodingGroup.GeneralDataCoding);
        private static readonly DataCodingScheme dcs_0x11 = new DataCodingScheme(CharacterSet.Gsm7, MessageClass.Class1, CodingGroup.GeneralDataCoding);

        protected DataCodingScheme(CharacterSet characterSet, MessageClass messageClass, CodingGroup codingGroup)
        {
            CharacterSet = characterSet;
            MessageClass = messageClass;
            CodingGroup = codingGroup;
        }

        public CharacterSet CharacterSet { get; }
        public MessageClass MessageClass { get; }
        public CodingGroup CodingGroup { get; }

        public static DataCodingScheme ParseByte(byte value)
        {
            return value switch
            {
                0x00 => dcs_0x00,
                0x08 => dcs_0x08,
                0x11 => dcs_0x11,
                _ => throw new ArgumentException($"Data Coding Scheme no. {value} is not supported"),
            };
        }
    }
}
