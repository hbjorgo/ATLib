using System;

namespace HeboTech.ATLib.DTOs
{
    public class CallDetails
    {
        public CallDetails(TimeSpan duration)
        {
            Duration = duration;
        }

        public TimeSpan Duration { get; }

        public override string ToString()
        {
            return $"Duration: {Duration}";
        }
    }
}
