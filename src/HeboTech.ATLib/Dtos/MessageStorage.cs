using System;

namespace HeboTech.ATLib.Dtos
{
    /// <summary>
    /// Message Storage Areas
    /// </summary>
    public class MessageStorage
    {
        /// <summary>
        /// SIM card storage area 
        /// </summary>
        public static readonly MessageStorage SM = new MessageStorage("SM");
        /// <summary>
        /// Modem storage area
        /// </summary>
        public static readonly MessageStorage ME = new MessageStorage("ME");
        /// <summary>
        /// All storage combined
        /// </summary>
        public static readonly MessageStorage MT = new MessageStorage("MT");
        /// <summary>
        /// Broadcast message storage area
        /// </summary>
        public static readonly MessageStorage BM = new MessageStorage("BM");
        /// <summary>
        /// Status report storage area
        /// </summary>
        public static readonly MessageStorage SR = new MessageStorage("SR");
        /// <summary>
        /// Terminal adaptor storage area
        /// </summary>
        public static readonly MessageStorage TA = new MessageStorage("TA");

        protected MessageStorage(string value)
        {
            Value = value;
        }

        public string Value { get; }

        protected static MessageStorage ParseString(string value)
        {
            if (value == SM.ToString())
                return SM;

            if (value == ME.ToString())
                return ME;

            if (value == MT.ToString())
                return MT;

            if (value == BM.ToString())
                return BM;

            if (value == SR.ToString())
                return SR;

            if (value == TA.ToString())
                return TA;

            throw new ArgumentException($"\"{value}\" is not supported");
        }

        public static implicit operator string(MessageStorage value) => value.Value;
        public static explicit operator MessageStorage(string value) => ParseString(value);

        public override string ToString()
        {
            return Value;
        }
    }
}
