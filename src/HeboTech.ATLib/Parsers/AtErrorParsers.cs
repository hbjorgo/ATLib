using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Parsers
{
    public static class AtErrorParsers
    {
        public enum AtCmeError
        {
            NO_CME_ERROR,
            PHONE_FAILURE = 0,
            NO_CONNECTION_TO_PHONE,

            CME_ERROR_SIM_FAILURE = 13
        }

        /*
        * Returns error code from response
        * Assumes AT+CMEE=1 (numeric) mode
        */
        public static CmeError GetCmeError(AtResponse response)
        {
            Match match = Regex.Match(response.FinalResponse, @"\+CME\sERROR:\s(?<error>\d+)");
            if (match.Success)
            {
                int cmeErrorCode = int.Parse(match.Groups["error"].Value);
                return CmeErrors.GetError(cmeErrorCode);
            }
            return default;
        }
    }

    public class CmeErrors
    {
        private static readonly Dictionary<int, string> mapping = new Dictionary<int, string>
        {
            { 0, "Phone failure" },
            { 1, "No connection to phone" },
            { 2, "Phone adaptor link reserved" },
            { 3, "Operation not allowed" },
            { 4, "Operation not supported" },
            { 5, "PH-SIM PIN required" },
            { 6, "PH-FSIM PIN required" },
            { 7, "PH-FSIM PUK required" },

            { 10, "SIM not inserted" },
            { 11, "SIM PIN required" },
            { 12, "SIM PUK required" },
            { 13, "SIM failure" },
            { 14, "SIM busy" },
            { 15, "SIM wrong" },
            { 16, "Incorrect password" },
            { 17, "SIM PIN2 required" },
            { 18, "SIM PUK2 required" },

            { 20, "Memory full" },
            { 21, "Invalid index" },
            { 22, "Not found" },
            { 23, "Memory failure" },
            { 24, "Text string too long" },
            { 25, "Invalid characters in text string" },
            { 26, "Dial string too long" },
            { 27, "Invalid characters in dial string" },

            { 30, "No network service" },
            { 31, "Network timeout" },
            { 32, "Network not allowed - emergency calls only" },

            { 40, "Network personalization PIN required" },
            { 41, "Network personalization PUK required" },
            { 42, "Network subset personalization PIN required" },
            { 43, "Network subset personalization PUK required" },
            { 44, "Service provider personalization PIN required" },
            { 45, "Service provider personalization PUK required" },
            { 46, "Corporate personalization PIN required" },
            { 47, "Corporate personalization PUK required" },

            { 100, "Unknown" },
            { 103, "Illegal MESSAGE" },
            { 106, "Illegal ME" },
            { 107, "GPRS services not allowed " },
            { 111, "PLMN not allowed " },
            { 112, "Location area not allowed " },
            { 113, "Roaming not allowed in this location area " },
            { 132, "service option not supported " },
            { 133, "requested service option not subscribed " },
            { 134, "service option temporarily out of order" },
            { 148, "unspecified GPRS error" },
            { 149, "PDP authentication failure" },
            { 150, "invalid mobile class" },

            { 257, "network rejected request" },
            { 258, "retry operation" },
            { 259, "invalid deflected to number " },
            { 260, "deflected to own number " },
            { 261, "unknown subscriber " },
            { 262, "service not available" },
            { 263, "unknown class specified" },
            { 264, "unknown network message" },
            { 273, "minimum TFTS per PDP address violated " },
            { 274, "TFT precedence index not unique " },
            { 275, "invalid parameter combination" },

            // "CME ERROR" codes for MMS
            { 170, "Unknown error for mms " },
            { 171, "MMS task is busy now " },
            { 172, "The mms data is over size" },
            { 173, "The operation is overtime " },
            { 174, "There is no mms receiver" },
            { 175, "The storage for address is full " },
            { 176, "Not find the address " },
            { 177, "Invalid parameter" },
            { 178, "Failed to read mss" },
            { 179, "There is not a mms push message " },
            { 180, "Memory error" },
            { 181, "Invalid file format" },
            { 182, "The mms storage is full " },
            { 183, "The box is empty " },
            { 184, "Failed to save mms" },
            { 185, "It’s busy editing mms now" },
            { 186, "It’s not allowed to edit now " },
            { 187, "No content in the buffer" },
            { 188, "Failed to receive mms" },
            { 189, "Invalid mms pdu" },
            { 190, "Network error" },
            { 191, "Failed to read file" },
            { 192, "None" },

            // "CME ERROR" codes for FTP
            { 201, "Unknown error for FTP " },
            { 202, "FTP task is busy" },
            { 203, "Failed to resolve server address" },
            { 204, "FTP timeout" },
            { 205, "Failed to read file" },
            { 206, "Failed to write file" },
            { 207, "It’s not allowed in current state" },
            { 208, "Failed to login" },
            { 209, "Failed to logout " },
            { 210, "Failed to transfer data" },
            { 211, "FTP command rejected by server" },
            { 212, "Memory error" },
            { 213, "Invalid parameter" },
            { 214, "Network error" }
        };

        public static CmeError GetError(int errorCode)
        {
            if (mapping.TryGetValue(errorCode, out string errorMessage))
                return new CmeError(errorCode, errorMessage);
            return default;
        }
    }

    public class CmeError
    {
        public CmeError(int errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public int ErrorCode { get; }
        public string ErrorMessage { get; }

        public override string ToString()
        {
            return $"{ErrorMessage} (Error code {ErrorCode})";
        }
    }
}
