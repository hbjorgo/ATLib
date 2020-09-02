namespace HeboTech.ATLib.Parsers
{
    public static class AtErrorParsers
    {
        public enum AtCmeError
        {
            CME_ERROR_NON_CME = -1,
            CME_SUCCESS = 0,
            CME_SIM_NOT_INSERTED = 10
        }

        /*
        * Returns error code from response
        * Assumes AT+CMEE=1 (numeric) mode
        */
        public static AtCmeError GetCmeError(AtResponse response)
        {
            if (response.Success)
            {
                return AtCmeError.CME_SUCCESS;
            }

            if (response.FinalResponse == null || !response.FinalResponse.StartsWith("+CME ERROR:"))
            {
                return AtCmeError.CME_ERROR_NON_CME;
            }

            if (!AtTokenizer.TokenizeStart(response.FinalResponse, out string tokenizerResponse))
            {
                return AtCmeError.CME_ERROR_NON_CME;
            }

            if (!AtTokenizer.TokenizeNextInt(tokenizerResponse, out _, out int ret))
            {
                return AtCmeError.CME_ERROR_NON_CME;
            }

            return (AtCmeError)ret;
        }
    }
}
