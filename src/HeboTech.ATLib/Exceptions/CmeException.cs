using System;

namespace HeboTech.ATLib.Exceptions
{
    public class CmeException : Exception
    {
        public CmeException(string message) : base(message)
        {
        }
    }
}
