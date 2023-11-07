using System;

namespace HeboTech.ATLib.CodingSchemes
{
    internal class DataCodingScheme
    {
        private static readonly DataCodingScheme[] dataCodingSchemes = new DataCodingScheme[0xFF];

        static DataCodingScheme()
        {
            dataCodingSchemes[0x00] = new DataCodingScheme(CharacterSet.Gsm7, MessageClass.Default, CodingGroup.GeneralDataCoding);
            dataCodingSchemes[0x08] = new DataCodingScheme(CharacterSet.UCS2, MessageClass.Default, CodingGroup.GeneralDataCoding);
            dataCodingSchemes[0x11] = new DataCodingScheme(CharacterSet.Gsm7, MessageClass.Class1, CodingGroup.GeneralDataCoding);
        }

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
            return dataCodingSchemes[value] ?? throw new NotImplementedException($"Data Coding Scheme no. {value} is not supported");
        }
    }
}
