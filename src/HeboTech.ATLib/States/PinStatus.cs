using System.ComponentModel;

namespace HeboTech.ATLib.States
{
    public enum PinStatus
    {
        [Description("Not pending any password")]
        READY,
        [Description("Waiting for SIM PIN to be given")]
        SIM_PIN,
        [Description("Waiting for SIM PUK to be given")]
        SIM_PUK,
        [Description("Waiting for Phone-To-SIM card password (antitheft) to be given")]
        PH_SIM_PIN,
        [Description("Waiting for Phone-To-very-First-SIM card password to be given")]
        PH_FSIM_PIN,
        [Description("Waiting for Phone-To-very-First-SIM card unblocking password to be given")]
        PH_FSIM_PUK,
        [Description("Waiting for SIM PIN2 to be given")]
        SIM_PIN2,
        [Description("Waiting for SIM PUK2 to be given")]
        SIM_PUK2,
        [Description("Waiting for network personalization password to be given")]
        PH_NET_PIN,
        [Description("Waiting for network personalization unblocking password to be given")]
        PH_NET_PUK,
        [Description("Waiting for network subset personalization password to be given")]
        PH_NETSUB_PIN,
        [Description("Waiting for network subset personalization unblocking password to be given")]
        PH_NETSUB_PUK,
        [Description("Waiting for service provider personalization password to be given")]
        PH_SP_PIN,
        [Description("Waiting for service provider personalization unblocking password to be given")]
        PH_SP_PUK,
        [Description("Waiting for corporate personalization password to be given")]
        PH_CORP_PIN,
        [Description("Waiting for corporate personalization unblocking password to be given")]
        PH_CORP_PUK
    }
}
