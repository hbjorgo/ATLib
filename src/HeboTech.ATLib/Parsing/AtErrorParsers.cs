using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Parsing
{
    public static class AtErrorParsers
    {
        /*
        * Returns error code from response
        * Assumes AT+CMEE=1 (numeric) mode
        */
        public static bool TryGetError(string response, out Error error)
        {
            Match errorMatch = Regex.Match(response, @"\+(?<errorType>[A-Z]{3})\sERROR:\s(?<errorCode>\d+)", RegexOptions.Compiled);
            if (errorMatch.Success)
            {
                string errorType = errorMatch.Groups["errorType"].Value;
                int errorCode = int.Parse(errorMatch.Groups["errorCode"].Value);
                switch (errorType)
                {
                    case "CME":
                        return CmeErrors.TryGetError(errorCode, out error);
                    case "CMS":
                        return CmsErrors.TryGetError(errorCode, out error);
                    default:
                        break;
                }
            }
            error = default;
            return false;
        }
    }

    public class CmeErrors
    {
        private static readonly Dictionary<int, string> mapping = new Dictionary<int, string>
        {
            { 0, "phone failure" },
            { 1, "no connection to phone" },
            { 2, "phone adaptor link reserved" },
            { 3, "operation not allowed" },
            { 4, "operation not supported" },
            { 5, "PH-SIM PIN required" },
            { 6, "PH-FSIM PIN required" },
            { 7, "PH-FSIM PUK required" },
            { 10, "SIM not inserted" },
            { 11, "SIM PIN required" },
            { 12, "SIM PUK required" },
            { 13, "SIM failure" },
            { 14, "SIM busy" },
            { 15, "SIM wrong" },
            { 16, "incorrect password" },
            { 17, "SIM PIN2 required" },
            { 18, "SIM PUK2 required" },
            { 20, "memory full" },
            { 21, "invalid index" },
            { 22, "not found" },
            { 23, "memory failure" },
            { 24, "text string too long" },
            { 25, "invalid characters in text string" },
            { 26, "dial string too long" },
            { 27, "invalid characters in dial string" },
            { 30, "no network service" },
            { 31, "network timeout" },
            { 32, "network not allowed - emergency calls only" },
            { 40, "network personalization PIN required" },
            { 41, "network personalization PUK required" },
            { 42, "network subset personalization PIN required" },
            { 43, "network subset personalization PUK required" },
            { 44, "service provider personalization PIN required" },
            { 45, "service provider personalization PUK required" },
            { 46, "corporate personalization PIN required" },
            { 47, "corporate personalization PUK required" },
            { 100, "Unknown" },
            { 103, "Illegal MESSAGE" },
            { 106, "Illegal ME" },
            { 107, "GPRS services not allowed" },
            { 111, "PLMN not allowed" },
            { 112, "Location area not allowed" },
            { 113, "Roaming not allowed in this location area" },
            { 132, "service option not supported" },
            { 133, "requested service option not subscribed" },
            { 134, "service option temporarily out of order" },
            { 148, "unspecified GPRS error" },
            { 149, "PDP authentication failure" },
            { 150, "invalid mobile class" },
            { 257, "network rejected request" },
            { 258, "retry operation" },
            { 259, "invalid deflected to number" },
            { 260, "deflected to own number" },
            { 261, "unknown subscriber" },
            { 262, "service not available" },
            { 263, "unknown class specified" },
            { 264, "unknown network message" },
            { 273, "minimum TFTS per PDP address violated" },
            { 274, "TFT precedence index not unique" },
            { 275, "invalid parameter combination" },

            // CME error codes for MMS
            { 170, "Unknown error for mms" },
            { 171, "MMS task is busy now" },
            { 172, "The mms data is over size" },
            { 173, "The operation is overtime" },
            { 174, "There is no mms receiver" },
            { 175, "The storage for address is full" },
            { 176, "Not find the address" },
            { 177, "Invalid parameter" },
            { 178, "Failed to read mss" },
            { 179, "There is not a mms push message" },
            { 180, "Memory error" },
            { 181, "Invalid file format" },
            { 182, "The mms storage is full" },
            { 183, "The box is empty" },
            { 184, "Failed to save mms" },
            { 185, "It’s busy editing mms now" },
            { 186, "It’s not allowed to edit now" },
            { 187, "No content in the buffer" },
            { 188, "Failed to receive mms" },
            { 189, "Invalid mms pdu" },
            { 190, "Network error" },
            { 191, "Failed to read file" },
            { 192, "None" },

            // CME error codes for FTP
            { 201, "Unknown error for FTP" },
            { 202, "FTP task is busy" },
            { 203, "Failed to resolve server address" },
            { 204, "FTP timeout" },
            { 205, "Failed to read file" },
            { 206, "Failed to write file" },
            { 207, "It’s not allowed in current state" },
            { 208, "Failed to login" },
            { 209, "Failed to logout" },
            { 210, "Failed to transfer data" },
            { 211, "FTP command rejected by server" },
            { 212, "Memory error" },
            { 213, "Invalid parameter" },
            { 214, "Network error" }
        };

        public static bool TryGetError(int errorCode, out Error error)
        {
            if (mapping.TryGetValue(errorCode, out string errorMessage))
            {
                error = new Error(errorCode, errorMessage);
                return true;
            }
            error = default;
            return false;
        }
    }

    public class CmsErrors
    {
        private static readonly Dictionary<int, string> mapping = new Dictionary<int, string>
        {
            { 300, "ME failure" },
            { 301, "SMS service of ME reserved" },
            { 302, "Operation not allowed" },
            { 303, "Operation not supported" },
            { 304, "Invalid PDU mode parameter" },
            { 305, "Invalid text mode parameter" },
            { 310, "SIM not inserted" },
            { 311, "SIM PIN required" },
            { 312, "PH-SIM PIN required" },
            { 313, "SIM failure" },
            { 314, "SIM busy " },
            { 315, "SIM wrong" },
            { 316, "SIM PUK required" },
            { 317, "SIM PIN2 required" },
            { 318, "SIM PUK2 required" },
            { 320, "Memory failure" },
            { 321, "Invalid memory index " },
            { 322, "Memory full " },
            { 330, "SMSC address unknown" },
            { 331, "no network service " },
            { 332, "Network timeout " },
            { 340, "NO +CNMA ACK EXPECTED" },
            { 341, "Buffer overflow" },
            { 342, "SMS size more than expected" },
            { 500, "unknown error" }
        };

        public static bool TryGetError(int errorCode, out Error error)
        {
            if (mapping.TryGetValue(errorCode, out string errorMessage))
            {
                error = new Error(errorCode, errorMessage);
                return true;
            }
            error = default;
            return false;
        }
    }

    public class Error
    {
        public Error(int errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public int ErrorCode { get; }
        public string ErrorMessage { get; }

        public override string ToString()
        {
            return $"Error code {ErrorCode} - {ErrorMessage}";
        }
    }
}
