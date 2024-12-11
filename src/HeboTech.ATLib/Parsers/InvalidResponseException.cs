using System;

namespace HeboTech.ATLib.Parsers
{
    internal class InvalidResponseException : Exception
    {
        public InvalidResponseException()
        {
        }

        public InvalidResponseException(string message) : base(message)
        {
        }
    }
}
