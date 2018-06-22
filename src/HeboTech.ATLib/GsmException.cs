using System;

namespace HeboTech.ATLib
{
    public class GsmException : Exception
    {
        public GsmException(string message) : base(message)
        {
        }

        public GsmException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
