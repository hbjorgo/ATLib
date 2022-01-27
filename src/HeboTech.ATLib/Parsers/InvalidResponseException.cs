using System;

namespace HeboTech.ATLib.Parsers
{
    public class InvalidResponseException : Exception
    {
        public InvalidResponseException()
        {
        }

        public InvalidResponseException(string message) : base(message)
        {
        }
    }
}
