namespace HeboTech.ATLib.Messaging
{
    /// <summary>
    /// Bits 0..6. Bit 7 is reserved.
    /// </summary>
    public enum SmsDeliveryStatus : byte
    {
        // Suffix _1, _2 and _3 are used to separate identical names, one suffix for each 'group' of messages

        // Transaction completed
        Message_received_by_SME = 0b0000_0000,
        Forwarded_to_SME_but_unconfirmed_delivery = 0b0000_0001,
        Message_replaced_by_the_SC = 0b0000_0010,

        // 000_0011..000_1111 Reserved
        // 001_0000..0011111 Values specific to each SC

        // Temporary error, SC still trying to transfer to SM
        Congestion_1 = 0b0010_0000,
        SME_busy_1 = 0b0010_0001,
        Service_rejected_1 = 0b0010_0011,
        Quality_of_service_not_available_1 = 0b0010_0100,
        Error_in_SME_1 = 0b0010_0101,

        // 0100110..0101111 Reserved
        // 0110000..0111111 Values specific to each SC

        // Permanent error, SC is not making any more transfer attempts
        Remote_procedure_error = 0b0100_0000,
        Incompatible_destination = 0b0100_0001,
        Connection_rejected_by_SME = 0b0100_0010,
        Not_obtainable = 0b0100_0011,
        Quality_of_service_not_available_2 = 0b0100_0100,
        No_interworking_available = 0b0100_0101,
        SM_validity_period_expired = 0b0100_0110,
        SM_deleted_by_originating_SME = 0b0100_0111,
        SM_deleted_by_SC_administration = 0b0100_1000,
        SM_does_not_exist = 0b0100_1001,

        // 1001010..1001111 Reserved
        // 1010000..1011111 Values specific to each SC

        // Temporary error, SC is not making any more transfer attempts
        Congestion_3 = 0b0110_0000,
        SME_busy_3 = 0b0110_0001,
        No_response_from_SME = 0b0110_0010,
        Service_rejected_3 = 0b0110_0011,
        Quality_of_service_not_available_3 = 0b0110_0100,
        Error_in_SME_3 = 0b0110_0101,

        //1100110..1101001	Reserved
        //1101010..1101111	Reserved
        //1110000..1111111	Values specific to each SC
    }
}
