using HeboTech.ATLib.CodingSchemes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeboTech.ATLib.PDU
{
    internal class UserData
    {
        private readonly CodingScheme dataCodingScheme;
        private int udhLength = 0;
        private List<string> triplets = new List<string>();
        private string userData;

        protected UserData(CodingScheme dataCodingScheme)
        {
            this.dataCodingScheme = dataCodingScheme;
        }

        public static UserData Initialize(CodingScheme dataCodingScheme)
        {
            return new UserData(dataCodingScheme);
        }

        public UserData AddTriplet(int tag, params int[] values)
        {
            string triplet = $"{tag.ToString("X2")}{values.Length.ToString("X2")}{string.Join("", values.Select(x => x.ToString("X2")))}";
            triplets.Add(triplet);
            udhLength += 2 + values.Length; // IEI + IE Length + Values
            return this;
        }

        public UserData AddUserData(string userData)
        {
            this.userData = userData;
            return this;
        }

        public int GetUserDataLength()
        {
            if (udhLength == 0)
            {
                switch (dataCodingScheme)
                {
                    case CodingScheme.Gsm7:
                        return ((userData.Length / 2) * 8) / 7;
                    case CodingScheme.UCS2:
                        return userData.Length / 2;
                    default:
                        throw new ArgumentException($"{nameof(dataCodingScheme)} with value {dataCodingScheme} is not supported");
                }
            }
            else
            {
                switch (dataCodingScheme)
                {
                    case CodingScheme.Gsm7:
                        return (((1 + udhLength) * 8) + ((userData.Length / 2) * 8)) / 7;
                    case CodingScheme.UCS2:
                        return (1 + udhLength) + (userData.Length / 2);
                    default:
                        throw new ArgumentException($"{nameof(dataCodingScheme)} with value {dataCodingScheme} is not supported");
                }
            }
        }

        public string Build()
        {
            if (udhLength == 0)
            {
                // UD
                return $"{userData}";
            }
            else
            {
                // UDH Length, UDH, UD
                return $"{udhLength.ToString("X2")}{string.Join("", triplets)}{userData}";
            }
        }

        public override string ToString()
        {
            return Build();
        }
    }
}
