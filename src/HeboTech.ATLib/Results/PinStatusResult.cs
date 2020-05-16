using HeboTech.ATLib.States;
using System;

namespace HeboTech.ATLib.Results
{
    public class PinStatusResult
    {
        public PinStatusResult(string status)
        {
            Status = GetPinStatus(status);
        }

        private PinStatus GetPinStatus(string status)
        {
            return status switch
            {
                "READY" => PinStatus.READY,
                "SIM PIN" => PinStatus.SIM_PIN,
                "SIM PUK" => PinStatus.SIM_PUK,
                "PH-SIM PIN" => PinStatus.PH_SIM_PIN,
                "PH-FSIM PIN" => PinStatus.PH_FSIM_PIN,
                "PH-FSIM PUK" => PinStatus.PH_FSIM_PUK,
                "SIM PIN2" => PinStatus.SIM_PIN2,
                "SIM PUK2" => PinStatus.SIM_PUK2,
                "PH-NET PIN" => PinStatus.PH_NET_PIN,
                "PH-NET PUK" => PinStatus.PH_NET_PUK,
                "PH-NETSUB PIN" => PinStatus.PH_NETSUB_PIN,
                "PH-NETSUB PUK" => PinStatus.PH_NETSUB_PUK,
                "PH-SP PIN" => PinStatus.PH_SP_PIN,
                "PH-SP PUK" => PinStatus.PH_SP_PUK,
                "PH-CORP PIN" => PinStatus.PH_CORP_PIN,
                "PH-CORP PUK" => PinStatus.PH_CORP_PUK,
                _ => throw new ArgumentException(nameof(status)),
            };
        }

        public PinStatus Status { get; }

        public override string ToString()
        {
            return Status.GetDescription();
        }
    }
}
