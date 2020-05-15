﻿using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.MessageReader;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class PinCommands
    {
        public static async Task<ATResult> GetPinStatus(this ICommunicator<string> comm)
        {
            await comm.Write($"AT+CPIN?\r");
            var message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF);
            if (PinStatusParser.TryParseNumeric(message, out PinStatusResult pinResult))
            {
                message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF);
                if (OkParser.TryParseNumeric(message, out OkResult _))
                    return pinResult;
                else if (ErrorParser.TryParseNumeric(message, out ErrorResult errorResult))
                    return errorResult;
            }
            else if (ErrorParser.TryParseNumeric(message, out ErrorResult errorResult))
                return errorResult;
            return default;
        }
    }
}
