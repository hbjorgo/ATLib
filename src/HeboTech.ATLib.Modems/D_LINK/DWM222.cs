﻿using HeboTech.ATLib.Modems.Qualcomm;
using HeboTech.ATLib.Parsers;

namespace HeboTech.ATLib.Modems.D_LINK
{
    public class DWM222 : MDM9225, IModem, IDWM222
    {
        /// <summary>
        /// Based on Qualcomm MDM9225 chipset
        /// 
        /// Serial port settings:
        /// 9600 8N1 Handshake.RequestToSend
        /// </summary>
        public DWM222(IAtChannel channel)
            : base(channel)
        {
        }
    }
}
