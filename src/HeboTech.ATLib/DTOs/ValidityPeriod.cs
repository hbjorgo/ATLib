﻿using HeboTech.ATLib.PDU;
using System;

namespace HeboTech.ATLib.DTOs
{
    public enum ValidityPeriodFormat : byte
    {
        NotPresent = 0x00,
        Enhanced = 0x01,
        Relative = 0x02,
        Absolute = 0x03
    }

    public class ValidityPeriod
    {
        private ValidityPeriod(ValidityPeriodFormat format, byte[] value)
        {
            Format = format;
            Value = value;
        }

        public ValidityPeriodFormat Format { get; }
        public byte[] Value { get; }

        /// <summary>
        /// No validity period
        /// </summary>
        /// <returns></returns>
        public static ValidityPeriod NotPresent() => new ValidityPeriod(ValidityPeriodFormat.NotPresent, Array.Empty<byte>());

        /// <summary>
        /// A realative validity period.
        /// </summary>
        /// <param name="value">
        /// Value   0-143: (Value + 1) x 5 minutes. Possible values: 5, 10, 15 minutes ... 11:55, 12:00 hours
        /// Value 144-167: (12 + (Value - 143) / 2) hours. Possible values: 12:30, 13:00, ... 23:30, 24:00 hours
        /// Value 168-196: (Value - 166) days. Possible values: 2, 3, 4, ... 30 days
        /// Value 197-255: (Value - 192) weeks. Possible values: 5, 6, 7, ... 63 weeks
        /// </param>
        /// <returns></returns>
        public static ValidityPeriod Relative(byte value) => new ValidityPeriod(ValidityPeriodFormat.Relative, new byte[] { value });

        public static ValidityPeriod Relative(RelativeValidityPeriods value) => Relative((byte)value);

        /// <summary>
        /// An absolute validity period
        /// </summary>
        /// <param name="value">The date and time the validity expires</param>
        /// <returns></returns>
        public static ValidityPeriod Absolute(DateTimeOffset value)
        {
            byte[] encoded = TpduTime.EncodeTimestamp(value);
            return new ValidityPeriod(ValidityPeriodFormat.Absolute, encoded);
        }
    }

    public enum RelativeValidityPeriods
    {
        Minutes_5 = 0,
        Minutes_10 = 1,
        Minutes_15 = 2,
        Minutes_20 = 3,
        Minutes_25 = 4,
        Minutes_30 = 5,
        Minutes_35 = 6,
        Minutes_40 = 7,
        Minutes_45 = 8,
        Minutes_50 = 9,
        Minutes_55 = 10,
        Hours_1 = 11,
        Hours_1_Minutes_5 = 12,
        Hours_1_Minutes_10 = 13,
        Hours_1_Minutes_15 = 14,
        Hours_1_Minutes_20 = 15,
        Hours_1_Minutes_25 = 16,
        Hours_1_Minutes_30 = 17,
        Hours_1_Minutes_35 = 18,
        Hours_1_Minutes_40 = 19,
        Hours_1_Minutes_45 = 20,
        Hours_1_Minutes_50 = 21,
        Hours_1_Minutes_55 = 22,
        Hours_2 = 23,
        Hours_2_Minutes_5 = 24,
        Hours_2_Minutes_10 = 25,
        Hours_2_Minutes_15 = 26,
        Hours_2_Minutes_20 = 27,
        Hours_2_Minutes_25 = 28,
        Hours_2_Minutes_30 = 29,
        Hours_2_Minutes_35 = 30,
        Hours_2_Minutes_40 = 31,
        Hours_2_Minutes_45 = 32,
        Hours_2_Minutes_50 = 33,
        Hours_2_Minutes_55 = 34,
        Hours_3 = 35,
        Hours_3_Minutes_5 = 36,
        Hours_3_Minutes_10 = 37,
        Hours_3_Minutes_15 = 38,
        Hours_3_Minutes_20 = 39,
        Hours_3_Minutes_25 = 40,
        Hours_3_Minutes_30 = 41,
        Hours_3_Minutes_35 = 42,
        Hours_3_Minutes_40 = 43,
        Hours_3_Minutes_45 = 44,
        Hours_3_Minutes_50 = 45,
        Hours_3_Minutes_55 = 46,
        Hours_4 = 47,
        Hours_4_Minutes_5 = 48,
        Hours_4_Minutes_10 = 49,
        Hours_4_Minutes_15 = 50,
        Hours_4_Minutes_20 = 51,
        Hours_4_Minutes_25 = 52,
        Hours_4_Minutes_30 = 53,
        Hours_4_Minutes_35 = 54,
        Hours_4_Minutes_40 = 55,
        Hours_4_Minutes_45 = 56,
        Hours_4_Minutes_50 = 57,
        Hours_4_Minutes_55 = 58,
        Hours_5 = 59,
        Hours_5_Minutes_5 = 60,
        Hours_5_Minutes_10 = 61,
        Hours_5_Minutes_15 = 62,
        Hours_5_Minutes_20 = 63,
        Hours_5_Minutes_25 = 64,
        Hours_5_Minutes_30 = 65,
        Hours_5_Minutes_35 = 66,
        Hours_5_Minutes_40 = 67,
        Hours_5_Minutes_45 = 68,
        Hours_5_Minutes_50 = 69,
        Hours_5_Minutes_55 = 70,
        Hours_6 = 71,
        Hours_6_Minutes_5 = 72,
        Hours_6_Minutes_10 = 73,
        Hours_6_Minutes_15 = 74,
        Hours_6_Minutes_20 = 75,
        Hours_6_Minutes_25 = 76,
        Hours_6_Minutes_30 = 77,
        Hours_6_Minutes_35 = 78,
        Hours_6_Minutes_40 = 79,
        Hours_6_Minutes_45 = 80,
        Hours_6_Minutes_50 = 81,
        Hours_6_Minutes_55 = 82,
        Hours_7 = 83,
        Hours_7_Minutes_5 = 84,
        Hours_7_Minutes_10 = 85,
        Hours_7_Minutes_15 = 86,
        Hours_7_Minutes_20 = 87,
        Hours_7_Minutes_25 = 88,
        Hours_7_Minutes_30 = 89,
        Hours_7_Minutes_35 = 90,
        Hours_7_Minutes_40 = 91,
        Hours_7_Minutes_45 = 92,
        Hours_7_Minutes_50 = 93,
        Hours_7_Minutes_55 = 94,
        Hours_8 = 95,
        Hours_8_Minutes_5 = 96,
        Hours_8_Minutes_10 = 97,
        Hours_8_Minutes_15 = 98,
        Hours_8_Minutes_20 = 99,
        Hours_8_Minutes_25 = 100,
        Hours_8_Minutes_30 = 101,
        Hours_8_Minutes_35 = 102,
        Hours_8_Minutes_40 = 103,
        Hours_8_Minutes_45 = 104,
        Hours_8_Minutes_50 = 105,
        Hours_8_Minutes_55 = 106,
        Hours_9 = 107,
        Hours_9_Minutes_5 = 108,
        Hours_9_Minutes_10 = 109,
        Hours_9_Minutes_15 = 110,
        Hours_9_Minutes_20 = 111,
        Hours_9_Minutes_25 = 112,
        Hours_9_Minutes_30 = 113,
        Hours_9_Minutes_35 = 114,
        Hours_9_Minutes_40 = 115,
        Hours_9_Minutes_45 = 116,
        Hours_9_Minutes_50 = 117,
        Hours_9_Minutes_55 = 118,
        Hours_10 = 119,
        Hours_10_Minutes_5 = 120,
        Hours_10_Minutes_10 = 121,
        Hours_10_Minutes_15 = 122,
        Hours_10_Minutes_20 = 123,
        Hours_10_Minutes_25 = 124,
        Hours_10_Minutes_30 = 125,
        Hours_10_Minutes_35 = 126,
        Hours_10_Minutes_40 = 127,
        Hours_10_Minutes_45 = 128,
        Hours_10_Minutes_50 = 129,
        Hours_10_Minutes_55 = 130,
        Hours_11 = 131,
        Hours_11_Minutes_5 = 132,
        Hours_11_Minutes_10 = 133,
        Hours_11_Minutes_15 = 134,
        Hours_11_Minutes_20 = 135,
        Hours_11_Minutes_25 = 136,
        Hours_11_Minutes_30 = 137,
        Hours_11_Minutes_35 = 138,
        Hours_11_Minutes_40 = 139,
        Hours_11_Minutes_45 = 140,
        Hours_11_Minutes_50 = 141,
        Hours_11_Minutes_55 = 142,
        Hours_12 = 143,
        Hours_12_Minutes_30 = 144,
        Hours_13 = 145,
        Hours_13_Minutes_30 = 146,
        Hours_14 = 147,
        Hours_14_Minutes_30 = 148,
        Hours_15 = 149,
        Hours_15_Minutes_30 = 150,
        Hours_16 = 151,
        Hours_16_Minutes_30 = 152,
        Hours_17 = 153,
        Hours_17_Minutes_30 = 154,
        Hours_18 = 155,
        Hours_18_Minutes_30 = 156,
        Hours_19 = 157,
        Hours_19_Minutes_30 = 158,
        Hours_20 = 159,
        Hours_20_Minutes_30 = 160,
        Hours_21 = 161,
        Hours_21_Minutes_30 = 162,
        Hours_22 = 163,
        Hours_22_Minutes_30 = 164,
        Hours_23 = 165,
        Hours_23_Minutes_30 = 166,
        Hours_24 = 167,
        Days_2 = 168,
        Days_3 = 169,
        Days_4 = 170,
        Days_5 = 171,
        Days_6 = 172,
        Days_7 = 173,
        Days_8 = 174,
        Days_9 = 175,
        Days_10 = 176,
        Days_11 = 177,
        Days_12 = 178,
        Days_13 = 179,
        Days_14 = 180,
        Days_15 = 181,
        Days_16 = 182,
        Days_17 = 183,
        Days_18 = 184,
        Days_19 = 185,
        Days_20 = 186,
        Days_21 = 187,
        Days_22 = 188,
        Days_23 = 189,
        Days_24 = 190,
        Days_25 = 191,
        Days_26 = 192,
        Days_27 = 193,
        Days_28 = 194,
        Days_29 = 195,
        Days_30 = 196,
        Weeks_5 = 197,
        Weeks_6 = 198,
        Weeks_7 = 199,
        Weeks_8 = 200,
        Weeks_9 = 201,
        Weeks_10 = 202,
        Weeks_11 = 203,
        Weeks_12 = 204,
        Weeks_13 = 205,
        Weeks_14 = 206,
        Weeks_15 = 207,
        Weeks_16 = 208,
        Weeks_17 = 209,
        Weeks_18 = 210,
        Weeks_19 = 211,
        Weeks_20 = 212,
        Weeks_21 = 213,
        Weeks_22 = 214,
        Weeks_23 = 215,
        Weeks_24 = 216,
        Weeks_25 = 217,
        Weeks_26 = 218,
        Weeks_27 = 219,
        Weeks_28 = 220,
        Weeks_29 = 221,
        Weeks_30 = 222,
        Weeks_31 = 223,
        Weeks_32 = 224,
        Weeks_33 = 225,
        Weeks_34 = 226,
        Weeks_35 = 227,
        Weeks_36 = 228,
        Weeks_37 = 229,
        Weeks_38 = 230,
        Weeks_39 = 231,
        Weeks_40 = 232,
        Weeks_41 = 233,
        Weeks_42 = 234,
        Weeks_43 = 235,
        Weeks_44 = 236,
        Weeks_45 = 237,
        Weeks_46 = 238,
        Weeks_47 = 239,
        Weeks_48 = 240,
        Weeks_49 = 241,
        Weeks_50 = 242,
        Weeks_51 = 243,
        Weeks_52 = 244,
        Weeks_53 = 245,
        Weeks_54 = 246,
        Weeks_55 = 247,
        Weeks_56 = 248,
        Weeks_57 = 249,
        Weeks_58 = 250,
        Weeks_59 = 251,
        Weeks_60 = 252,
        Weeks_61 = 253,
        Weeks_62 = 254,
        Weeks_63 = 255
    }
}
