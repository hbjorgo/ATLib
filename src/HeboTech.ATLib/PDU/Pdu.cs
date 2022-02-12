using System;

namespace HeboTech.ATLib.PDU
{
    public class Pdu
    {
        public static PduMessage Decode(string data, int timestampYearOffset = 2000)
        {
            int offset = 0;
            int smscLength = Convert.ToInt32(data[offset..(offset += 2)], 16);
            int smscAddressType = Convert.ToInt32(data[offset..(offset += 2)], 16);
            string serviceCenterNumber = null;
            // National
            if (smscAddressType == 0x81)
            {

            }
            // International
            else if (smscAddressType == 0x91)
            {
                serviceCenterNumber += '+';
                for (int i = 0; i < smscLength - 1; i++)
                {
                    string temp = data[offset..(offset += 2)];
                    serviceCenterNumber += temp[1];
                    if (temp[0] != 'F')
                        serviceCenterNumber += temp[0];
                }
            }

            int firstSmsDeliverOctet = Convert.ToInt32(data[offset..(offset += 2)], 16);
            int senderAddressLength = Convert.ToInt32(data[offset..(offset += 2)], 16);
            int senderAddressType = Convert.ToInt32(data[offset..(offset += 2)], 16);
            string senderNumber = null;
            // TODO: What types are there here?
            if (senderAddressType == 0xC8 || senderAddressType == 0x91)
            {
                senderNumber += '+';
                for (int i = 0; i < (int)Math.Ceiling(senderAddressLength / 2f); i++)
                {
                    string temp = data[offset..(offset += 2)];
                    senderNumber += temp[1];
                    if (temp[0] != 'F')
                        senderNumber += temp[0];
                }
            }

            int tpPid = Convert.ToInt32(data[offset..(offset += 2)], 16);
            int tpDcs = Convert.ToInt32(data[offset..(offset += 2)], 16);
            string tpScts = null;
            for (int i = 0; i < 7; i++)
            {
                string temp = data[offset..(offset += 2)];
                tpScts += temp[1];
                tpScts += temp[0];
            }
            DateTimeOffset timeStamp = new DateTimeOffset(
                int.Parse(tpScts[..2]) + timestampYearOffset,
                int.Parse(tpScts[2..4]),
                int.Parse(tpScts[4..6]),
                int.Parse(tpScts[6..8]),
                int.Parse(tpScts[8..10]),
                int.Parse(tpScts[10..12]),
                TimeSpan.FromHours(int.Parse(tpScts[12..14])));
            int tpUdl = Convert.ToInt32(data[offset..(offset += 2)], 16);
            string tpUd = data[offset..];
            string message = Gsm7.Decode(tpUd);

            return new PduMessage(serviceCenterNumber, senderNumber, message, timeStamp);
        }
    }

    public class PduMessage
    {
        public PduMessage(string serviceCenterNumber, string senderNumber, string message, DateTimeOffset timestamp)
        {
            ServiceCenterNumber = serviceCenterNumber;
            SenderNumber = senderNumber;
            Message = message;
            Timestamp = timestamp;
        }

        public string ServiceCenterNumber { get; }
        public string SenderNumber { get; }
        public string Message { get; }
        public DateTimeOffset Timestamp { get; }
    }
}
