using System;

namespace HeboTech.ATLib.Parsers
{
    internal class AtCommand
    {
        public AtCommand(AtCommandType commandType, string command, string responsePrefix, string smsPdu, TimeSpan timeout)
        {
            CommandType = commandType;
            Command = command;
            ResponsePrefix = responsePrefix;
            SmsPdu = smsPdu;
            Timeout = timeout;
        }

        public AtCommandType CommandType { get; }
        public string Command { get; }
        public string ResponsePrefix { get; }
        public string SmsPdu { get; set; }
        public TimeSpan Timeout { get; }
    }
}
