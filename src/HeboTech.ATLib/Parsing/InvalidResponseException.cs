using System;

namespace HeboTech.ATLib.Parsing
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
